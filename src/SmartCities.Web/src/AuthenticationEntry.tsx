import {
  Button,
  Field,
  Input,
  Spinner,
  Text,
  Title2,
} from '@fluentui/react-components';
import {
  useEffect,
  useMemo,
  useState,
  type FormEvent,
} from 'react';
import {
  AuthenticationClientError,
  buildAuthenticationChallengeUrl,
  createLocalSession,
  loadAuthenticationProviders,
  loadCurrentSession,
  signOutCurrentSession,
  type AuthenticatedAuthenticationSession,
  type AuthenticationProviderDiscovery,
} from './authentication';
import {
  text,
  type LocalizationBundle,
} from './localization';
import { resourceKeys } from './resourceKeys';

interface AuthenticationEntryProps {
  bundle: LocalizationBundle;
  online: boolean;
}

type SessionState =
  | { status: 'checking' }
  | { status: 'anonymous' }
  | {
      status: 'authenticated';
      session: AuthenticatedAuthenticationSession;
    }
  | { status: 'unavailable' };

export function AuthenticationEntry({
  bundle,
  online,
}: AuthenticationEntryProps) {
  const [sessionState, setSessionState] =
    useState<SessionState>(
      online
        ? { status: 'checking' }
        : { status: 'unavailable' },
    );
  const [providers, setProviders] = useState<
    AuthenticationProviderDiscovery[] | null
  >(null);
  const [discoveryFailed, setDiscoveryFailed] =
    useState(false);
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [signingIn, setSigningIn] = useState(false);
  const [signingOut, setSigningOut] = useState(false);
  const [localError, setLocalError] =
    useState<string | null>(null);

  useEffect(() => {
    if (!online) {
      setSessionState({ status: 'unavailable' });
      setProviders(null);
      setDiscoveryFailed(false);
      return;
    }

    const controller = new AbortController();

    setSessionState({ status: 'checking' });
    setProviders(null);
    setDiscoveryFailed(false);
    setLocalError(null);

    void loadCurrentSession(
      (input, init) =>
        fetch(input, {
          ...init,
          signal: controller.signal,
        }),
    )
      .then((session) => {
        if (controller.signal.aborted) {
          return;
        }

        setSessionState(
          session.authenticated
            ? {
                status: 'authenticated',
                session,
              }
            : { status: 'anonymous' },
        );
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setSessionState({
            status: 'unavailable',
          });
        }
      });

    return () => controller.abort();
  }, [online]);

  useEffect(() => {
    if (
      !online
      || sessionState.status !== 'anonymous'
    ) {
      setProviders(null);
      setDiscoveryFailed(false);
      return;
    }

    const controller = new AbortController();

    setProviders(null);
    setDiscoveryFailed(false);

    void loadAuthenticationProviders(
      (input, init) =>
        fetch(input, {
          ...init,
          signal: controller.signal,
        }),
    )
      .then((result) => {
        if (!controller.signal.aborted) {
          setProviders(result);
          setDiscoveryFailed(false);
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setProviders([]);
          setDiscoveryFailed(true);
        }
      });

    return () => controller.abort();
  }, [online, sessionState.status]);

  const labels = useMemo(
    () => ({
      title: text(bundle, resourceKeys.authenticationTitle),
      intro: text(bundle, resourceKeys.authenticationIntro),
      loading: text(bundle, resourceKeys.authenticationLoading),
      unavailable: text(bundle, resourceKeys.authenticationUnavailable),
      noProviders: text(bundle, resourceKeys.authenticationNoProviders),
      userName: text(bundle, resourceKeys.authenticationUserName),
      password: text(bundle, resourceKeys.authenticationPassword),
      localSubmit: text(bundle, resourceKeys.authenticationLocalSubmit),
      localSubmitting: text(bundle, resourceKeys.authenticationLocalSubmitting),
      invalidCredentials: text(
        bundle,
        resourceKeys.authenticationInvalidCredentials,
      ),
      genericError: text(bundle, resourceKeys.authenticationGenericError),
      success: text(bundle, resourceKeys.authenticationSuccess),
      continueWith: text(bundle, resourceKeys.authenticationContinueWith),
      sessionTitle: text(
        bundle,
        resourceKeys.authenticationSessionTitle,
      ),
      sessionChecking: text(
        bundle,
        resourceKeys.authenticationSessionChecking,
      ),
      sessionUnavailable: text(
        bundle,
        resourceKeys.authenticationSessionUnavailable,
      ),
      logout: text(
        bundle,
        resourceKeys.authenticationSessionLogout,
      ),
      loggingOut: text(
        bundle,
        resourceKeys.authenticationSessionLoggingOut,
      ),
    }),
    [bundle],
  );

  const localProvider =
    sessionState.status === 'anonymous'
      ? providers?.find(
          (provider) =>
            provider.loginMode === 'credentials',
        )
      : undefined;
  const redirectProviders =
    sessionState.status === 'anonymous'
      ? providers?.filter(
          (provider) =>
            provider.loginMode === 'redirect',
        ) ?? []
      : [];

  async function handleLocalSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (
      !online
      || sessionState.status !== 'anonymous'
      || !localProvider?.sessionPath
      || !userName.trim()
      || !password
    ) {
      return;
    }

    setSigningIn(true);
    setLocalError(null);

    try {
      await createLocalSession(
        localProvider.sessionPath,
        userName.trim(),
        password,
      );

      const authoritative =
        await loadCurrentSession();

      if (!authoritative.authenticated) {
        throw new AuthenticationClientError(
          'authentication_session_not_established',
        );
      }

      setSessionState({
        status: 'authenticated',
        session: authoritative,
      });
      setUserName('');
    } catch (error) {
      if (
        error instanceof AuthenticationClientError
        && error.code === 'authentication_invalid_credentials'
      ) {
        setLocalError(labels.invalidCredentials);
      } else {
        setLocalError(labels.genericError);
      }
    } finally {
      setPassword('');
      setSigningIn(false);
    }
  }

  async function handleLogout() {
    if (
      !online
      || sessionState.status !== 'authenticated'
    ) {
      return;
    }

    setSigningOut(true);
    setLocalError(null);

    try {
      await signOutCurrentSession();

      const authoritative =
        await loadCurrentSession();

      if (authoritative.authenticated) {
        throw new AuthenticationClientError(
          'authentication_logout_not_effective',
        );
      }

      setSessionState({ status: 'anonymous' });
    } catch {
      setLocalError(labels.genericError);
    } finally {
      setSigningOut(false);
    }
  }

  function handleRedirect(
    provider: AuthenticationProviderDiscovery,
  ) {
    if (
      !online
      || sessionState.status !== 'anonymous'
    ) {
      return;
    }

    try {
      const returnUrl =
        `${window.location.pathname}${window.location.search}${window.location.hash}`;
      const challengeUrl =
        buildAuthenticationChallengeUrl(
          provider,
          returnUrl || '/',
        );

      window.location.assign(challengeUrl);
    } catch {
      setLocalError(labels.genericError);
    }
  }

  const title =
    sessionState.status === 'authenticated'
      ? labels.sessionTitle
      : sessionState.status === 'checking'
        ? labels.sessionChecking
        : sessionState.status === 'unavailable'
          ? labels.sessionUnavailable
          : labels.title;

  return (
    <div className="authentication-card">
      <div className="d-flex flex-column gap-2">
        <Title2>{title}</Title2>
        {sessionState.status === 'anonymous' && (
          <Text size={300}>
            {labels.intro}
          </Text>
        )}
      </div>

      {sessionState.status === 'checking' && (
        <div
          className="authentication-loading"
          role="status"
        >
          <Spinner size="tiny" />
        </div>
      )}

      {sessionState.status === 'unavailable' && (
        <div
          role="alert"
          className="status-message mt-3"
        >
          {labels.unavailable}
        </div>
      )}

      {sessionState.status === 'anonymous'
        && providers === null && (
          <div
            className="authentication-loading"
            role="status"
          >
            <Spinner size="tiny" />
            <Text size={300}>{labels.loading}</Text>
          </div>
        )}

      {sessionState.status === 'anonymous'
        && discoveryFailed && (
          <div
            role="alert"
            className="status-message mt-3"
          >
            {labels.unavailable}
          </div>
        )}

      {sessionState.status === 'anonymous'
        && providers !== null
        && !discoveryFailed
        && providers.length === 0 && (
          <div
            role="status"
            className="status-message mt-3"
          >
            {labels.noProviders}
          </div>
        )}

      {sessionState.status === 'authenticated' && (
        <div
          role="status"
          className="status-message mt-3"
        >
          <Text block weight="semibold">
            {labels.success}
          </Text>
          <Text block size={300}>
            {sessionState.session.subjectId}
          </Text>
          {localError && (
            <Text block role="alert" size={300}>
              {localError}
            </Text>
          )}
          <Button
            className="mt-3"
            appearance="secondary"
            disabled={signingOut || !online}
            onClick={() => void handleLogout()}
          >
            {signingOut
              ? labels.loggingOut
              : labels.logout}
          </Button>
        </div>
      )}

      {sessionState.status === 'anonymous'
        && localProvider && (
          <form
            className="authentication-local-form mt-4"
            onSubmit={handleLocalSubmit}
          >
            <Field label={labels.userName} required>
              <Input
                autoComplete="username"
                value={userName}
                onChange={(_, data) =>
                  setUserName(data.value)}
              />
            </Field>

            <Field label={labels.password} required>
              <Input
                type="password"
                autoComplete="current-password"
                value={password}
                onChange={(_, data) =>
                  setPassword(data.value)}
              />
            </Field>

            {localError && (
              <div
                role="alert"
                className="status-message"
              >
                {localError}
              </div>
            )}

            <Button
              appearance="primary"
              type="submit"
              disabled={
                signingIn
                || !online
                || !userName.trim()
                || !password
              }
            >
              {signingIn
                ? labels.localSubmitting
                : labels.localSubmit}
            </Button>
          </form>
        )}

      {sessionState.status === 'anonymous'
        && redirectProviders.length > 0 && (
          <div
            className="authentication-provider-list mt-4"
            aria-label={labels.title}
          >
            {redirectProviders.map((provider) => (
              <Button
                key={provider.providerId}
                appearance="secondary"
                disabled={!online}
                onClick={() =>
                  handleRedirect(provider)}
              >
                {labels.continueWith}{' '}
                {provider.displayName}
              </Button>
            ))}
          </div>
        )}
    </div>
  );
}
