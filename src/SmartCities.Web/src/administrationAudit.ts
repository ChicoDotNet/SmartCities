import type { FetchLike } from './authentication.ts';

export interface AdministrationAuditEntry {
  eventId: string;
  occurredAtUtc: string;
  actorSubjectId: string;
  actorIdentityProvider: string;
  action: string;
  resourceType: string;
  resourceId: string;
  descriptor: string;
  previousValue: string | null;
  newValue: string | null;
  correlationId: string;
}

export class AdministrationAuditClientError extends Error {
  public readonly code: string;

  public constructor(code: string) {
    super(code);
    this.code = code;
  }
}

export async function loadAdministrationAudit(
  limit = 50,
  fetcher: FetchLike = fetch,
): Promise<AdministrationAuditEntry[]> {
  if (!Number.isInteger(limit) || limit < 1 || limit > 200) {
    throw new AdministrationAuditClientError(
      'administration_audit_limit_invalid',
    );
  }

  const response = await fetcher(
    `/api/administration/audit?limit=${limit}`,
    {
      method: 'GET',
      credentials: 'same-origin',
      cache: 'no-store',
      headers: {
        Accept: 'application/json',
      },
    },
  );

  if (response.status === 401) {
    throw new AdministrationAuditClientError(
      'administration_unauthenticated',
    );
  }

  if (response.status === 403) {
    throw new AdministrationAuditClientError(
      'administration_forbidden',
    );
  }

  if (!response.ok) {
    throw new AdministrationAuditClientError(
      'administration_audit_request_failed',
    );
  }

  const payload = await response.json() as unknown;

  if (!Array.isArray(payload)) {
    throw new AdministrationAuditClientError(
      'administration_audit_contract_invalid',
    );
  }

  return payload.map(validateEntry);
}

function validateEntry(
  value: unknown,
): AdministrationAuditEntry {
  if (!isRecord(value)) {
    throw new AdministrationAuditClientError(
      'administration_audit_contract_invalid',
    );
  }

  const eventId = stringValue(value.eventId);
  const occurredAtUtc = stringValue(value.occurredAtUtc);
  const actorSubjectId = stringValue(value.actorSubjectId);
  const actorIdentityProvider =
    stringValue(value.actorIdentityProvider);
  const action = stringValue(value.action);
  const resourceType = stringValue(value.resourceType);
  const resourceId = stringValue(value.resourceId);
  const descriptor = stringValue(value.descriptor);
  const correlationId = stringValue(value.correlationId);
  const previousValue = nullableString(value.previousValue);
  const newValue = nullableString(value.newValue);

  if (
    !eventId
    || !occurredAtUtc
    || Number.isNaN(Date.parse(occurredAtUtc))
    || !actorSubjectId
    || !actorIdentityProvider
    || !action
    || !resourceType
    || !resourceId
    || !descriptor
    || !correlationId
  ) {
    throw new AdministrationAuditClientError(
      'administration_audit_contract_invalid',
    );
  }

  return {
    eventId,
    occurredAtUtc,
    actorSubjectId,
    actorIdentityProvider,
    action,
    resourceType,
    resourceId,
    descriptor,
    previousValue,
    newValue,
    correlationId,
  };
}

function nullableString(
  value: unknown,
): string | null {
  if (value === null) {
    return null;
  }

  return stringValue(value) || null;
}

function isRecord(
  value: unknown,
): value is Record<string, unknown> {
  return typeof value === 'object'
    && value !== null
    && !Array.isArray(value);
}

function stringValue(
  value: unknown,
): string {
  return typeof value === 'string'
    ? value.trim()
    : '';
}
