# SmartCities

[English (canonical)](README.md) · **Español (México)**

**Una suite Open Source, centrada en el ciudadano, para construir mejores interfaces entre las ciudades, su evidencia y las personas que toman decisiones.**

SmartCities nace como software **100% Open Source desde el principio**, bajo AGPL-v3.

> La ciudadanía debe ver servicios simples. La complejidad urbana debe quedar trazable detrás de esos servicios.

## ¿Qué queremos construir?

Una suite modular que pueda cubrir progresivamente ventanilla digital ciudadana; reportes y seguimiento de casos; participación pública; movilidad integral; transporte público; ciclismo y micromovilidad; movilidad peatonal y accesibilidad; seguridad vial; estudios de impacto vial; espacio público y vida pública; demanda vial y ferroviaria; intermodalidad; microsimulación; evaluación técnica, económica, social y ambiental; GIS, encuestas, levantamientos y fotogrametría; indicadores y datos abiertos; y otros dominios de Smart City.

## IAIA(oh)

Usamos el concepto **Inteligencia Aumentada por IA, observada por humanos**. La IA ayuda a organizar evidencia, comparar alternativas, señalar incertidumbres y producir recomendaciones explicables. La autoridad humana responsable permanece explícita.

## Criterio E-Kernel

Durante el bootstrap usaremos un contrato público y un mock determinista. Cuando exista el primer paquete NuGet de **Criterio E-Kernel Core**, podremos conectarlo mediante un adaptador sin contaminar los módulos ciudadanos con detalles del motor.

## Tecnología

El núcleo inicia como una **Class Library en C# / .NET 10**.

La UI futura utilizará React + TypeScript (`.tsx` / `.ts`), Fluent UI y Bootstrap para layout/utilidades. La localización de producto tendrá su fuente canónica en recursos `.resx` del backend y llegará a React como JSON.

Rust sólo entrará cuando exista evidencia de una necesidad real de rendimiento, concurrencia, memoria o seguridad, y la integración .NET se hará preferentemente sobre **FerrumWeave**.

## Participa

Consulta [CONTRIBUTING.md](CONTRIBUTING.md), [GOVERNANCE.md](GOVERNANCE.md) y [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

La documentación técnica canónica está escrita en inglés para que el proyecto pueda crecer internacionalmente.
