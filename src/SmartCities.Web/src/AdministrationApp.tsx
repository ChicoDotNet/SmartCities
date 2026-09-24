import {
  Button,
  Dropdown,
  Field,
  Input,
  Option,
  Spinner,
  Switch,
  Text,
  Title1,
  Title2,
} from '@fluentui/react-components';
import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type FormEvent,
} from 'react';
import {
  addAdministrationWhitelistRule,
  AdministrationClientError,
  createAdministrationBootstrapSession,
  deleteAdministrationWhitelistRule,
  loadAdministrationAccess,
  loadAdministrationWhitelist,
  resolveAdministrationView,
  type AdministrationAccess,
  type AdministrationRuleKind,
  type AdministrationWhitelistRule,
} from './administration';
import { AuthenticationEntry } from './AuthenticationEntry';
import {
  loadCurrentSession,
  type AuthenticationSession,
} from './authentication';
import {
  loadFeatureFlagSnapshot,
  setFeatureFlag,
  type FeatureFlagSnapshot,
} from './features';
import {
  text,
  type LocalizationBundle,
  type SupportedCulture,
} from './localization';
import { resourceKeys } from './resourceKeys';

const bootstrapAccount =
  'townhalladmin@smartcities.local';
const manageFeatureFlags =
  'feature-flags.manage';
const manageWhitelist =
  'administration-whitelist.manage';

interface AdministrationAppProps {
  bundle: LocalizationBundle;
  culture: SupportedCulture;
  online: boolean;
  onCultureChange: (culture: SupportedCulture) => void;
}

type AdministrationState =
  | { status: 'checking' }
  | { status: 'unavailable' }
  | {
      status: 'ready';
      session: AuthenticationSession;
      access: AdministrationAccess;
    };

