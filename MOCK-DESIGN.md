# Mock Design

## Purpose

This document evaluates whether the vision described in `PROBLEM.md`, `TECHNICAL.md`, and `VISION.md` can be turned into a coherent C# system without prematurely locking into low-level implementation details.

The intent is to answer a high-level question:

Can a source-generator-driven semantic value object architecture realistically support richer constraints, composite symbolic values, lifecycle-aware types, and strongly typed localization in one cohesive system?

## Executive Assessment

The proposed system is conceptually viable, but only if it is framed as a layered semantic modelling platform rather than a single magical value object abstraction.

The strongest conclusion is:

- Composite structured values are highly viable.
- Generated constrained value types are viable, but many guarantees will remain construction-time or analyzer-assisted rather than purely compile-time.
- Strongly typed localization is viable if localization definitions are static and available at build time.
- Lifecycle/stateful value modelling is viable, but it is the riskiest area in terms of API complexity, state explosion, and ergonomics.

The overall direction makes sense, but the repository should treat this as an exploration of multiple related capability tracks sharing a common generation and diagnostics platform, not as one monolithic feature.

## Core Architectural Position

The system should be organized around three cooperating layers:

1. Authoring layer
   Developers declare semantic intent in a concise form.
   This is where constraints, component definitions, lifecycle states, and localization contracts are described.

2. Build-time intelligence layer
   Source generators synthesize code.
   Roslyn analyzers validate declarations, surface diagnostics, and enforce usage rules that generators alone cannot guarantee.

3. Runtime semantic layer
   A small runtime library provides invariant enforcement, parsing/formatting contracts, transition helpers, and integration points for serialization and localization.

This separation is important because the desired guarantees do not all come from the same mechanism:

- Generators are good at producing boilerplate and strongly typed APIs.
- Analyzers are good at validating declarations and catching misuse patterns.
- Runtime code is still necessary for values originating outside the compiler, such as user input, files, network payloads, and database data.

## What the System Is

At a high level, this should become a semantic modelling toolkit for C# that lets developers define domain-specific value concepts declaratively and receive:

- Generated types
- Generated parse/format behavior
- Generated equality and access patterns
- Generated localization bindings
- Diagnostics for invalid declarations or invalid usage patterns

The center of gravity should be semantic intent, not primitive wrapping.

## What the System Is Not

To stay viable, the system should not assume:

- Arbitrary semantic constraints can always be proven at compile time
- One representation style will fit every kind of semantic type
- Stateful lifecycle resources can be modelled with the same ergonomics as immutable scalar values
- Cryptographic correctness comes from typing alone
- Localization can be fully type-safe if resource definitions remain dynamic or external to the build

These are important guardrails. Overstating the static guarantees would create an architecture that is conceptually attractive but practically brittle.

## Capability Areas

### 1. Constraint-Rich Value Objects

This area is viable and likely foundational.

Examples such as positive numbers, natural numbers, prefix-constrained identifiers, and URL-safe symbolic strings fit well into a generated semantic type system.

Expected guarantee model:

- Declaration-time validation:
  Invalid authoring metadata should fail fast through diagnostics.
- Construction-time validation:
  Arbitrary runtime inputs still need validation at creation boundaries.
- Usage-time ergonomics:
  Generated APIs can make correct usage easy and incorrect usage noisy.

Key implication:

The design should distinguish between "compile-time validated definitions" and "compile-time proven values." The former is broadly feasible. The latter is only feasible for limited cases such as literals or constrained composition patterns recognized by analyzers.

### 2. Composite Structured Symbolic Values

This is one of the best early targets and appears very coherent with source generation.

Values shaped like `word::word::word` are a strong fit for:

- Declarative component definitions
- Generated parsing and formatting
- Generated named accessors
- Per-component validation
- Serialization support
- Equality and deconstruction

This capability has a clear authoring story, clear generated output, and a strong payoff in reduced handwritten infrastructure.

It is also a good proving ground because it naturally exercises:

- Constraint composition
- Generated parsing pipelines
- Diagnostics
- Domain ergonomics

### 3. Stateful Lifecycle Modelling

This area is viable, but it should be treated as a separate design track rather than a trivial extension of constrained value objects.

Why it is harder:

- Lifecycle models often describe resources, not just values.
- State transitions can multiply API surface area quickly.
- The type system can model legal transitions, but the resulting APIs may become heavy if every state becomes a distinct generated surface.

