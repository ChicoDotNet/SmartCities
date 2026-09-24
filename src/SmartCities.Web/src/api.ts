export interface MobilityReportAcceptance {
  reportId: string;
  caseId: string;
  wasCreated: boolean;
}

export type MobilityReportOutcomeStatus =
  | 'pending-human-review'
  | 'finalized';

export type MobilityReportDisposition =
  | 'accepted'
  | 'modified'
  | 'rejected'
  | 'deferred';

export interface MobilityReportOutcome {
  reportId: string;
  caseId: string;
  status: MobilityReportOutcomeStatus;
  disposition: MobilityReportDisposition | null;
  statusLabel: string;
  explanation: string;
}

export interface ApiProblem {
  title?: string;
  detail?: string;
  code?: string;
  fields?: string[];
}

export type FetchLike = (
  input: RequestInfo | URL,
  init?: RequestInit,
) => Promise<Response>;

export class ApiProblemError extends Error {
  public readonly problem: ApiProblem;

  public constructor(problem: ApiProblem) {
    super(problem.code ?? 'api_problem');
    this.problem = problem;
  }
}

export async function submitMobilityReport(
  culture: string,
  categoryKey: string,
  locationReference: string,
  description: string,
): Promise<MobilityReportAcceptance> {
  const response = await fetch('/api/citizen/mobility-reports', {
    method: 'POST',
    headers: {
      'Accept-Language': culture,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      reportId: crypto.randomUUID(),
      caseId: crypto.randomUUID(),
      categoryKey,
      locationReference,
      description,
      evidenceReferences: [],
    }),
  });

  if (!response.ok) {
    let problem: ApiProblem = {};

    try {
      problem = await response.json() as ApiProblem;
    } catch {
      // The caller will use localized generic submit text when no problem body exists.
    }

    throw new ApiProblemError(problem);
  }

  return response.json() as Promise<MobilityReportAcceptance>;
}

export async function loadMobilityReportOutcome(
  reportId: string,
  culture: string,
  fetcher: FetchLike = fetch,
): Promise<MobilityReportOutcome | null> {
  const normalizedReportId = normalizeReportId(
    reportId);

  if (!normalizedReportId) {
    throw new Error(
      'mobility_outcome_report_id_invalid',
    );
  }

  const response = await fetcher(
    `/api/citizen/mobility-reports/${encodeURIComponent(normalizedReportId)}/outcome`,
    {
      method: 'GET',
      cache: 'no-store',
      headers: {
        Accept: 'application/json',
        'Accept-Language': culture,
      },
    },
  );

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error(
      'mobility_outcome_request_failed',
    );
  }

  const payload = await response.json() as unknown;

  return validateMobilityReportOutcome(
    payload,
    normalizedReportId,
  );
}

export function reportIdFromSearch(
  search: string,
): string | null {
  const params = new URLSearchParams(search);
  return normalizeReportId(
    params.get('reportId') ?? '',
  );
}

export function trackingSearchForReport(
  search: string,
  reportId: string,
): string {
  const normalizedReportId = normalizeReportId(
    reportId);

  if (!normalizedReportId) {
    throw new Error(
      'mobility_outcome_report_id_invalid',
    );
  }

  const params = new URLSearchParams(search);
  params.set('reportId', normalizedReportId);

  return `?${params.toString()}`;
}

function validateMobilityReportOutcome(
  value: unknown,
  expectedReportId: string,
): MobilityReportOutcome {
  if (!isRecord(value)) {
    throw new Error(
      'mobility_outcome_contract_invalid',
    );
  }

  const reportId = stringValue(value.reportId);
  const caseId = stringValue(value.caseId);
  const status = value.status;
  const disposition = value.disposition;
  const statusLabel = stringValue(
    value.statusLabel,
  );
  const explanation = stringValue(
    value.explanation,
  );

  if (
    reportId !== expectedReportId
    || !caseId
    || !statusLabel
    || !explanation
  ) {
    throw new Error(
      'mobility_outcome_contract_invalid',
    );
  }

  if (
    status === 'pending-human-review'
    && disposition === null
  ) {
    return {
      reportId,
      caseId,
      status,
      disposition: null,
      statusLabel,
      explanation,
    };
  }

  if (
    status === 'finalized'
    && (
      disposition === 'accepted'
      || disposition === 'modified'
      || disposition === 'rejected'
      || disposition === 'deferred'
    )
  ) {
    return {
      reportId,
      caseId,
      status,
      disposition,
      statusLabel,
      explanation,
    };
  }

  throw new Error(
    'mobility_outcome_contract_invalid',
  );
}

function normalizeReportId(
  value: string,
): string | null {
  const normalized = value.trim();

  if (
    normalized.length === 0
    || normalized.length > 128
    || Array.from(normalized).some(
      (character) =>
        character.charCodeAt(0) < 0x20
        || character.charCodeAt(0) === 0x7f,
    )
  ) {
    return null;
  }

  return normalized;
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
