#!/usr/bin/env python3

import json
import os
import time
import urllib.error
import urllib.request

BASE_URL = os.environ.get(
    "SMARTCITIES_API_BASE_URL",
    "http://127.0.0.1:5000",
).rstrip("/")


def request(method: str, path: str, payload: dict | None = None) -> tuple[int, dict]:
    data = None
    headers = {
        "Accept": "application/json",
        "Accept-Language": "es-MX",
    }

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

    print("Local vertical slice verified: live -> ready -> build -> OpenAPI -> create -> replay -> recover.")


if __name__ == "__main__":
    main()
