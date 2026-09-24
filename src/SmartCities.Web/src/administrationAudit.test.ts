import assert from 'node:assert/strict';
import test from 'node:test';
import {
  loadAdministrationAudit,
} from './administrationAudit.ts';

test('Administration audit loads bounded newest-first history through same-origin auth', async () => {
  const entries = await loadAdministrationAudit(
    50,
    async (input, init) => {
      assert.equal(
        input,
        '/api/administration/audit?limit=50',
      );
      assert.equal(init?.credentials, 'same-origin');
      assert.equal(init?.cache, 'no-store');

      return new Response(
        JSON.stringify([
          {
            eventId: 'audit-2',
            occurredAtUtc: '2026-09-24T09:31:00+00:00',
            actorSubjectId: 'provider:tenant:official',
            actorIdentityProvider: 'provider',
            action: 'feature-flag.set',
            resourceType: 'feature-flag',
            resourceId: 'citizen-mobility',
            descriptor: 'citizen-mobility',
            previousValue: 'true',
            newValue: 'false',
            correlationId: 'corr-2',
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

  assert.equal(entries[0]?.eventId, 'audit-2');
  assert.equal(entries[0]?.newValue, 'false');
});
