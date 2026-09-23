import { type ResourceKey } from './resourceKeys';

export type SupportedCulture = 'en' | 'es-MX';

export interface LocalizationBundle {
  requestedCulture: string;
  resolvedCulture: string;
  resources: Record<string, string>;
}

export function preferredCulture(language: string): SupportedCulture {
  return language.toLowerCase().startsWith('es') ? 'es-MX' : 'en';
}

export async function loadLocalization(
  culture: SupportedCulture,
  signal?: AbortSignal,
): Promise<LocalizationBundle> {
  const response = await fetch(
    `/api/localization/resources/${encodeURIComponent(culture)}`,
    {
      headers: {
        Accept: 'application/json',
      },
      signal,
    },
  );

  if (!response.ok) {
    throw new Error('localization_request_failed');
  }

  return response.json() as Promise<LocalizationBundle>;
}

export function text(
  bundle: LocalizationBundle,
  key: ResourceKey,
): string {
  const value = bundle.resources[key];

  if (!value) {
    throw new Error(`missing_resource:${key}`);
  }

  return value;
}