export function AdministrationApp({
  bundle,
  culture,
  online,
  onCultureChange,
}: AdministrationAppProps) {
  const [state, setState] =
    useState<AdministrationState>(
      online
        ? { status: 'checking' }
        : { status: 'unavailable' },
    );
  const [refreshToken, setRefreshToken] =
    useState(0);
  const [bootstrapPassword, setBootstrapPassword] =
    useState('');
  const [bootstrapSubmitting, setBootstrapSubmitting] =
    useState(false);
  const [error, setError] =
    useState<string | null>(null);

  const labels = useMemo(
    () => ({
      appTitle: text(bundle, resourceKeys.appTitle),
      title: text(bundle, resourceKeys.administrationTitle),
      intro: text(bundle, resourceKeys.administrationIntro),
      checking: text(bundle, resourceKeys.administrationChecking),
      unavailable: text(bundle, resourceKeys.administrationUnavailable),
      denied: text(bundle, resourceKeys.administrationDenied),
      deniedDetail: text(
        bundle,
        resourceKeys.administrationDeniedDetail,
      ),
      townHall: text(bundle, resourceKeys.administrationTownHall),
      citizenPortal: text(
        bundle,
        resourceKeys.administrationCitizenPortal,
      ),
      bootstrapTitle: text(
        bundle,
        resourceKeys.administrationBootstrapTitle,
      ),
      bootstrapIntro: text(
        bundle,
        resourceKeys.administrationBootstrapIntro,
      ),
      bootstrapAccount: text(
        bundle,
        resourceKeys.administrationBootstrapAccount,
      ),
      bootstrapPassword: text(
        bundle,
        resourceKeys.administrationBootstrapPassword,
      ),
      bootstrapSubmit: text(
        bundle,
        resourceKeys.administrationBootstrapSubmit,
      ),
      bootstrapSubmitting: text(
        bundle,
        resourceKeys.administrationBootstrapSubmitting,
      ),
      bootstrapInvalid: text(
        bundle,
        resourceKeys.administrationBootstrapInvalid,
      ),
      bootstrapUnavailable: text(
        bundle,
        resourceKeys.administrationBootstrapUnavailable,
      ),
      genericError: text(
        bundle,
        resourceKeys.administrationGenericError,
      ),
    }),
    [bundle],
  );

  useEffect(() => {
    if (!online) {
      setState({ status: 'unavailable' });
      return;
    }

    const controller = new AbortController();

    setState({ status: 'checking' });
    setError(null);

    void Promise.all([
      loadCurrentSession(
        (input, init) =>
          fetch(input, {
            ...init,
            signal: controller.signal,
          }),
      ),
      loadAdministrationAccess(
        (input, init) =>
          fetch(input, {
            ...init,
            signal: controller.signal,
          }),
      ),
    ])
      .then(([session, access]) => {
        if (!controller.signal.aborted) {
          setState({
            status: 'ready',
            session,
            access,
          });
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setState({ status: 'unavailable' });
        }
      });

    return () => controller.abort();
  }, [online, refreshToken]);

  async function handleBootstrap(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (!online || !bootstrapPassword) {
      return;
    }

    setBootstrapSubmitting(true);
    setError(null);

    try {
      await createAdministrationBootstrapSession(
        bootstrapAccount,
        bootstrapPassword,
      );
      setBootstrapPassword('');
      setRefreshToken((value) => value + 1);
    } catch (caught) {
      if (
        caught instanceof AdministrationClientError
        && caught.code ===
          'administration_bootstrap_invalid_credentials'
      ) {
        setError(labels.bootstrapInvalid);
      } else if (
        caught instanceof AdministrationClientError
        && caught.code ===
          'administration_bootstrap_unavailable'
      ) {
        setError(labels.bootstrapUnavailable);
        setRefreshToken((value) => value + 1);
      } else {
        setError(labels.genericError);
      }
    } finally {
      setBootstrapPassword('');
      setBootstrapSubmitting(false);
    }
  }

  const handleAuthorityChanged = useCallback(
    () => setRefreshToken(
      (value) => value + 1,
    ),
    [],
  );

  const view =
    state.status === 'ready'
      ? resolveAdministrationView(
          state.session,
          state.access,
        )
      : null;

  return (
    <main className="container py-4 py-md-5 app-main">
      <header className="d-flex justify-content-between align-items-center gap-3 mb-5">
        <div>
          <Text block weight="semibold" size={500}>
            {labels.appTitle}
          </Text>
          <Text block size={300}>
            {labels.title}
          </Text>
        </div>
        <div className="d-flex flex-wrap gap-2">
          <Button
            appearance="subtle"
            as="a"
            href="/"
          >
            {labels.citizenPortal}
          </Button>
          <Button
            appearance={culture === 'en' ? 'primary' : 'secondary'}
            size="small"
            onClick={() => onCultureChange('en')}
          >
            EN
          </Button>
          <Button
            appearance={culture === 'es-MX' ? 'primary' : 'secondary'}
            size="small"
            onClick={() => onCultureChange('es-MX')}
          >
            ES-MX
          </Button>
        </div>
      </header>

      {state.status === 'checking' && (
        <section className="administration-card" role="status">
          <Spinner />
          <Text>{labels.checking}</Text>
        </section>
      )}

      {state.status === 'unavailable' && (
        <section className="administration-card">
          <div className="status-message" role="alert">
            {labels.unavailable}
          </div>
        </section>
      )}

      {state.status === 'ready' && (
        <>
          <section className="administration-card mb-4">
            <Title1>{labels.title}</Title1>
            <Text block className="mt-2">
              {labels.intro}
            </Text>
            <Text block className="mt-3" size={300}>
              {labels.townHall}: {state.access.townHallId}
            </Text>
          </section>

          {view === 'bootstrap' && (
            <section className="administration-card">
              <Title2>{labels.bootstrapTitle}</Title2>
              <Text block className="mt-2 mb-4">
                {labels.bootstrapIntro}
              </Text>

              <form
                className="administration-form"
                onSubmit={handleBootstrap}
              >
                <Field label={labels.bootstrapAccount}>
                  <Input
                    value={bootstrapAccount}
                    readOnly
                  />
                </Field>

                <Field
                  label={labels.bootstrapPassword}
                  required
                >
                  <Input
                    type="password"
                    autoComplete="current-password"
                    value={bootstrapPassword}
                    onChange={(_, data) =>
                      setBootstrapPassword(data.value)}
                  />
                </Field>

                {error && (
                  <div
                    className="status-message"
                    role="alert"
                  >
                    {error}
                  </div>
                )}

                <Button
                  appearance="primary"
                  type="submit"
                  disabled={
                    bootstrapSubmitting
                    || !online
                    || !bootstrapPassword
                  }
                >
                  {bootstrapSubmitting
                    ? labels.bootstrapSubmitting
                    : labels.bootstrapSubmit}
                </Button>
              </form>
            </section>
          )}

          {view === 'sign-in' && (
            <AuthenticationEntry
              bundle={bundle}
              online={online}
              onSessionChanged={handleAuthorityChanged}
            />
          )}

          {view === 'denied' && (
            <div className="d-grid gap-4">
              <section className="administration-card">
                <Title2>{labels.denied}</Title2>
                <Text block className="mt-2">
                  {labels.deniedDetail}
                </Text>
              </section>
              <AuthenticationEntry
                bundle={bundle}
                online={online}
                onSessionChanged={() =>
                  setRefreshToken(
                    (value) => value + 1,
                  )}
              />
            </div>
          )}

          {view === 'authorized'
            && state.session.authenticated && (
              <AdministrationWorkspace
                bundle={bundle}
                online={online}
                session={state.session}
                bootstrapSession={
                  state.session.identityProvider
                    === 'local-bootstrap'
                }
                onAuthorityChanged={handleAuthorityChanged}
              />
            )}
        </>
      )}
    </main>
  );
}

