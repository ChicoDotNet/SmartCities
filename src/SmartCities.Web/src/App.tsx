import {
  Button,
  Dropdown,
  Field,
  Input,
  Option,
  Spinner,
  Text,
  Textarea,
  Title1,
} from '@fluentui/react-components';
import { useEffect, useMemo, useState, type FormEvent } from 'react';
import {
  ApiProblemError,
  loadMobilityReportOutcome,
  reportIdFromSearch,
  submitMobilityReport,
  trackingSearchForReport,
  type MobilityReportAcceptance,
  type MobilityReportOutcome,
} from './api';
import {
  loadLocalization,
  preferredCulture,
  text,
  type LocalizationBundle,
  type SupportedCulture,
} from './localization';
import { resourceKeys } from './resourceKeys';
import { AuthenticationEntry } from './AuthenticationEntry';

const categories = [
  {
    id: 'pedestrian-safety',
    resource: resourceKeys.pedestrianSafety,
  },
  {
    id: 'public-transport',
    resource: resourceKeys.publicTransport,
  },
  {
    id: 'road-safety',
    resource: resourceKeys.roadSafety,
  },
] as const;

type OutcomeLoadState =
  | 'idle'
  | 'loading'
  | 'ready'
  | 'not-found'
  | 'unavailable';

