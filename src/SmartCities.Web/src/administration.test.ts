import assert from 'node:assert/strict';
import test from 'node:test';
import {
  addAdministrationWhitelistRule,
  AdministrationClientError,
  createAdministrationBootstrapSession,
  deleteAdministrationWhitelistRule,
  loadAdministrationAccess,
  loadAdministrationWhitelist,
  resolveAdministrationView,
  type AdministrationAccess,
} from './administration.ts';
import type { AuthenticationSession } from './authentication.ts';

test('administration access probe is backend-authoritative and no-store', async () => {
  const expected: AdministrationAccess = {
    townHallId: 'town-hall-a',
    authorized: true,
    bootstrapAvailable: false,
  };

  const access = await loadAdministrationAccess(
    async (input, init) => {
      assert.equal(input, '/api/administration/access');
      assert.equal(init?.method, 'GET');
      assert.equal(init?.credentials, 'same-origin');
      assert.equal(init?.cache, 'no-store');

      return new Response(
        JSON.stringify(expected),
        {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
          },
        },
      );
    },
  );

  assert.deepEqual(access, expected);
});

test('administration view distinguishes bootstrap, sign-in, denied, and authorized states', () => {
  const anonymous: AuthenticationSession = {
    authenticated: false,
    subjectId: null,
    identityProvider: null,
    authorityRoles: [],
    permissions: [],
  };
  const authenticated: AuthenticationSession = {
    authenticated: true,
    subjectId: 'provider:tenant:user',
    identityProvider: 'provider',
    authorityRoles: [],
    permissions: [],
  };

  assert.equal(
    resolveAdministrationView(
      anonymous,
      {
        townHallId: 'town',
        authorized: false,
        bootstrapAvailable: true,
      },
    ),
    'bootstrap',
  );
  assert.equal(
    resolveAdministrationView(
      anonymous,
      {
        townHallId: 'town',
        authorized: false,
        bootstrapAvailable: false,
      },
    ),
    'sign-in',
  );
  assert.equal(
    resolveAdministrationView(
      authenticated,
      {
        townHallId: 'town',
        authorized: false,
        bootstrapAvailable: false,
      },
    ),
    'denied',
  );
  assert.equal(
    resolveAdministrationView(
      authenticated,
      {
        townHallId: 'town',
        authorized: true,
        bootstrapAvailable: false,
      },
    ),
    'authorized',
  );
});

test('bootstrap session posts only to the fixed Administration bootstrap endpoint', async () => {
  await createAdministrationBootstrapSession(
    'townhalladmin@smartcities.local',
    'bootstrap-secret-value',
    async (input, init) => {
      assert.equal(
        input,
        '/api/administration/bootstrap/session',
      );
      assert.equal(init?.method, 'POST');
      assert.equal(init?.credentials, 'same-origin');
      assert.deepEqual(
        JSON.parse(String(init?.body)),
        {
          userName: 'townhalladmin@smartcities.local',
          password: 'bootstrap-secret-value',
        },
      );

      return new Response(
        JSON.stringify({
          subjectId:
            'local-bootstrap:default:townhalladmin@smartcities.local',
          userName:
            'townhalladmin@smartcities.local',
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
});

test('bootstrap endpoint surfaces invalid credentials and disabled bootstrap distinctly', async () => {
  await assert.rejects(
    createAdministrationBootstrapSession(
      'townhalladmin@smartcities.local',
      'wrong-secret',
      async () => new Response(null, { status: 401 }),
    ),
    (error: unknown) =>
      error instanceof AdministrationClientError
      && error.code ===
        'administration_bootstrap_invalid_credentials',
  );

  await assert.rejects(
    createAdministrationBootstrapSession(
      'townhalladmin@smartcities.local',
      'configured-secret',
      async () => new Response(null, { status: 404 }),
    ),
    (error: unknown) =>
      error instanceof AdministrationClientError
      && error.code ===
        'administration_bootstrap_unavailable',
  );
});

test('whitelist management uses same-origin credentials and stable rule contracts', async () => {
  const rules = await loadAdministrationWhitelist(
    async (input, init) => {
      assert.equal(input, '/api/administration/whitelist');
      assert.equal(init?.method, 'GET');
      assert.equal(init?.credentials, 'same-origin');

      return new Response(
        JSON.stringify([
          {
            ruleId: 'rule-1',
            kind: 'email-domain',
            value: 'townhall.gov',
          },
        ]),
        {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
          },
        },
      );
    },
  );

  assert.deepEqual(rules, [
    {
      ruleId: 'rule-1',
      kind: 'email-domain',
      value: 'townhall.gov',
    },
  ]);

  const added = await addAdministrationWhitelistRule(
    'email',
    'official@example.com',
    async (input, init) => {
      assert.equal(input, '/api/administration/whitelist');
      assert.equal(init?.method, 'POST');
      assert.equal(init?.credentials, 'same-origin');
      assert.deepEqual(
        JSON.parse(String(init?.body)),
        {
          kind: 'email',
          value: 'official@example.com',
        },
      );

      return new Response(
        JSON.stringify({
          ruleId: 'rule-2',
          kind: 'email',
          value: 'official@example.com',
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

  assert.equal(added.ruleId, 'rule-2');

  await deleteAdministrationWhitelistRule(
    'rule/with spaces',
    async (input, init) => {
      assert.equal(
        input,
        '/api/administration/whitelist/rule%2Fwith%20spaces',
      );
      assert.equal(init?.method, 'DELETE');
      assert.equal(init?.credentials, 'same-origin');

      return new Response(null, {
        status: 204,
      });
    },
  );
});

test('whitelist management surfaces backend authorization as authoritative', async () => {
  await assert.rejects(
    loadAdministrationWhitelist(
      async () =>
        new Response(null, {
          status: 403,
        }),
    ),
    (error: unknown) =>
      error instanceof AdministrationClientError
      && error.code === 'administration_forbidden',
  );
});
