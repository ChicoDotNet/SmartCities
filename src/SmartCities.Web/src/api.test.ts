import assert from 'node:assert/strict';
import test from 'node:test';
import {
  loadMobilityReportOutcome,
  reportIdFromSearch,
  trackingSearchForReport,
} from './api.ts';

test('citizen outcome client loads localized authoritative state without caching', async () => {
  const outcome = await loadMobilityReportOutcome(
    'report-001',
    'es-MX',
    async (input, init) => {
      assert.equal(
        input,
        '/api/citizen/mobility-reports/report-001/outcome',
      );
      assert.equal(init?.method, 'GET');
      assert.equal(init?.cache, 'no-store');

      const headers = new Headers(init?.headers);
      assert.equal(
        headers.get('Accept-Language'),
        'es-MX',
      );

      return new Response(
        JSON.stringify({
          reportId: 'report-001',
          caseId: 'case-001',
          status: 'finalized',
          disposition: 'accepted',
          statusLabel: 'Revisión concluida',
          explanation: 'La revisión humana concluyó.',
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

  assert.equal(outcome?.reportId, 'report-001');
  assert.equal(outcome?.status, 'finalized');
  assert.equal(outcome?.disposition, 'accepted');
});

test('citizen outcome client returns null for an unknown report', async () => {
  const outcome = await loadMobilityReportOutcome(
    'missing',
    'en',
    async () => new Response(null, { status: 404 }),
  );

  assert.equal(outcome, null);
});

test('citizen outcome contract fails closed on impossible pending disposition', async () => {
  await assert.rejects(
    loadMobilityReportOutcome(
      'report-001',
      'en',
      async () =>
        new Response(
          JSON.stringify({
            reportId: 'report-001',
            caseId: 'case-001',
            status: 'pending-human-review',
            disposition: 'accepted',
            statusLabel: 'Pending',
            explanation: 'Pending.',
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        ),
    ),
    /mobility_outcome_contract_invalid/,
  );
});

test('report tracking survives refresh through the URL and preserves unrelated query values', () => {
  assert.equal(
    reportIdFromSearch('?reportId=report-001&culture=es-MX'),
    'report-001',
  );
  assert.equal(
    trackingSearchForReport(
      '?culture=es-MX',
      'report-001',
    ),
    '?culture=es-MX&reportId=report-001',
  );
  assert.equal(
    reportIdFromSearch('?reportId=%20%20'),
    null,
  );
});
