import assert from 'node:assert/strict';
import test from 'node:test';
import {
  addAdministrationGrant,
  deleteAdministrationGrant,
  loadAdministrationGrantCatalog,
  loadAdministrationGrants,
} from './administrationGrants.ts';

test('grant catalog is loaded through the protected Administration API', async () => {
  const catalog = await loadAdministrationGrantCatalog(
    async (input, init) => {
      assert.equal(
        input,
        '/api/administration/grants/catalog',
      );
      assert.equal(init?.credentials, 'same-origin');

      return new Response(
        JSON.stringify({
          authorityRoles: [
            'mobility-reviewer',
            'town-hall-admin',
          ],
          permissions: [
            'administration-grants.manage',
            'citizen-mobility.manage',
          ],
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

  assert.deepEqual(
    catalog.permissions,
    [
      'administration-grants.manage',
      'citizen-mobility.manage',
    ],
  );
});

test('grant CRUD preserves portable target and grant kinds', async () => {
  const grants = await loadAdministrationGrants(
    async () =>
      new Response(
        JSON.stringify([
          {
            grantId: 'grant-1',
            targetKind: 'email-domain',
            targetValue: 'townhall.gov',
            grantKind: 'permission',
            value: 'citizen-mobility.manage',
          },
        ]),
        {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
          },
        },
      ),
  );

  assert.equal(grants[0]?.targetValue, 'townhall.gov');

  const added = await addAdministrationGrant(
    'email',
    'official@example.com',
    'authority-role',
    'mobility-reviewer',
    async (input, init) => {
      assert.equal(
        input,
        '/api/administration/grants',
      );
      assert.equal(init?.method, 'POST');

      return new Response(
        JSON.stringify({
          grantId: 'grant-2',
          targetKind: 'email',
          targetValue: 'official@example.com',
          grantKind: 'authority-role',
          value: 'mobility-reviewer',
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

  assert.equal(added.grantId, 'grant-2');

  await deleteAdministrationGrant(
    'grant/2',
    async (input, init) => {
      assert.equal(
        input,
        '/api/administration/grants/grant%2F2',
      );
      assert.equal(init?.method, 'DELETE');
      return new Response(null, { status: 204 });
    },
  );
});
