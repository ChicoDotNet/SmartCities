import type { FetchLike } from './authentication.ts';

export type AdministrationGrantTargetKind =
  | 'email-domain'
  | 'email'
  | 'canonical-subject';

export type AdministrationGrantKind =
  | 'authority-role'
  | 'permission';

export interface AdministrationAuthorizationGrant {
  grantId: string;
  targetKind: AdministrationGrantTargetKind;
  targetValue: string;
  grantKind: AdministrationGrantKind;
  value: string;
}

export interface AdministrationAuthorizationGrantCatalog {
  authorityRoles: string[];
  permissions: string[];
}

export class AdministrationGrantClientError extends Error {
  public readonly code: string;

  public constructor(code: string) {
    super(code);
    this.code = code;
  }
}

export async function loadAdministrationGrantCatalog(
  fetcher: FetchLike = fetch,
): Promise<AdministrationAuthorizationGrantCatalog> {
  const response = await fetcher(
    '/api/administration/grants/catalog',
    {
      method: 'GET',
      credentials: 'same-origin',
      cache: 'no-store',
      headers: {
        Accept: 'application/json',
      },
    },
  );

  await ensureResponse(response);

  const payload = await response.json() as unknown;

  if (
    !isRecord(payload)
    || !isStringArray(payload.authorityRoles)
    || !isStringArray(payload.permissions)
  ) {
    throw new AdministrationGrantClientError(
      'administration_grant_catalog_invalid',
    );
  }

  return {
    authorityRoles: [...payload.authorityRoles],
    permissions: [...payload.permissions],
  };
}

export async function loadAdministrationGrants(
  fetcher: FetchLike = fetch,
): Promise<AdministrationAuthorizationGrant[]> {
  const response = await fetcher(
    '/api/administration/grants',
    {
      method: 'GET',
      credentials: 'same-origin',
      cache: 'no-store',
      headers: {
        Accept: 'application/json',
      },
    },
  );

  await ensureResponse(response);

  const payload = await response.json() as unknown;

  if (!Array.isArray(payload)) {
    throw new AdministrationGrantClientError(
      'administration_grants_contract_invalid',
    );
  }

  return payload.map(validateGrant);
}

export async function addAdministrationGrant(
  targetKind: AdministrationGrantTargetKind,
  targetValue: string,
  grantKind: AdministrationGrantKind,
  value: string,
  fetcher: FetchLike = fetch,
): Promise<AdministrationAuthorizationGrant> {
  const response = await fetcher(
    '/api/administration/grants',
    {
      method: 'POST',
      credentials: 'same-origin',
      cache: 'no-store',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        targetKind,
        targetValue,
        grantKind,
        value,
      }),
    },
  );

  await ensureResponse(response);

  return validateGrant(
    await response.json() as unknown,
  );
}

export async function deleteAdministrationGrant(
  grantId: string,
  fetcher: FetchLike = fetch,
): Promise<void> {
  const normalized = grantId.trim();

  if (!normalized) {
    throw new AdministrationGrantClientError(
      'administration_grant_id_invalid',
    );
  }

  const response = await fetcher(
    `/api/administration/grants/${encodeURIComponent(normalized)}`,
    {
      method: 'DELETE',
      credentials: 'same-origin',
      cache: 'no-store',
      headers: {
        Accept: 'application/json',
      },
    },
  );

  if (response.status === 404) {
    throw new AdministrationGrantClientError(
      'administration_grant_not_found',
    );
  }

  await ensureResponse(response);
}

function validateGrant(
  value: unknown,
): AdministrationAuthorizationGrant {
  if (!isRecord(value)) {
    throw new AdministrationGrantClientError(
      'administration_grants_contract_invalid',
    );
  }

  const grantId = stringValue(value.grantId);
  const targetKind = value.targetKind;
  const targetValue = stringValue(value.targetValue);
  const grantKind = value.grantKind;
  const grantValue = stringValue(value.value);

  if (
    !grantId
    || !targetValue
    || !grantValue
    || (
      targetKind !== 'email-domain'
      && targetKind !== 'email'
      && targetKind !== 'canonical-subject'
    )
    || (
      grantKind !== 'authority-role'
      && grantKind !== 'permission'
    )
  ) {
    throw new AdministrationGrantClientError(
      'administration_grants_contract_invalid',
    );
  }

  return {
    grantId,
    targetKind,
    targetValue,
    grantKind,
    value: grantValue,
  };
}

async function ensureResponse(
  response: Response,
): Promise<void> {
  if (response.status === 401) {
    throw new AdministrationGrantClientError(
      'administration_unauthenticated',
    );
  }

  if (response.status === 403) {
    throw new AdministrationGrantClientError(
      'administration_forbidden',
    );
  }

  if (!response.ok) {
    throw new AdministrationGrantClientError(
      'administration_grants_request_failed',
    );
  }
}

function isRecord(
  value: unknown,
): value is Record<string, unknown> {
  return typeof value === 'object'
    && value !== null
    && !Array.isArray(value);
}

function isStringArray(
  value: unknown,
): value is string[] {
  return Array.isArray(value)
    && value.every(
      item =>
        typeof item === 'string'
        && item.trim().length > 0,
    );
}

function stringValue(
  value: unknown,
): string {
  return typeof value === 'string'
    ? value.trim()
    : '';
}