interface AdministrationWorkspaceProps {
  bundle: LocalizationBundle;
  online: boolean;
  session: Extract<
    AuthenticationSession,
    { authenticated: true }
  >;
  bootstrapSession: boolean;
  onAuthorityChanged: () => void;
}

function AdministrationWorkspace({
  bundle,
  online,
  session,
  bootstrapSession,
  onAuthorityChanged,
}: AdministrationWorkspaceProps) {
  const hasAuthorityRole =
    session.authorityRoles.length > 0;
  const canManageFeatures =
    hasAuthorityRole
    && session.permissions.includes(
      manageFeatureFlags,
    );
  const canManageWhitelist =
    hasAuthorityRole
    && session.permissions.includes(
      manageWhitelist,
    );

  return (
    <div className="d-grid gap-4">
      <AuthenticationEntry
        bundle={bundle}
        online={online}
        onSessionChanged={onAuthorityChanged}
      />
      <FeatureManagementPanel
        bundle={bundle}
        online={online}
        canManage={canManageFeatures}
        onAuthorityChanged={onAuthorityChanged}
      />
      <WhitelistManagementPanel
        bundle={bundle}
        online={online}
        canManage={canManageWhitelist}
        bootstrapSession={bootstrapSession}
        onAuthorityChanged={onAuthorityChanged}
      />
    </div>
  );
}

interface FeatureManagementPanelProps {
  bundle: LocalizationBundle;
  online: boolean;
  canManage: boolean;
  onAuthorityChanged: () => void;
}

