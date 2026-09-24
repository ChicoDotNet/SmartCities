import assert from 'node:assert/strict';
import test from 'node:test';
import {
  buildAuthenticationChallengeUrl,
  createLocalSession,
  loadAuthenticationProviders,
  loadCurrentSession,
  signOutCurrentSession,
  type AuthenticationProviderDiscovery,
} from './authentication.ts';

test('provider discovery loads exactly the API-provided login choices', async () => {
  const expected: AuthenticationProviderDiscovery[] = [
    {
      providerId: 'google',
      displayName: 'Google',
      loginMode: 'redirect',
      challengePath: '/api/authentication/providers/google/challenge',
      sessionPath: null,
      tokenPath: null,
    },
    {
      providerId: 'local',
      displayName: 'Local',
      loginMode: 'credentials',
      challengePath: null,
      sessionPath: '/api/authentication/local/session',
      tokenPath: '/api/authentication/local/token',
    },
  ];

  const providers = await loadAuthenticationProviders(
    async (input, init) => {
      assert.equal(input, '/api/authentication/providers');
      assert.equal(init?.method, 'GET');

      return new Response(
        JSON.stringify(expected),
        {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
          },
        });
    },
  );

  assert.deepEqual(providers, expected);
});

test('provider discovery fails closed when the API returns an unsupported login mode', async () => {
  await assert.rejects(
    loadAuthenticationProviders(
      async () =>
        new Response(
          JSON.stringify([
            {
              providerId: 'unexpected',
              displayName: 'Unexpected',
              loginMode: 'magic',
              challengePath: '/magic',
              sessionPath: null,
              tokenPath: null,
            },
          ]),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        ),
    ),
    /authentication_provider_contract_invalid/,
  );
});

test('redirect challenge URL carries only the current local application path', () => {
  const provider: AuthenticationProviderDiscovery = {
    providerId: 'google',
    displayName: 'Google',
    loginMode: 'redirect',
    challengePath: '/api/authentication/providers/google/challenge',
    sessionPath: null,
    tokenPath: null,
  };

  assert.equal(
    buildAuthenticationChallengeUrl(
      provider,
      '/citizen/report?case=42#status',
    ),
    '/api/authentication/providers/google/challenge?returnUrl=%2Fcitizen%2Freport%3Fcase%3D42%23status',
  );

  assert.throws(
    () =>
      buildAuthenticationChallengeUrl(
        provider,
        'https://evil.example',
      ),
    /authentication_return_url_invalid/,
  );
});

test('local session posts credentials without retaining them in the returned identity', async () => {
  const session = await createLocalSession(
    '/api/authentication/local/session',
    'alice',
    'correct-password',
    async (input, init) => {
      assert.equal(
        input,
        '/api/authentication/local/session',
      );
      assert.equal(init?.method, 'POST');
      assert.equal(
        init?.headers instanceof Headers
          ? init.headers.get('Content-Type')
          : (init?.headers as Record<string, string>)['Content-Type'],
        'application/json',
      );
      assert.deepEqual(
        JSON.parse(String(init?.body)),
        {
          userName: 'alice',
          password: 'correct-password',
        },
      );

      return new Response(
        JSON.stringify({
          subjectId: 'local:default:alice-001',
          identityProvider: 'local',
          authorityRoles: ['citizen'],
        }),
        {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
          },
        },
      );
    },
  );

  assert.deepEqual(session, {
    subjectId: 'local:default:alice-001',
    identityProvider: 'local',
    authorityRoles: ['citizen'],
  });
  assert.equal(
    Object.hasOwn(session, 'password'),
    false,
  );
});

test('invalid local credentials surface an authentication-specific error', async () => {
  await assert.rejects(
    createLocalSession(
      '/api/authentication/local/session',
      'alice',
      'wrong-password',
      async () =>
        new Response(null, {
          status: 401,
        }),
    ),
    /authentication_invalid_credentials/,
  );
});


test('current session bootstrap treats anonymous 200 as authoritative anonymous state', async () => {
  const session = await loadCurrentSession(
    async (input, init) => {
      assert.equal(input, '/api/authentication/session');
      assert.equal(init?.method, 'GET');
      assert.equal(init?.credentials, 'same-origin');

      return new Response(
        JSON.stringify({
          authenticated: false,
          subjectId: null,
          identityProvider: null,
          authorityRoles: [],
          permissions: [],
        }),
        {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
          },
        },
      );
    },
  );

  assert.deepEqual(session, {
    authenticated: false,
    subjectId: null,
    identityProvider: null,
    authorityRoles: [],
    permissions: [],
  });
});

test('current session bootstrap accepts only canonical authenticated identity', async () => {
  const session = await loadCurrentSession(
    async () =>
      new Response(
        JSON.stringify({
          authenticated: true,
          subjectId: 'workforce:tenant-a:user-42',
          identityProvider: 'workforce',
          authorityRoles: ['mobility-reviewer'],
          permissions: ['decision-review.finalize'],
        }),
        {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
          },
        },
      ),
  );

  assert.equal(session.authenticated, true);
  assert.equal(
    session.subjectId,
    'workforce:tenant-a:user-42',
  );
  assert.equal(session.identityProvider, 'workforce');
  assert.deepEqual(
    session.authorityRoles,
    ['mobility-reviewer'],
  );
  assert.deepEqual(
    session.permissions,
    ['decision-review.finalize'],
  );
});

test('logout posts to the canonical session endpoint with the browser CSRF marker', async () => {
  await signOutCurrentSession(
    async (input, init) => {
      assert.equal(
        input,
        '/api/authentication/session/logout',
      );
      assert.equal(init?.method, 'POST');
      assert.equal(init?.credentials, 'same-origin');

      const headers = new Headers(init?.headers);
      assert.equal(
        headers.get('X-SmartCities-Request'),
        'browser',
      );

      return new Response(null, {
        status: 204,
      });
    },
  );
});
