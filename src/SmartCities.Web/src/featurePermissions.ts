export const featureFlagsManagePermission =
  'feature-flags.manage';

export const featureFlagsConfigPermission =
  'feature-flags.config';

export function managementPermissionForFeature(
  featureId: string,
): string {
  return buildFeaturePermission(
    featureId,
    'manage',
  );
}

export function configurationPermissionForFeature(
  featureId: string,
): string {
  return buildFeaturePermission(
    featureId,
    'config',
  );
}

export function canManageFeature(
  permissions: readonly string[],
  featureId: string,
): boolean {
  return permissions.includes(
    featureFlagsManagePermission,
  )
    && permissions.includes(
      managementPermissionForFeature(featureId),
    );
}

export function canConfigureFeature(
  permissions: readonly string[],
  featureId: string,
): boolean {
  return permissions.includes(
    featureFlagsConfigPermission,
  )
    && permissions.includes(
      configurationPermissionForFeature(featureId),
    );
}

function buildFeaturePermission(
  featureId: string,
  capability: 'manage' | 'config',
): string {
  const normalized = featureId.trim();

  if (
    !normalized
    || normalized.length > 128
    || normalized.startsWith('-')
    || normalized.endsWith('-')
    || Array.from(normalized).some(
      (character) =>
        !(
          (character >= 'a' && character <= 'z')
          || (character >= '0' && character <= '9')
          || character === '-'
        ),
    )
  ) {
    throw new Error(
      'feature_permission_identifier_invalid',
    );
  }

  return `${normalized}.${capability}`;
}
