export type AuthenticationLoginMode =
  | 'credentials'
  | 'redirect';

export interface AuthenticationProviderDiscovery {
  providerId: string;
  displayName: string;
  loginMode: AuthenticationLoginMode;
  challengePath: string | null;
  sessionPath: string | null;
  tokenPath: string | null;
}

export interface LocalSession {
  subjectId: string;
  identityProvider: string;
  authorityRoles: string[];
}

export type FetchLike = (
  input: RequestInfo | URL,
  init?: RequestInit,
) => Promise<Response>;

export class AuthenticationClientError extends Error {
  public constructor(public readonly code: string) {
    super(code);
  }
}

export async function loadAuthenticationProviders(
  fetcher: FetchLike = fetch,
): Promise<AuthenticationProviderDiscovery[]> {
  const response = await fetcher(
    '/api/authentication/providers',
    {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
    },
  );

  if (!response.ok) {
    throw new AuthenticationClientError(
      'authentication_provider_request_failed',
    );
  }

  const payload = await response.json() as unknown;

  if (!Array.isArray(payload)) {
    throw new AuthenticationClientError(
      'authentication_provider_contract_invalid',
    );
  }

  return payload.map(validateProvider);
}

export async function createLocalSession(
  sessionPath: string,
  userName: string,
  password: string,
  fetcher: FetchLike = fetch,
): Promise<LocalSession> {
  if (!isSafeLocalPath(sessionPath)) {
    throw new AuthenticationClientError(
      'authentication_session_path_invalid',
    );
  }

  const response = await fetcher(sessionPath, {
    method: 'POST',
    credentials: 'same-origin',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      userName,
      password,
    }),
  });

  if (response.status === 401) {
    throw new AuthenticationClientError(
      'authentication_invalid_credentials',
    );
  }

  if (!response.ok) {
    throw new AuthenticationClientError(
      'authentication_session_failed',
    );
  }

  const payload = await response.json() as unknown;

  if (!isLocalSession(payload)) {
    throw new AuthenticationClientError(
      'authentication_session_contract_invalid',
    );
  }

  return payload;
}

export function buildAuthenticationChallengeUrl(
  provider: AuthenticationProviderDiscovery,
  returnUrl: string,
): string {
  if (
    provider.loginMode !== 'redirect'
    || !provider.challengePath
    || !isSafeLocalPath(provider.challengePath)
  ) {
    throw new AuthenticationClientError(
      'authentication_challenge_unavailable',
    );
  }

  if (!isSafeLocalPath(returnUrl)) {
    throw new AuthenticationClientError(
      'authentication_return_url_invalid',
    );
  }

  const query = new URLSearchParams({
    returnUrl,
  });

  return `${provider.challengePath}?${query.toString()}`;
}

function validateProvider(
  value: unknown,
): AuthenticationProviderDiscovery {
  if (!isRecord(value)) {
    throw new AuthenticationClientError(
      'authentication_provider_contract_invalid',
    );
  }

  const providerId = stringValue(value.providerId);
  const displayName = stringValue(value.displayName);
  const loginMode = value.loginMode;
  const challengePath = nullableString(value.challengePath);
  const sessionPath = nullableString(value.sessionPath);
  const tokenPath = nullableString(value.tokenPath);

  if (
    !providerId
    || !displayName
    || (loginMode !== 'credentials'
      && loginMode !== 'redirect')
  ) {
    throw new AuthenticationClientError(
      'authentication_provider_contract_invalid',
    );
  }

  if (
    loginMode === 'credentials'
    && (
      challengePath !== null
      || !sessionPath
      || !isSafeLocalPath(sessionPath)
    )
  ) {
    throw new AuthenticationClientError(
      'authentication_provider_contract_invalid',
    );
  }

  if (
    loginMode === 'redirect'
    && (
      !challengePath
      || !isSafeLocalPath(challengePath)
      || sessionPath !== null
      || tokenPath !== null
    )
  ) {
    throw new AuthenticationClientError(
      'authentication_provider_contract_invalid',
    );
  }

  if (
    tokenPath !== null
    && !isSafeLocalPath(tokenPath)
  ) {
    throw new AuthenticationClientError(
      'authentication_provider_contract_invalid',
    );
  }

  return {
    providerId,
    displayName,
    loginMode,
    challengePath,
    sessionPath,
    tokenPath,
  };
}

function isLocalSession(
  value: unknown,
): value is LocalSession {
  return isRecord(value)
    && Boolean(stringValue(value.subjectId))
    && Boolean(stringValue(value.identityProvider))
    && Array.isArray(value.authorityRoles)
    && value.authorityRoles.every(
      (role) =>
        typeof role === 'string'
        && role.trim().length > 0,
    );
}

function isSafeLocalPath(value: string): boolean {
  return value.startsWith('/')
    && !value.startsWith('//')
    && !value.includes('\\')
    && !Array.from(value).some(
      (character) =>
        character.charCodeAt(0) < 0x20
        || character.charCodeAt(0) === 0x7f,
    );
}

function isRecord(
  value: unknown,
): value is Record<string, unknown> {
  return typeof value === 'object'
    && value !== null
    && !Array.isArray(value);
}

function stringValue(value: unknown): string {
  return typeof value === 'string'
    ? value.trim()
    : '';
}

function nullableString(
  value: unknown,
): string | null {
  if (value === null) {
    return null;
  }

  const parsed = stringValue(value);
  return parsed || null;
}
