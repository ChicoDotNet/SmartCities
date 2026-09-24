#!/usr/bin/env python3

import base64
import hashlib
import hmac
import json
import os
import time
import urllib.error
import urllib.request

BASE_URL = os.environ.get(
    "SMARTCITIES_API_BASE_URL",
    "http://127.0.0.1:5000",
).rstrip("/")


def request(
    method: str,
    path: str,
    payload: dict | None = None,
    extra_headers: dict[str, str] | None = None,
) -> tuple[int, dict]:
    data = None
    headers = {
        "Accept": "application/json",
        "Accept-Language": "es-MX",
    }

    if extra_headers is not None:
        headers.update(extra_headers)

    if payload is not None:
        data = json.dumps(payload).encode("utf-8")
        headers["Content-Type"] = "application/json"

    req = urllib.request.Request(
        f"{BASE_URL}{path}",
        data=data,
        headers=headers,
        method=method,
    )

    try:
        with urllib.request.urlopen(req, timeout=5) as response:
            body = response.read().decode("utf-8")
            return response.status, json.loads(body) if body else {}
    except urllib.error.HTTPError as exc:
        body = exc.read().decode("utf-8")

        try:
            parsed = json.loads(body) if body else {}
        except json.JSONDecodeError:
            parsed = {"raw": body}

        return exc.code, parsed


def base64url(value: bytes) -> str:
    return base64.urlsafe_b64encode(value).rstrip(b"=").decode("ascii")


def create_reviewer_token() -> str:
    now = int(time.time())
    header = {
        "alg": "HS256",
        "typ": "JWT",
    }
    payload = {
        "iss": os.environ["SMARTCITIES_E2E_JWT_ISSUER"],
        "aud": os.environ["SMARTCITIES_E2E_JWT_AUDIENCE"],
        "sub": "e2e-reviewer-001",
        "role": "mobility-reviewer",
        "permission": "decision-review.finalize",
        "iat": now,
        "nbf": now - 5,
        "exp": now + 300,
    }
    signing_input = (
        f"{base64url(json.dumps(header, separators=(',', ':')).encode('utf-8'))}."
        f"{base64url(json.dumps(payload, separators=(',', ':')).encode('utf-8'))}"
    )
    signature = hmac.new(
        os.environ["SMARTCITIES_E2E_JWT_SIGNING_KEY"].encode("utf-8"),
        signing_input.encode("ascii"),
        hashlib.sha256,
    ).digest()
    return f"{signing_input}.{base64url(signature)}"


def recommendation_id_for_case(case_id: str) -> str:
    digest = hashlib.sha256(case_id.encode("utf-8")).hexdigest().upper()
    return f"mock:mobility:{digest}"


def wait_until_ready() -> None:
    deadline = time.monotonic() + 60

    while time.monotonic() < deadline:
        try:
            status, _ = request(
                "GET",
                "/health/live",
            )
            if status == 200:
                return
        except (OSError, urllib.error.URLError):
            pass

        time.sleep(1)

    raise RuntimeError("SmartCities.Api did not become ready within 60 seconds.")


def require(condition: bool, message: str) -> None:
    if not condition:
        raise AssertionError(message)