The safest conceptual direction is:

- Model lifecycle concepts as generated state-specific types or views
- Make transitions explicit
- Avoid pretending that mutable runtime resources can be made purely value-like

This can still fit the broader architecture, but it should not define the baseline design for every other semantic type.

### 4. Strongly Typed Localization

This area is viable if the source of truth for localization is build-time discoverable.

The best conceptual fit is:

- Static localization definitions
- Generated typed accessors
- Generated parameter contracts
- Culture-aware rendering routed through a runtime formatting layer

The biggest architectural dependency is the authoring model. If localization content is authored in static definitions that the generator can inspect, strong typing is realistic. If keys and templates remain mostly dynamic, the guarantees weaken significantly.

Localization also has a different problem shape than scalar values:

- Keys and templates are catalog-like assets
- Parameter typing is API-shape validation
- Formatting behavior spans culture and rendering policies

That makes localization a strong candidate for a sibling subsystem built on the same generation/diagnostics platform, not necessarily the same domain abstraction as numeric or symbolic values.

## Recommended High-Level System Shape

The platform should be treated as a family of related generation capabilities sharing common conventions and infrastructure.

Proposed major components:

- Semantic core
  Minimal runtime contracts, parsing abstractions, validation result patterns, formatting hooks, and shared marker abstractions.

- Generator layer
  Emits constrained value types, composite symbolic types, localization bindings, and state-specific wrappers or views.

- Analyzer layer
  Validates authoring declarations, catches misuse, and reinforces the limits of what can be checked before runtime.

- Integration layer
  Optional adapters for serialization, dependency injection, localization resource loading, or other ecosystem boundaries.

- Samples and experiments
  Small end-to-end demonstrations proving each semantic track independently before combining them.

## Proposed Repository Layout

The current repository is still early enough to stay flexible. A high-level structure like the following would support the exploration well:

```text
/docs
  PROBLEM.md
  TECHNICAL.md
  VISION.md
  MOCK-DESIGN.md

/src
  /Semantic.Core
  /Semantic.Generators
  /Semantic.Analyzers
  /Semantic.Localization
  /Semantic.Integrations

/samples
  /ConstraintValuesSample
  /CompositeValuesSample
  /LifecycleSample
  /LocalizationSample

/experiments
  /AuthoringModels
  /StateModelingTrials
  /DiagnosticsTrials
```

This layout is only directional. The key idea is to separate:

- shared runtime concerns
- build-time code generation
- diagnostics
- optional integrations
- exploratory spikes

## High-Level Workflow

The intended development flow could look like this:

1. A developer authors a semantic declaration.
2. An analyzer validates whether the declaration is coherent.
3. A generator emits the strongly typed API surface and supporting infrastructure.
4. The consuming application uses generated types and APIs.
5. Runtime boundaries validate external raw input where compile-time certainty is impossible.

This flow works well for constrained values and composite symbolic values.

For lifecycle modelling, the flow expands:

1. A developer defines states, operations, and allowed transitions.
2. Build-time tooling generates state-specific surfaces and transition helpers.
3. The runtime layer enforces actual resource behavior and failure handling.

For localization:

1. A developer defines a catalog or typed localization contract.
2. Tooling validates keys and parameter signatures.
3. Generated bindings expose typed localization calls.
4. Runtime formatting applies culture-aware rendering.

## Boundaries and Responsibilities

To keep the architecture coherent, each concern should have a clear responsibility boundary.

Authoring declarations should define:

- semantic intent
- constraints
- structural composition
- lifecycle shapes
- localization contracts

Generators should define:

- emitted code
- ergonomics
- boilerplate elimination
- discoverable generated APIs

Analyzers should define:

- diagnostics
- misuse detection
- unsupported pattern warnings
- authoring guidance

Runtime libraries should define:

- invariant checks for external inputs
- parsing and formatting behavior
- culture-aware rendering
- transition execution support
- adapter points for external frameworks

## Major Risks

### 1. Overpromising Compile-Time Guarantees

This is the most important conceptual risk.

Many of the desired guarantees are not fully compile-time in the strict sense. For example:

- A positive integer from user input cannot be statically proven positive.
- A cryptographic payload cannot be made secure by type modelling alone.
- A state transition may be type-legal but still fail at runtime due to environmental conditions.

