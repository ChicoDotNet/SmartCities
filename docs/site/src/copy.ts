export type SiteLocale = 'en' | 'es-MX';

export interface SiteCopy {
  eyebrow: string;
  title: string;
  lead: string;
  openSource: string;
  citizenFirst: string;
  criterion: string;
  humanAuthority: string;
  modulesTitle: string;
  modules: readonly string[];
  architectureTitle: string;
  architecture: string;
  contributeTitle: string;
  contribute: string;
  repository: string;
  contributeLink: string;
  switchLanguage: string;
}

export const siteCopy: Record<SiteLocale, SiteCopy> = {
  en: {
    eyebrow: 'Open source from day zero',
    title: 'SmartCities',
    lead: 'Citizen-first urban technology with traceable evidence, explainable decision support, and explicit human authority.',
    openSource: 'Reusable product code, contracts, reference UI, tests, and engineering documentation are public under AGPL-v3.',
    citizenFirst: 'Citizens see simple services. Urban complexity stays behind the interface, but never disappears from the audit trail.',
    criterion: 'A public criterion port starts with a deterministic mock and can later connect to Criterio E-Kernel Core through NuGet.',
    humanAuthority: 'IAIA(oh) keeps recommendations observable by humans and preserves accountable authority for consequential civic decisions.',
    modulesTitle: 'A modular city platform',
    modules: [
      'Digital Town Hall and citizen cases',
      'Mobility, transit, cycling, walking and accessibility',
      'Road safety, traffic impact and microsimulation',
      'GIS, surveys, photogrammetry and evidence',
      'Technical, economic, social and environmental appraisal',
      'Public space, wayfinding and multimodal planning',
      'Future water, energy, waste, resilience and digital inclusion modules'
    ],
    architectureTitle: 'C# first. Rust when evidence demands it.',
    architecture: 'The repository starts with a .NET 10 class library. Performance-critical components may later use Rust through FerrumWeave after measurable need is demonstrated.',
    contributeTitle: 'Built in public',
    contribute: 'Architecture, governance, contribution rules, issues, CI, and the roadmap are open from the beginning.',
    repository: 'View repository',
    contributeLink: 'How to contribute',
    switchLanguage: 'Español (México)'
  },
  'es-MX': {
    eyebrow: 'Open Source desde el día cero',
    title: 'SmartCities',
    lead: 'Tecnología urbana centrada en el ciudadano, con evidencia trazable, decisiones explicables y autoridad humana explícita.',
    openSource: 'El código reusable, contratos, UI de referencia, pruebas y documentación de ingeniería son públicos bajo AGPL-v3.',
    citizenFirst: 'La ciudadanía ve servicios simples. La complejidad urbana queda detrás de la interfaz, pero nunca desaparece de la trazabilidad.',
    criterion: 'Un contrato público de criterio inicia con un mock determinista y después podrá conectar Criterio E-Kernel Core mediante NuGet.',
    humanAuthority: 'IAIA(oh) mantiene las recomendaciones observadas por humanos y preserva la autoridad responsable en decisiones públicas de consecuencia.',
    modulesTitle: 'Una plataforma urbana modular',
    modules: [
      'Ventanilla digital y casos ciudadanos',
      'Movilidad, transporte público, ciclismo, caminabilidad y accesibilidad',
      'Seguridad vial, impacto vial y microsimulación',
      'GIS, encuestas, fotogrametría y evidencia',
      'Evaluación técnica, económica, social y ambiental',
      'Espacio público, orientación e intermodalidad',
      'Futuros módulos de agua, energía, residuos, resiliencia e inclusión digital'
    ],
    architectureTitle: 'C# primero. Rust cuando la evidencia lo exija.',
    architecture: 'El repositorio inicia con una Class Library en .NET 10. Componentes críticos en rendimiento podrán usar Rust sobre FerrumWeave cuando exista una necesidad medible.',
    contributeTitle: 'Construido en público',
    contribute: 'Arquitectura, gobierno, reglas de contribución, issues, CI y roadmap están abiertos desde el principio.',
    repository: 'Ver repositorio',
    contributeLink: 'Cómo contribuir',
    switchLanguage: 'English'
  }
};