export function App() {
  const [culture, setCulture] = useState<SupportedCulture>(
    preferredCulture(navigator.language),
  );
  const [bundle, setBundle] = useState<LocalizationBundle | null>(null);
  const [category, setCategory] = useState('');
  const [location, setLocation] = useState('');
  const [description, setDescription] = useState('');
  const [acceptance, setAcceptance] =
    useState<MobilityReportAcceptance | null>(null);
  const [trackedReportId, setTrackedReportId] =
    useState<string | null>(
      () => reportIdFromSearch(
        window.location.search,
      ),
    );
  const [outcome, setOutcome] =
    useState<MobilityReportOutcome | null>(null);
  const [outcomeState, setOutcomeState] =
    useState<OutcomeLoadState>(
      trackedReportId
        ? navigator.onLine
          ? 'loading'
          : 'unavailable'
        : 'idle',
    );
  const [outcomeRefreshToken, setOutcomeRefreshToken] =
    useState(0);
  const [errorDetail, setErrorDetail] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [online, setOnline] = useState(navigator.onLine);

  useEffect(() => {
    const controller = new AbortController();
    setBundle(null);

    void loadLocalization(culture, controller.signal)
      .then(setBundle)
      .catch(() => setBundle(null));

    return () => controller.abort();
  }, [culture]);

  useEffect(() => {
    const handleOnline = () => setOnline(true);
    const handleOffline = () => setOnline(false);

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);

  useEffect(() => {
    if (!trackedReportId) {
      setOutcome(null);
      setOutcomeState('idle');
      return;
    }

    if (!bundle) {
      return;
    }

    if (!online) {
      setOutcome(null);
      setOutcomeState('unavailable');
      return;
    }

    const controller = new AbortController();

    setOutcome(null);
    setOutcomeState('loading');

    void loadMobilityReportOutcome(
      trackedReportId,
      bundle.resolvedCulture,
      (input, init) =>
        fetch(input, {
          ...init,
          signal: controller.signal,
        }),
    )
      .then((result) => {
        if (controller.signal.aborted) {
          return;
        }

        if (result === null) {
          setOutcome(null);
          setOutcomeState('not-found');
          return;
        }

        setOutcome(result);
        setOutcomeState('ready');
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setOutcome(null);
          setOutcomeState('unavailable');
        }
      });

    return () => controller.abort();
  }, [
    bundle,
    online,
    outcomeRefreshToken,
    trackedReportId,
  ]);

  const labels = useMemo(() => {
    if (!bundle) {
      return null;
    }

    return {
      appTitle: text(bundle, resourceKeys.appTitle),
      title: text(bundle, resourceKeys.title),
      intro: text(bundle, resourceKeys.intro),
      categoryLabel: text(bundle, resourceKeys.categoryLabel),
      categoryPlaceholder: text(bundle, resourceKeys.categoryPlaceholder),
      locationLabel: text(bundle, resourceKeys.locationLabel),
      locationPlaceholder: text(bundle, resourceKeys.locationPlaceholder),
      descriptionLabel: text(bundle, resourceKeys.descriptionLabel),
      descriptionPlaceholder: text(bundle, resourceKeys.descriptionPlaceholder),
      evidenceNote: text(bundle, resourceKeys.evidenceNote),
      submit: text(bundle, resourceKeys.submit),
      submitting: text(bundle, resourceKeys.submitting),
      offline: text(bundle, resourceKeys.offline),
      submitError: text(bundle, resourceKeys.submitError),
      successCreated: text(bundle, resourceKeys.successCreated),
      successExisting: text(bundle, resourceKeys.successExisting),
      caseLabel: text(bundle, resourceKeys.caseLabel),
      reportLabel: text(bundle, resourceKeys.reportLabel),
      outcomeTitle: text(bundle, resourceKeys.outcomeTitle),
      outcomeLoading: text(bundle, resourceKeys.outcomeLoading),
      outcomeUnavailable: text(bundle, resourceKeys.outcomeUnavailable),
      outcomeNotFound: text(bundle, resourceKeys.outcomeNotFound),
      outcomeRefresh: text(bundle, resourceKeys.outcomeRefresh),
    };
  }, [bundle]);

  if (!bundle || !labels) {
    return (
      <main className="app-loading" aria-busy="true">
        <Spinner />
      </main>
    );
  }

  const activeBundle = bundle;
  const copy = labels;

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!online || !category || !location.trim() || !description.trim()) {
      return;
    }

    setSubmitting(true);
    setAcceptance(null);
    setErrorDetail(null);

    try {
      const result = await submitMobilityReport(
        activeBundle.resolvedCulture,
        category,
        location.trim(),
        description.trim(),
      );

      setAcceptance(result);
      setOutcome(null);
      setOutcomeState('loading');
      setTrackedReportId(result.reportId);

      const search = trackingSearchForReport(
        window.location.search,
        result.reportId,
      );

      window.history.replaceState(
        window.history.state,
        '',
        `${window.location.pathname}${search}${window.location.hash}`,
      );
    } catch (error) {
      if (error instanceof ApiProblemError && error.problem.detail) {
        setErrorDetail(error.problem.detail);
      } else {
        setErrorDetail(copy.submitError);
      }
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <main className="container py-4 py-md-5 app-main">
      <header className="d-flex justify-content-between align-items-center gap-3 mb-5">
        <Text weight="semibold" size={500}>
          {labels.appTitle}
        </Text>
        <div className="d-flex gap-2" aria-label="locale">
          <Button
            appearance={culture === 'en' ? 'primary' : 'secondary'}
            size="small"
            onClick={() => setCulture('en')}
          >
            EN
          </Button>
          <Button
            appearance={culture === 'es-MX' ? 'primary' : 'secondary'}
            size="small"
            onClick={() => setCulture('es-MX')}
          >
            ES-MX
          </Button>
        </div>
      </header>

      <section className="row justify-content-center mb-4">
        <div className="col-12 col-lg-8 col-xl-7">
          <AuthenticationEntry
            bundle={bundle}
            online={online}
          />
        </div>
      </section>

      <section className="row justify-content-center">
        <div className="col-12 col-lg-8 col-xl-7">
          <div className="citizen-card">
            <Title1>{labels.title}</Title1>
            <Text block className="mt-2 mb-4" size={400}>
              {labels.intro}
            </Text>

            {trackedReportId && (
              <div
                className="status-message mb-4"
                aria-live="polite"
              >
                <Text block weight="semibold">
                  {labels.outcomeTitle}
                </Text>
                <Text block size={300}>
                  {labels.reportLabel}: {trackedReportId}
                </Text>

                {outcomeState === 'loading' && (
                  <div
                    className="d-flex align-items-center gap-2 mt-2"
                    role="status"
                  >
                    <Spinner size="tiny" />
                    <Text size={300}>
                      {labels.outcomeLoading}
                    </Text>
                  </div>
                )}

                {outcomeState === 'ready' && outcome && (
                  <div className="mt-2">
                    <Text block weight="semibold">
                      {outcome.statusLabel}
                    </Text>
                    <Text block size={300}>
                      {outcome.explanation}
                    </Text>
                    <Text block size={300}>
                      {labels.caseLabel}: {outcome.caseId}
                    </Text>
                  </div>
                )}

                {outcomeState === 'not-found' && (
                  <Text block className="mt-2" size={300}>
                    {labels.outcomeNotFound}
                  </Text>
                )}

                {outcomeState === 'unavailable' && (
                  <Text block className="mt-2" size={300}>
                    {labels.outcomeUnavailable}
                  </Text>
                )}

                <Button
                  className="mt-3"
                  appearance="secondary"
                  size="small"
                  disabled={!online || outcomeState === 'loading'}
                  onClick={() =>
                    setOutcomeRefreshToken(
                      (value) => value + 1,
                    )}
                >
                  {labels.outcomeRefresh}
                </Button>
              </div>
            )}

            <form onSubmit={handleSubmit} className="d-grid gap-4">
              <Field label={labels.categoryLabel} required>
                <Dropdown
                  placeholder={labels.categoryPlaceholder}
                  selectedOptions={category ? [category] : []}
                  onOptionSelect={(_, data) =>
                    setCategory(data.optionValue ?? '')}
                >
                  {categories.map((item) => (
                    <Option key={item.id} value={item.id}>
                      {text(bundle, item.resource)}
                    </Option>
                  ))}
                </Dropdown>
              </Field>

              <Field label={labels.locationLabel} required>
                <Input
                  value={location}
                  placeholder={labels.locationPlaceholder}
                  onChange={(_, data) => setLocation(data.value)}
                />
              </Field>

              <Field label={labels.descriptionLabel} required>
                <Textarea
                  resize="vertical"
                  value={description}
                  placeholder={labels.descriptionPlaceholder}
                  onChange={(_, data) => setDescription(data.value)}
                />
              </Field>

              <Text size={300}>{labels.evidenceNote}</Text>

              {!online && (
                <div role="status" className="status-message">
                  {labels.offline}
                </div>
              )}

              {errorDetail && (
                <div role="alert" className="status-message">
                  {errorDetail}
                </div>
              )}

              {acceptance && (
                <div role="status" className="status-message">
                  <Text block weight="semibold">
                    {acceptance.wasCreated
                      ? labels.successCreated
                      : labels.successExisting}
                  </Text>
                  <Text block>
                    {labels.caseLabel}: {acceptance.caseId}
                  </Text>
                </div>
              )}

              <div>
                <Button
                  appearance="primary"
                  type="submit"
                  disabled={
                    submitting
                    || !online
                    || !category
                    || !location.trim()
                    || !description.trim()
                  }
                >
                  {submitting ? labels.submitting : labels.submit}
                </Button>
              </div>
            </form>
          </div>
        </div>
      </section>
    </main>
  );
}
