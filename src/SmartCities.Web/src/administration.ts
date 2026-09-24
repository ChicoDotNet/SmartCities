import type {
  AuthenticationSession,
  FetchLike,
} from './authentication.ts';

export interface AdministrationAccess {
  townHallId: string;
  authorized: boolean;
  bootstrapAvailable: boolean;
}

export type AdministrationView =
  | 'bootstrap'
  | 'sign-in'
  | 'denied'
  | 'authorized';

export type AdministrationRuleKind =
  | 'email-domain'
  | 'email'
  | 'canonical-subject';

export interface AdministrationWhitelistRule {
  ruleId: string;
  kind: AdministrationRuleKind;
  value: string;
}

export interface AdministrationBootstrapSession {
  subjectId: string;
  userName: string;
}

export class AdministrationClientError extends Error {
  public readonly code: string;

  public constructor(code: string) {
    super(code);
    this.code = code;
  }
}

export async function loadAdministrationAccess(
  fetcher: FetchLike = fetch,
): Promise<AdministrationAccess> {
  const response = await fetcher(
    '/api/administration/access',
    {
      method: 'GET',
      credentials: 'same-origin',
      cache: 'no-store',
      headers: {
        Accept: 'application/json',
      },
    },
  );

  if (!response.ok) {
    throw new AdministrationClientError(
      'administration_access_request_failed',
    );
  }

  return validateAccess(
    await response.json() as unknown,
  );
}

export function resolveAdministrationView(
  session: AuthenticationSession,
  access: AdministrationAccess,
): AdministrationView {
  if (access.authorized) {
    return session.authenticated
      ? 'authorized'
      : 'denied';
  }

  if (!session.authenticated) {
    return access.bootstrapAvailable
      ? 'bootstrap'
      : 'sign-in';
  }

  return 'denied';
}

export async function createAdministrationBootstrapSession(
  userName: string,
  password: string,
  fetcher: FetchLike = fetch,
): Promise<AdministrationBootstrapSession> {
  const response = await fetcher(
    '/api/administration/bootstrap/session',
    {
      method: 'POST',
      credentials: 'same-origin',
      cache: 'no-store',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        userName,
        password,
      }),
    },
  );

  if (response.status === 401) {
    throw new AdministrationClientError(
      'administration_bootstrap_invalid_credentials',
    );
  }

  if (response.status === 404) {
    throw new AdministrationClientError(
      'administration_bootstrap_unavailable',
    );
  }

  if (!response.ok) {
    throw new AdministrationClientError(
      'administration_bootstrap_failed',
    );
  }

  const payload = await response.json() as unknown;

  if (
    !isRecord(payload)
    || !stringValue(payload.subjectId)
    || !stringValue(payload.userName)
  ) {
    throw new AdministrationClientError(
      'administration_bootstrap_contract_invalid',
    );
  }

  return {
    subjectId: stringValue(payload.subjectId),
    userName: stringValue(payload.userName),
  };
}

export async function loadAdministrationWhitelist(
  fetcher: FetchLike = fetch,
): Promise<AdministrationWhitelistRule[]> {
  const response = await fetcher(
    '/api/administration/whitelist',
    {
      method: 'GET',
      credentials: 'same-origin',
      cache: 'no-store',
      headers: {
        Accept: 'application/json',
      },
    },
  );

  await ensureAdministrationResponse(response);

  const payload = await response.json() as unknown;

  if (!Array.isArray(payload)) {
    throw new AdministrationClientError(
      'administration_whitelist_contract_invalid',
    );
  }

  const seen = new Set<string>();

  return payload.map((item) => {
    const rule = validateRule(item);

    if (seen.has(rule.ruleId)) {
      throw new AdministrationClientError(
        'administration_whitelist_contract_invalid',
      );
    }

    seen.add(rule.ruleId);
    return rule;
  });
}

export async function addAdministrationWhitelistRule(
  kind: AdministrationRuleKind,
  value: string,
  fetcher: FetchLike = fetch,
): Promise<AdministrationWhitelistRule> {
  const response = await fetcher(
    '/api/administration/whitelist',
    {
      method: 'POST',
      credentials: 'same-origin',
      cache: 'no-store',
      headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        kind,
        value,
      }),
    },
  );

  await ensureAdministrationResponse(response);

  return validateRule(
    await response.json() as unknown,
  );
}

export async function deleteAdministrationWhitelistRule(
  ruleId: string,
  fetcher: FetchLike = fetch,
): Promise<void> {
  const normalized = ruleId.trim();

  if (!normalized) {
    throw new AdministrationClientError(
      'administration_rule_id_invalid',
    );
  }

  const response = await fetcher(
    `/api/administration/whitelist/${encodeURIComponent(normalized)}`,
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
    throw new AdministrationClientError(
      'administration_rule_not_found',
    );
  }

  await ensureAdministrationResponse(response);
}

function validateAccess(
  value: unknown,
): AdministrationAccess {
  if (
    !isRecord(value)
    || !stringValue(value.townHallId)
    || typeof value.authorized !== 'boolean'
    || typeof value.bootstrapAvailable !== 'boolean'
  ) {
    throw new AdministrationClientError(
      'administration_access_contract_invalid',
    );
  }

  return {
    townHallId: stringValue(value.townHallId),
    authorized: value.authorized,
    bootstrapAvailable: value.bootstrapAvailable,
  };
}

function validateRule(
  value: unknown,
): AdministrationWhitelistRule {
  if (!isRecord(value)) {
    throw new AdministrationClientError(
      'administration_whitelist_contract_invalid',
    );
  }

  const ruleId = stringValue(value.ruleId);
  const kind = value.kind;
  const ruleValue = stringValue(value.value);

  if (
    !ruleId
    || !ruleValue
    || (
      kind !== 'email-domain'
      && kind !== 'email'
      && kind !== 'canonical-subject'
    )
  ) {
    throw new AdministrationClientError(
      'administration_whitelist_contract_invalid',
    );
  }

  return {
    ruleId,
    kind,
    value: ruleValue,
  };
}

async function ensureAdministrationResponse(
  response: Response,
): Promise<void> {
  if (response.status === 401) {
    throw new AdministrationClientError(
      'administration_unauthenticated',
    );
  }

  if (response.status === 403) {
    throw new AdministrationClientError(
      'administration_forbidden',
    );
  }

  if (!response.ok) {
    throw new AdministrationClientError(
      'administration_request_failed',
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

function stringValue(
  value: unknown,
): string {
  return typeof value === 'string'
    ? value.trim()
    : '';
}
