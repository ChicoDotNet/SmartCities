export interface MobilityReportAcceptance {
  reportId: string;
  caseId: string;
  wasCreated: boolean;
}

export interface ApiProblem {
  title?: string;
  detail?: string;
  code?: string;
  fields?: string[];
}

export class ApiProblemError extends Error {
  public constructor(public readonly problem: ApiProblem) {
    super(problem.code ?? 'api_problem');
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
