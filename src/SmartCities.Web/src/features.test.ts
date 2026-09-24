import assert from 'node:assert/strict';
import test from 'node:test';
import {
  loadFeatureFlagSnapshot,
  type FeatureFlagSnapshot,
} from './features.ts';

test('feature snapshot is authoritative for the current town hall', async () => {
  const expected: FeatureFlagSnapshot = {
    townHallId: 'town-hall-a',
    features: [
      {
        featureId: 'citizen-mobility',
        enabled: false,
      },
    ],
  };

  const snapshot = await loadFeatureFlagSnapshot(
    async (input, init) => {
      assert.equal(input, '/api/system/features');
      assert.equal(init?.method, 'GET');
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

  assert.deepEqual(snapshot, expected);
});

test('feature snapshot fails closed on duplicate feature identifiers', async () => {
  await assert.rejects(
    loadFeatureFlagSnapshot(
      async () =>
        new Response(
          JSON.stringify({
            townHallId: 'town-hall-a',
            features: [
              {
                featureId: 'citizen-mobility',
                enabled: true,
              },
              {
                featureId: 'citizen-mobility',
                enabled: false,
              },
            ],
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        ),
    ),
    /feature_flag_contract_invalid/,
  );
});