def main() -> None:
    wait_until_ready()

    live_status, live = request(
        "GET",
        "/health/live",
    )
    require(live_status == 200, f"Expected live 200, got {live_status}: {live}")
    require(live["status"] == "Healthy", "Liveness is not healthy.")
    require(live["checks"] == {}, "Liveness unexpectedly depends on readiness checks.")

    ready_status, ready = request(
        "GET",
        "/health/ready",
    )
    require(ready_status == 200, f"Expected ready 200, got {ready_status}: {ready}")
    require(ready["status"] == "Healthy", "Readiness is not healthy.")
    require(ready["checks"]["database"] == "Healthy", "Database readiness check is not healthy.")

    build_status, build = request(
        "GET",
        "/api/system/build",
    )
    require(build_status == 200, f"Expected build 200, got {build_status}: {build}")
    require(build["serviceName"] == "SmartCities.Api", "Unexpected build service name.")
    require(bool(build["version"]), "Build version is empty.")
    require(
        build["commitSha"] == os.environ["SMARTCITIES_EXPECTED_COMMIT"],
        "Build commit does not match the running CI revision.",
    )
    require(
        build["buildId"] == os.environ["SMARTCITIES_EXPECTED_BUILD_ID"],
        "Build ID does not match the running CI job.",
    )

    openapi_status, openapi = request(
        "GET",
        "/openapi/v1.json",
    )
    require(openapi_status == 200, f"Expected OpenAPI 200, got {openapi_status}: {openapi}")
    require(
        "/api/citizen/mobility-reports" in openapi["paths"],
        "OpenAPI does not expose the citizen mobility POST route.",
    )
    require(
        "/api/citizen/mobility-reports/{reportId}/outcome"
        in openapi["paths"],
        "OpenAPI does not expose the citizen outcome route.",
    )

    report_id = "e2e-report-001"
    original_case_id = "e2e-case-original"
    payload = {
        "reportId": report_id,
        "caseId": original_case_id,
        "categoryKey": "pedestrian-safety",
        "locationReference": "E2E intersection",
        "description": "E2E unsafe pedestrian crossing.",
        "evidenceReferences": [],
    }

    created_status, created = request(
        "POST",
        "/api/citizen/mobility-reports",
        payload,
    )
    require(created_status == 201, f"Expected 201, got {created_status}: {created}")
    require(created["reportId"] == report_id, "Created report ID changed.")
    require(created["caseId"] == original_case_id, "Created case ID changed.")
    require(created["wasCreated"] is True, "First submission was not reported as created.")

    replay_payload = dict(payload)
    replay_payload["caseId"] = "e2e-case-ignored"

    replay_status, replay = request(
        "POST",
        "/api/citizen/mobility-reports",
        replay_payload,
    )
    require(replay_status == 200, f"Expected replay 200, got {replay_status}: {replay}")
    require(replay["caseId"] == original_case_id, "Replay replaced the authoritative case.")
    require(replay["wasCreated"] is False, "Replay was incorrectly reported as created.")

    recovered_status, recovered = request(
        "GET",
        f"/api/citizen/mobility-reports/{report_id}",
    )
    require(recovered_status == 200, f"Expected recovery 200, got {recovered_status}: {recovered}")
    require(recovered["reportId"] == report_id, "Recovered report ID changed.")
    require(recovered["caseId"] == original_case_id, "Recovered case is not authoritative.")

    pending_status, pending_outcome = request(
        "GET",
        f"/api/citizen/mobility-reports/{report_id}/outcome",
    )
    require(
        pending_status == 200,
        f"Expected pending outcome 200, got {pending_status}: {pending_outcome}",
    )
    require(
        pending_outcome["reportId"] == report_id,
        "Pending outcome report ID changed.",
    )
    require(
        pending_outcome["caseId"] == original_case_id,
        "Pending outcome case ID changed.",
    )
    require(
        pending_outcome["status"] == "pending-human-review",
        "Pending outcome status is not human-review pending.",
    )
    require(
        pending_outcome["disposition"] is None,
        "Pending outcome exposed a final disposition.",
    )
    require(
        pending_outcome["statusLabel"] == "En revisión humana",
        "Pending outcome is not localized to es-MX.",
    )
    require(
        "persona autorizada"
        in pending_outcome["explanation"].lower(),
        "Pending explanation is not citizen-facing es-MX copy.",
    )

    recommendation_id = recommendation_id_for_case(original_case_id)
    finalize_status, finalized = request(
        "POST",
        f"/api/human-oversight/decision-reviews/{recommendation_id}/finalize",
        {
            "authorityRole": "mobility-reviewer",
            "disposition": 0,
        },
        {
            "Authorization": f"Bearer {create_reviewer_token()}",
        },
    )
    require(
        finalize_status == 200,
        f"Expected decision review finalize 200, got {finalize_status}: {finalized}",
    )
    require(
        finalized["recommendationId"] == recommendation_id,
        "Finalized recommendation ID changed.",
    )
    require(
        finalized["evidenceCaseId"] == original_case_id,
        "Finalized review lost the authoritative Evidence Case.",
    )
    require(
        finalized["authoritySubjectId"] == "local:default:e2e-reviewer-001",
        "Finalized review lost canonical human authority.",
    )
    require(
        finalized["authorityRole"] == "mobility-reviewer",
        "Finalized review lost canonical authority role.",
    )
    require(
        finalized["disposition"] == 0,
        "Finalized review did not preserve the human disposition.",
    )

    finalized_status, finalized_outcome = request(
        "GET",
        f"/api/citizen/mobility-reports/{report_id}/outcome",
    )
    require(
        finalized_status == 200,
        (
            "Expected finalized citizen outcome 200, got "
            f"{finalized_status}: {finalized_outcome}"
        ),
    )
    require(
        finalized_outcome["status"] == "finalized",
        "Citizen outcome did not become finalized.",
    )
    require(
        finalized_outcome["disposition"] == "accepted",
        "Citizen outcome did not expose the authoritative human disposition.",
    )
    require(
        finalized_outcome["statusLabel"] == "Revisión concluida",
        "Finalized outcome is not localized to es-MX.",
    )
    require(
        "aceptada" in finalized_outcome["explanation"].lower(),
        "Finalized es-MX explanation does not describe the disposition.",
    )

    english_status, english_outcome = request(
        "GET",
        f"/api/citizen/mobility-reports/{report_id}/outcome",
        extra_headers={"Accept-Language": "en"},
    )
    require(
        english_status == 200,
        f"Expected English outcome 200, got {english_status}: {english_outcome}",
    )
    require(
        english_outcome["reportId"] == finalized_outcome["reportId"],
        "Locale switch changed report identity.",
    )
    require(
        english_outcome["caseId"] == finalized_outcome["caseId"],
        "Locale switch changed case identity.",
    )
    require(
        english_outcome["status"] == finalized_outcome["status"],
        "Locale switch changed machine status.",
    )
    require(
        english_outcome["disposition"] == finalized_outcome["disposition"],
        "Locale switch changed human disposition.",
    )
    require(
        english_outcome["statusLabel"] == "Review complete",
        "English outcome status label is incorrect.",
    )

    fallback_status, fallback_outcome = request(
        "GET",
        f"/api/citizen/mobility-reports/{report_id}/outcome",
        extra_headers={"Accept-Language": "fr-FR"},
    )
    require(
        fallback_status == 200,
        (
            "Expected fallback outcome 200, got "
            f"{fallback_status}: {fallback_outcome}"
        ),
    )
    require(
        fallback_outcome["statusLabel"] == "Review complete",
        "Unsupported locale did not fall back to neutral English.",
    )

    post_finalize_replay_status, post_finalize_replay = request(
        "POST",
        "/api/citizen/mobility-reports",
        replay_payload,
    )
    require(
        post_finalize_replay_status == 200,
        (
            "Expected post-finalization replay 200, got "
            f"{post_finalize_replay_status}: {post_finalize_replay}"
        ),
    )
    require(
        post_finalize_replay["caseId"] == original_case_id,
        "Post-finalization replay replaced the authoritative case.",
    )

    conflict_status, conflict = request(
        "POST",
        f"/api/human-oversight/decision-reviews/{recommendation_id}/finalize",
        {
            "authorityRole": "mobility-reviewer",
            "disposition": 2,
        },
        {
            "Authorization": f"Bearer {create_reviewer_token()}",
        },
    )
    require(
        conflict_status == 409,
        f"Expected second finalization 409, got {conflict_status}: {conflict}",
    )
    require(
        conflict["authoritySubjectId"] == "local:default:e2e-reviewer-001",
        "Replay changed the original canonical human authority.",
    )
    require(
        conflict["disposition"] == 0,
        "Replay changed the original human disposition.",
    )

    print(
        "Local vertical slice verified: live -> ready -> build -> OpenAPI "
        "-> create -> replay -> recover -> pending citizen outcome "
        "-> mock criterion -> human finalize -> localized reviewed outcome "
        "-> post-finalization replay preserves authority."
    )


if __name__ == "__main__":
    main()
