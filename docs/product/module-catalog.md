# SmartCities module catalog

This catalog defines the intended product surface. It is an architectural capability map, not a claim that every module is implemented.

## 1. Citizen Experience plane

- **Citizen Portal / Digital Town Hall** — simple, accessible front door for civic services.
- **Requests & Cases** — reports, service requests, attachments, status, routing, SLA, and closure.
- **Participation** — consultations, proposals, surveys, public comment, and participatory processes.
- **Wayfinding & Journey** — orientation across walking, cycling, transit, stations, and public space.
- **Transparency & Decisions** — evidence, uncertainty, authority, alternatives, and final disposition.
- **Notifications** — case updates, service changes, closures, mobility alerts, and opt-in civic notifications.
- **Accessibility** — accessible interaction, plain language, assistive technology support, and inclusive mobility needs.

## 2. Urban Intelligence plane

- **City Context & GIS** — boundaries, zones, networks, corridors, land references, POI, and geospatial layers.
- **Evidence Hub** — versioned evidence references, provenance, quality metadata, documents, observations, and datasets.
- **Data Acquisition** — traffic/passenger counts, inventories, origin-destination surveys, stated-preference surveys, and field observations.
- **Photogrammetry & Field Survey** — georeferenced survey products and field-derived infrastructure evidence.
- **Demand Modeling** — trip, passenger, corridor, station, and network demand.
- **Traffic Modeling** — road-network assignment, capacity, and operational analysis.
- **Microsimulation** — intersections, corridors, stations, and multimodal environments.
- **Scenario & Alternatives** — comparable interventions with assumptions, constraints, and counterfactuals.
- **Technical, Economic, Social & Environmental Appraisal** — feasibility and impact analysis.
- **Audit & Traceability** — inputs, methods, assumptions, model versions, recommendations, and approvals.
- **Open Data & Indicators** — privacy-safe publishable indicators, metadata, and datasets.

## 3. Mobility & Public Realm plane

- **Integrated Mobility Planning**
- **Public Transport**
- **Rail Demand & Planning**
- **Interurban & Multimodal Mobility**
- **Cycling & Micromobility**
- **Pedestrian Mobility & Universal Accessibility**
- **Road Network & Highway Demand**
- **Road Safety**
- **Traffic Impact**
- **Public Space & Public Life**
- **Stations & Interchanges**
- **Urban Wayfinding**
- **Parking, Curb & Urban Freight**
- **Equipment Commissioning & Operations**
- **Specialized Training**

## 4. Extensible Smart City Domain plane

Architectural extension points not required for the first mobility-focused releases:

- **Land Use & Housing**
- **Environment & Climate**
- **Water**
- **Waste**
- **Energy & Public Lighting**
- **Urban Resilience**
- **Emergency & Civil Protection**
- **Digital Inclusion**
- **Local Economy & Tourism**

## 5. Cross-cutting platform modules

- **SmartCities.Contracts** — stable public contracts.
- **SmartCities.Criterion** — criterion abstraction, deterministic mock, future adapters.
- **SmartCities.HumanOversight** — review, authority, disposition, dissent, accountability.
- **SmartCities.Integrations** — standards and provider adapters that do not own domain logic.
- **SmartCities.Identity** — optional identity and authorization boundary.
- **SmartCities.Localization** — .NET resource sets and JSON resource delivery.
- **SmartCities.Observability** — telemetry, audit events, privacy-safe diagnostics.

## Module rule

A module owns a coherent public capability. It must not silently absorb proprietary expert knowledge merely because that knowledge could improve a recommendation.

The public suite owns **interfaces, data contracts, generic algorithms, interoperability, localization infrastructure, and observable behavior**.

Protected expertise belongs behind explicit external boundaries.
