import { useState } from 'react';
import { Button, Link } from '@fluentui/react-components';
import { type SiteLocale, siteCopy } from './copy';

const repositoryUrl = 'https://github.com/ChicoDotNet/SmartCities';

function initialLocale(): SiteLocale {
  return navigator.language.toLowerCase().startsWith('es') ? 'es-MX' : 'en';
}

export function App() {
  const [locale, setLocale] = useState<SiteLocale>(initialLocale);
  const copy = siteCopy[locale];

  const toggleLocale = () => setLocale((current) => (current === 'en' ? 'es-MX' : 'en'));

  return (
    <main>
      <section className="hero">
        <div className="container py-5">
          <div className="d-flex justify-content-between align-items-center gap-3 mb-5">
            <div className="brand-mark" aria-label="SmartCities">SC</div>
            <Button appearance="subtle" onClick={toggleLocale}>{copy.switchLanguage}</Button>
          </div>

          <div className="row align-items-center g-5">
            <div className="col-12 col-lg-7">
              <p className="eyebrow">{copy.eyebrow}</p>
              <h1>{copy.title}</h1>
              <p className="lead-copy">{copy.lead}</p>
              <div className="d-flex flex-wrap gap-3 mt-4">
                <Link href={repositoryUrl} target="_blank">{copy.repository}</Link>
                <Link href={`${repositoryUrl}/blob/dev/CONTRIBUTING.md`} target="_blank">{copy.contributeLink}</Link>
              </div>
            </div>
            <div className="col-12 col-lg-5">
              <div className="city-grid" aria-hidden="true">
                {Array.from({ length: 16 }, (_, index) => <span key={index} />)}
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="container py-5">
        <div className="row g-4">
          {[copy.openSource, copy.citizenFirst, copy.criterion, copy.humanAuthority].map((text, index) => (
            <div className="col-12 col-md-6" key={text}>
              <article className="principle-card h-100">
                <span className="card-index">0{index + 1}</span>
                <p>{text}</p>
              </article>
            </div>
          ))}
        </div>
      </section>

      <section className="modules py-5">
        <div className="container">
          <h2>{copy.modulesTitle}</h2>
          <div className="row g-3 mt-3">
            {copy.modules.map((module) => (
              <div className="col-12 col-md-6 col-lg-4" key={module}>
                <div className="module-pill h-100">{module}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="container py-5">
        <div className="row g-5">
          <div className="col-12 col-lg-6">
            <p className="eyebrow">Architecture</p>
            <h2>{copy.architectureTitle}</h2>
            <p>{copy.architecture}</p>
          </div>
          <div className="col-12 col-lg-6">
            <p className="eyebrow">Community</p>
            <h2>{copy.contributeTitle}</h2>
            <p>{copy.contribute}</p>
          </div>
        </div>
      </section>

      <footer className="container py-4">
        <span>SmartCities · AGPL-3.0-only · ChicoDotNet</span>
      </footer>
    </main>
  );
}
