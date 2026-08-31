# Technical

## Language and Platform

The implementation should be built in C#.

The architecture should leverage:

- Source generators
- Modern C# type systems
- Strongly typed value objects
- Compile-time validation
- Generated supporting infrastructure

Some implementations may remain manually authored where generation is not appropriate.

## Constraint-Oriented Value Objects

The system should support richer semantic constraints than traditional primitive wrappers.

Examples include:

- Positive numbers
- Natural numbers
- Constrained symbolic identifiers
- URL-compatible strings
- Prefix-constrained values
- Restricted symbolic formats
- Cryptographic signature types

These constraints should ideally be enforced intrinsically by the type system and construction semantics.

## Cryptographic Value Types

The architecture should support strongly typed immutable cryptographic representations.

Examples:

- Cryptographic signatures
- Immutable signed byte arrays
- Structured cryptographic payloads

The value objects should preserve semantic meaning rather than exposing raw primitive representations directly.

## Stateful Type Modelling

The system should support lifecycle-oriented state machine modelling.

Example lifecycle:

- File metadata state
- Open state
- Streaming state
- Closed state
- Disposed state
- Error state

The architecture should ensure that only operations valid for a given state are accessible.

Potential implementation approaches include:

- Nested types
- Subclasses
- Discriminated unions
- Source-generated state wrappers
- State-specific interfaces

## Composite Structured Value Objects

The architecture should support composite symbolic values.

Example format:

`word::word::word`

The generated infrastructure should support:

- Delimiter handling
- Parsing
- Formatting
- Serialization
- Equality
- Deconstruction
- Component accessors
- Per-component validation

## Generated Composite Type Definitions

The system should support source-generator-driven declarations using metadata such as:

- Delimiter definitions
- Component counts
- Component names
- Validation constraints

The generators should synthesize:

- Parsing logic
- Formatting logic
- Equality
- Validation
- Deconstruction
- Serialization infrastructure
- Accessors
- Supporting helpers

## Enumeration and Symbolic Type Support

The architecture should improve support for:

- Enumerations
- Strongly typed strings
- Symbolic identifiers
- Constrained symbolic domains

The system should explore stronger semantic modelling than traditional enums or raw strings.

## Localization Source Generation

The architecture should support strongly typed localization generation.

Potential workflow:

- Define localization interfaces
- Generate localization bindings
- Generate parameterized templates
- Generate culture-aware formatting infrastructure

## Localization Features

The generated localization system should support:

- Localized templates
- Placeholder substitution
- Typed formatting parameters
- Culture-aware rendering
- Compile-time key validation
- Compile-time parameter validation
- Strongly typed localization bindings

The system should avoid stringly typed runtime localization APIs wherever practical.

## Compile-Time Validation

The architecture should maximize compile-time guarantees around:

- Localization keys
- Localization parameters
- Value constraints
- Structured symbolic values
- State transitions
- Composite formatting semantics

## Architectural Direction

The architecture should treat value objects as semantic stateful domain constructs rather than lightweight primitive wrappers.

The system should prioritize:

- Rich semantic constraints
- Stateful modelling
- Strong typing
- Compile-time validation
- Source-generated infrastructure
- Reusable generation patterns
- Localization safety
- Structured symbolic representations

Rather than minimal validation-only wrappers around primitive values.