function FeatureManagementPanel({
  bundle,
  online,
  canManage,
  onAuthorityChanged,
}: FeatureManagementPanelProps) {
  const [snapshot, setSnapshot] =
    useState<FeatureFlagSnapshot | null>(null);
  const [loading, setLoading] = useState(true);
  const [savingFeature, setSavingFeature] =
    useState<string | null>(null);
  const [error, setError] =
    useState<string | null>(null);
  const [refreshToken, setRefreshToken] =
    useState(0);

  const labels = useMemo(
    () => ({
      title: text(
        bundle,
        resourceKeys.administrationFeaturesTitle,
      ),
      intro: text(
        bundle,
        resourceKeys.administrationFeaturesIntro,
      ),
      enabled: text(
        bundle,
        resourceKeys.administrationFeaturesEnabled,
      ),
      disabled: text(
        bundle,
        resourceKeys.administrationFeaturesDisabled,
      ),
      saving: text(
        bundle,
        resourceKeys.administrationFeaturesSaving,
      ),
      noPermission: text(
        bundle,
        resourceKeys.administrationFeaturesNoPermission,
      ),
      genericError: text(
        bundle,
        resourceKeys.administrationGenericError,
      ),
    }),
    [bundle],
  );

  useEffect(() => {
    if (!online) {
      setSnapshot(null);
      setLoading(false);
      setError(labels.genericError);
      return;
    }

    const controller = new AbortController();
    setLoading(true);
    setError(null);

    void loadFeatureFlagSnapshot(
      (input, init) =>
        fetch(input, {
          ...init,
          signal: controller.signal,
        }),
    )
      .then((result) => {
        if (!controller.signal.aborted) {
          setSnapshot(result);
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setSnapshot(null);
          setError(labels.genericError);
        }
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      });

    return () => controller.abort();
  }, [labels.genericError, online, refreshToken]);

  async function updateFeature(
    featureId: string,
    enabled: boolean,
  ) {
    if (!online || !canManage) {
      return;
    }

    setSavingFeature(featureId);
    setError(null);

    try {
      await setFeatureFlag(
        featureId,
        enabled,
      );
      setRefreshToken((value) => value + 1);
    } catch {
      setError(labels.genericError);
      onAuthorityChanged();
    } finally {
      setSavingFeature(null);
    }
  }

  return (
    <section className="administration-card">
      <Title2>{labels.title}</Title2>
      <Text block className="mt-2 mb-4">
        {labels.intro}
      </Text>

      {!canManage && (
        <div className="status-message mb-3">
          {labels.noPermission}
        </div>
      )}

      {loading && <Spinner size="small" />}

      {error && (
        <div className="status-message" role="alert">
          {error}
        </div>
      )}

      {snapshot?.features.map((feature) => (
        <div
          key={feature.featureId}
          className="administration-row"
        >
          <div>
            <Text block weight="semibold">
              {feature.featureId}
            </Text>
            <Text block size={300}>
              {feature.enabled
                ? labels.enabled
                : labels.disabled}
            </Text>
          </div>
          <Switch
            checked={feature.enabled}
            disabled={
              !canManage
              || !online
              || savingFeature === feature.featureId
            }
            aria-label={feature.featureId}
            onChange={(_, data) =>
              void updateFeature(
                feature.featureId,
                data.checked,
              )}
          />
          {savingFeature === feature.featureId && (
            <Text size={200}>{labels.saving}</Text>
          )}
        </div>
      ))}
    </section>
  );
}

interface WhitelistManagementPanelProps {
  bundle: LocalizationBundle;
  online: boolean;
  canManage: boolean;
  bootstrapSession: boolean;
  onAuthorityChanged: () => void;
}

