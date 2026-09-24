export interface FeatureFlagState {
  featureId: string;
  enabled: boolean;
}

export interface FeatureFlagSnapshot {
  townHallId: string;
  features: FeatureFlagState[];
}

export type FeatureFetchLike = (
  input: RequestInfo | URL,
  init?: RequestInit,
) => Promise<Response>;

export async function loadFeatureFlagSnapshot(
  fetcher: FeatureFetchLike = fetch,
): Promise<FeatureFlagSnapshot> {
  const response = await fetcher(
    '/api/system/features',
    {
      method: 'GET',
      cache: 'no-store',
      headers: {
        Accept: 'application/json',
      },
    },
  );

  if (!response.ok) {
    throw new Error(
      'feature_flag_request_failed',
    );
  }

  const payload = await response.json() as unknown;

  return validateSnapshot(payload);
}

export function isFeatureEnabled(
  snapshot: FeatureFlagSnapshot,
  featureId: string,
): boolean {
  return snapshot.features.some(
    (feature) =>
      feature.featureId === featureId
      && feature.enabled,
  );
}

function validateSnapshot(
  value: unknown,
): FeatureFlagSnapshot {
  if (
    !isRecord(value)
    || !stringValue(value.townHallId)
    || !Array.isArray(value.features)
  ) {
    throw new Error(
      'feature_flag_contract_invalid',
    );
  }

  const features: FeatureFlagState[] = [];
  const seen = new Set<string>();

  for (const item of value.features) {
    if (
      !isRecord(item)
      || typeof item.enabled !== 'boolean'
    ) {
      throw new Error(
        'feature_flag_contract_invalid',
      );
    }

    const featureId = stringValue(
      item.featureId,
    );

    if (!featureId || seen.has(featureId)) {
      throw new Error(
        'feature_flag_contract_invalid',
      );
    }

    seen.add(featureId);
    features.push({
      featureId,
      enabled: item.enabled,
    });
  }

  return {
    townHallId: stringValue(value.townHallId),
    features,
  };
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
