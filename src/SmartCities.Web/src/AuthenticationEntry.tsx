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
  type AuthenticationProviderDiscovery,
  type LocalSession,
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

export function AuthenticationEntry({
  bundle,
  online,
}: AuthenticationEntryProps) {
  const [providers, setProviders] = useState<
    AuthenticationProviderDiscovery[] | null
  >(null);
  const [discoveryFailed, setDiscoveryFailed] =
    useState(false);
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [signingIn, setSigningIn] = useState(false);
  const [session, setSession] =
    useState<LocalSession | null>(null);
  const [localError, setLocalError] =
    useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

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
  }, []);

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
    }),
    [bundle],
  );

  const localProvider = providers?.find(
    (provider) => provider.loginMode === 'credentials',
  );
  const redirectProviders = providers?.filter(
    (provider) => provider.loginMode === 'redirect',
  ) ?? [];

  async function handleLocalSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();

    if (
      !online
      || !localProvider?.sessionPath
      || !userName.trim()
      || !password
    ) {
      return;
    }

    setSigningIn(true);
    setLocalError(null);

    try {
      const authenticated = await createLocalSession(
        localProvider.sessionPath,
        userName.trim(),
        password,
      );

      setSession(authenticated);
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

  function handleRedirect(
    provider: AuthenticationProviderDiscovery,
  ) {
    if (!online) {
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

  return (
    <div className="authentication-card">
      <div className="d-flex flex-column gap-2">
        <Title2>{labels.title}</Title2>
        <Text size={300}>
          {labels.intro}
        </Text>
      </div>

      {providers === null && (
        <div
          className="authentication-loading"
          role="status"
        >
          <Spinner size="tiny" />
          <Text size={300}>{labels.loading}</Text>
        </div>
      )}

      {discoveryFailed && (
        <div
          role="alert"
          className="status-message mt-3"
        >
          {labels.unavailable}
        </div>
      )}

      {providers !== null
        && !discoveryFailed
        && providers.length === 0 && (
          <div
            role="status"
            className="status-message mt-3"
          >
            {labels.noProviders}
          </div>
        )}

      {session && (
        <div
          role="status"
          className="status-message mt-3"
        >
          <Text block weight="semibold">
            {labels.success}
          </Text>
          <Text block size={300}>
            {session.subjectId}
          </Text>
        </div>
      )}

      {!session && localProvider && (
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

      {redirectProviders.length > 0 && (
        <div
          className="authentication-provider-list mt-4"
          aria-label={labels.title}
        >
          {redirectProviders.map((provider) => (
            <Button
              key={provider.providerId}
              appearance="secondary"
              disabled={!online}
              onClick={() => handleRedirect(provider)}
            >
              {labels.continueWith} {provider.displayName}
            </Button>
          ))}
        </div>
      )}
    </div>
  );
}