The architecture remains valid, but the guarantee model must be described precisely:

- compile-time validated declarations
- analyzer-enforced usage patterns
- construction-time and transition-time runtime enforcement

### 2. One Abstraction Trying to Fit Too Much

Constraint values, composite symbolic structures, lifecycle resources, and localization catalogs are related, but not identical.

If the design forces them under one overly unified abstraction, the result may become confusing or brittle.

A better direction is a shared platform with specialized tracks.

### 3. Stateful API Explosion

Lifecycle-aware generation can create many types, transitions, and edge-case branches.

Without discipline, this becomes:

- hard to learn
- hard to debug
- hard to maintain
- visually noisy in generated APIs

This track needs especially careful scoping and should probably follow successful constrained/composite experiments rather than lead them.

### 4. Authoring Model Complexity

The entire platform depends on how developers declare intent.

If authoring becomes too verbose, magical, or hard to reason about, the generated output will not justify the complexity.

This is a central design decision and should be explored early.

### 5. Localization Source-of-Truth Tension

Typed localization is easiest when definitions are static and code-visible.

If the project expects compatibility with highly dynamic or externally managed localization assets, the architecture may need softer guarantees and more adapter logic than currently implied.

### 6. Cryptographic Scope Ambiguity

Strongly typed cryptographic representations are useful, but the boundaries need clarity.

Questions include:

- Is the goal semantic wrappers for already-validated crypto data?
- Is the goal generated parsing and formatting?
- Is the goal integrating with actual cryptographic operations?

Those are different scopes with different risk profiles.

## Key Unresolved Decisions

The following design questions should be answered before the architecture hardens:

1. What is the primary authoring model?
   The platform needs a clear declaration style for semantic intent.

2. What guarantees are expected to be compile-time, analyzer-time, and runtime?
   This needs explicit policy, not just aspiration.

3. Is lifecycle modelling a first-class pillar or a later specialized extension?
   It is valuable, but it is substantially more complex than the other tracks.

4. Is localization part of the same platform or a sibling subsystem sharing the same tooling stack?
   Both are reasonable, but the design should pick one.

5. What kinds of integrations matter early?
   Serialization, ASP.NET binding, persistence, and localization resource loading can influence the abstractions.

6. What is the intended audience for the system?
   An internal experimentation platform, a reusable library, and a broader ecosystem tool each justify different tradeoffs.

## Conceptual Viability Verdict

Yes, the system is fundamentally implementable.

The most viable interpretation is:

- a shared semantic tooling platform
- multiple focused capability tracks
- source generators paired with analyzers
- runtime enforcement where data enters from outside the compiler

The least viable interpretation would be a promise that all semantic meaning, state safety, and localization correctness can be reduced to pure compile-time type guarantees. C# can support a lot here, but not that absolute version of the vision.

## Recommended Exploration Order

To validate the architecture pragmatically, the repository should explore the tracks in this order:

1. Composite symbolic values
   This has high feasibility, high clarity, and strong generator payoff.

2. Constraint-rich scalar or symbolic values
   This clarifies the guarantee model and authoring ergonomics.

3. Analyzer diagnostics
   This converts "nice generated types" into an actual semantic tooling platform.

4. Strongly typed localization
   This validates whether the generation approach generalizes well beyond value wrappers.

5. Lifecycle/stateful modelling
   This should be explored once the platform boundaries are understood.

This order reduces the chance of committing the whole design around the hardest area first.

## Clarifying Questions

These questions do not block the mock design, but they should guide the next iteration:

1. Should lifecycle/stateful modelling be considered a core first milestone, or should it remain an advanced extension after constrained/composite values are proven?
2. Do you want localization to live inside the same semantic platform, or as a separate but related generator package?
3. Is the expected authoring style more attribute-driven, interface-driven, code-first, or definition-file-driven?
4. Are cryptographic value objects meant to model opaque semantic data only, or also participate in operational cryptographic workflows?
5. Is the intended near-term goal an experimental lab proving concepts, or a foundation for a reusable production-oriented library?

## Recommended Next Step

The next concrete step should not be a full implementation. It should be a focused architecture spike that tests one narrow authoring model across:

- one constrained scalar value
- one composite symbolic value
- one typed localization example

If one authoring and generation approach feels coherent across those three, the broader platform direction is likely sound.
