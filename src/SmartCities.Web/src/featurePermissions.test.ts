import assert from 'node:assert/strict';
import test from 'node:test';
import {
  canConfigureFeature,
  canManageFeature,
  configurationPermissionForFeature,
  managementPermissionForFeature,
} from './featurePermissions.ts';

test('feature permissions distinguish operational manage from configuration', () => {
  assert.equal(
    managementPermissionForFeature(
      'citizen-mobility',
    ),
    'citizen-mobility.manage',
  );
  assert.equal(
    configurationPermissionForFeature(
      'citizen-mobility',
    ),
    'citizen-mobility.config',
  );

  assert.equal(
    canManageFeature(
      [
        'feature-flags.manage',
        'citizen-mobility.manage',
      ],
      'citizen-mobility',
    ),
    true,
  );
  assert.equal(
    canManageFeature(
      [
        'feature-flags.manage',
        'citizen-mobility.config',
      ],
      'citizen-mobility',
    ),
    false,
  );
  assert.equal(
    canConfigureFeature(
      [
        'feature-flags.config',
        'citizen-mobility.config',
      ],
      'citizen-mobility',
    ),
    true,
  );
  assert.equal(
    canConfigureFeature(
      [
        'feature-flags.manage',
        'citizen-mobility.manage',
      ],
      'citizen-mobility',
    ),
    false,
  );
});

test('feature permissions require both global and feature-specific grants', () => {
  assert.equal(
    canConfigureFeature(
      ['feature-flags.config'],
      'citizen-mobility',
    ),
    false,
  );
  assert.equal(
    canConfigureFeature(
      ['citizen-mobility.config'],
      'citizen-mobility',
    ),
    false,
  );
  assert.equal(
    canManageFeature(
      ['feature-flags.manage'],
      'citizen-mobility',
    ),
    false,
  );
  assert.equal(
    canManageFeature(
      ['citizen-mobility.manage'],
      'citizen-mobility',
    ),
    false,
  );
});