function WhitelistManagementPanel({
  bundle,
  online,
  canManage,
  bootstrapSession,
  onAuthorityChanged,
}: WhitelistManagementPanelProps) {
  const [rules, setRules] =
    useState<AdministrationWhitelistRule[] | null>(
      null,
    );
  const [kind, setKind] =
    useState<AdministrationRuleKind>(
      'email-domain',
    );
  const [value, setValue] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] =
    useState<string | null>(null);
  const [refreshToken, setRefreshToken] =
    useState(0);

  const labels = useMemo(
    () => ({
      title: text(
        bundle,
        resourceKeys.administrationWhitelistTitle,
      ),
      intro: text(
        bundle,
        resourceKeys.administrationWhitelistIntro,
      ),
      kind: text(
        bundle,
        resourceKeys.administrationWhitelistKind,
      ),
      domain: text(
        bundle,
        resourceKeys.administrationWhitelistDomain,
      ),
      email: text(
        bundle,
        resourceKeys.administrationWhitelistEmail,
      ),
      subject: text(
        bundle,
        resourceKeys.administrationWhitelistSubject,
      ),
      value: text(
        bundle,
        resourceKeys.administrationWhitelistValue,
      ),
      add: text(
        bundle,
        resourceKeys.administrationWhitelistAdd,
      ),
      adding: text(
        bundle,
        resourceKeys.administrationWhitelistAdding,
      ),
      remove: text(
        bundle,
        resourceKeys.administrationWhitelistRemove,
      ),
      empty: text(
        bundle,
        resourceKeys.administrationWhitelistEmpty,
      ),
      noPermission: text(
        bundle,
        resourceKeys.administrationWhitelistNoPermission,
      ),
      bootstrapHint: text(
        bundle,
        resourceKeys.administrationWhitelistBootstrapHint,
      ),
      genericError: text(
        bundle,
        resourceKeys.administrationGenericError,
      ),
    }),
    [bundle],
  );

  useEffect(() => {
    if (!canManage || !online) {
      setRules(null);
      return;
    }

    const controller = new AbortController();
    setError(null);

    void loadAdministrationWhitelist(
      (input, init) =>
        fetch(input, {
          ...init,
          signal: controller.signal,
        }),
    )
      .then((result) => {
        if (!controller.signal.aborted) {
          setRules(result);
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setRules(null);
          setError(labels.genericError);
          onAuthorityChanged();
        }
      });

    return () => controller.abort();
  }, [
    canManage,
    labels.genericError,
    online,
    onAuthorityChanged,
    refreshToken,
  ]);

  async function addRule(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (
      !canManage
      || !online
      || !value.trim()
    ) {
      return;
    }

    setBusy(true);
    setError(null);

    try {
      await addAdministrationWhitelistRule(
        kind,
        value.trim(),
      );
      setValue('');
      onAuthorityChanged();

      if (!bootstrapSession) {
        setRefreshToken((current) => current + 1);
      }
    } catch {
      setError(labels.genericError);
      onAuthorityChanged();
    } finally {
      setBusy(false);
    }
  }

  async function removeRule(
    ruleId: string,
  ) {
    if (!canManage || !online || busy) {
      return;
    }

    setBusy(true);
    setError(null);

    try {
      await deleteAdministrationWhitelistRule(
        ruleId,
      );
      onAuthorityChanged();
      setRefreshToken((current) => current + 1);
    } catch {
      setError(labels.genericError);
      onAuthorityChanged();
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="administration-card">
      <Title2>{labels.title}</Title2>
      <Text block className="mt-2 mb-4">
        {labels.intro}
      </Text>

      {!canManage && (
        <div className="status-message">
          {labels.noPermission}
        </div>
      )}

      {canManage && bootstrapSession && (
        <div className="status-message mb-4">
          {labels.bootstrapHint}
        </div>
      )}

      {canManage && (
        <>
          <form
            className="administration-form mb-4"
            onSubmit={addRule}
          >
            <Field label={labels.kind}>
              <Dropdown
                value={
                  kind === 'email-domain'
                    ? labels.domain
                    : kind === 'email'
                      ? labels.email
                      : labels.subject
                }
                selectedOptions={[kind]}
                onOptionSelect={(_, data) => {
                  const selected =
                    data.optionValue;
                  if (
                    selected === 'email-domain'
                    || selected === 'email'
                    || selected === 'canonical-subject'
                  ) {
                    setKind(selected);
                  }
                }}
              >
                <Option value="email-domain">
                  {labels.domain}
                </Option>
                <Option value="email">
                  {labels.email}
                </Option>
                <Option value="canonical-subject">
                  {labels.subject}
                </Option>
              </Dropdown>
            </Field>

            <Field label={labels.value} required>
              <Input
                value={value}
                onChange={(_, data) =>
                  setValue(data.value)}
              />
            </Field>

            <Button
              appearance="primary"
              type="submit"
              disabled={
                busy
                || !online
                || !value.trim()
              }
            >
              {busy
                ? labels.adding
                : labels.add}
            </Button>
          </form>

          {error && (
            <div
              className="status-message mb-3"
              role="alert"
            >
              {error}
            </div>
          )}

          {rules === null && !error && (
            <Spinner size="small" />
          )}

          {rules?.length === 0 && (
            <Text>{labels.empty}</Text>
          )}

          {rules?.map((rule) => (
            <div
              key={rule.ruleId}
              className="administration-row"
            >
              <div>
                <Text block weight="semibold">
                  {rule.kind}
                </Text>
                <Text block size={300}>
                  {rule.value}
                </Text>
              </div>
              <Button
                appearance="secondary"
                disabled={busy || !online}
                onClick={() =>
                  void removeRule(rule.ruleId)}
              >
                {labels.remove}
              </Button>
            </div>
          ))}
        </>
      )}
    </section>
  );
}
