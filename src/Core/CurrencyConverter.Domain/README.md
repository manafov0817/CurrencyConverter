# Domain Layer

## Overview

The Domain Layer represents the core of the Currency Converter application following Domain-Driven Design (DDD) principles. This layer contains all the business entities, value objects, domain events, and business rules that form the heart of the application.

## Key Components

### Aggregate Roots

The aggregate roots in this project (such as `ExchangeRateRecord`) are currently not used for direct persistence operations. However, they have been designed with future database operations in mind. They:

- Encapsulate domain entities and value objects
- Maintain invariants and enforce business rules
- Include domain event infrastructure for potential event sourcing
- Contain an `Id` property to serve as unique identifiers in a persistence context

### Future Database Operations

While the current implementation primarily uses these domain models for in-memory operations, they are fully prepared for future database integration:

- The private constructors marked with `// For ORM` comments support entity frameworks and other ORMs
- The `Id` property in `AggregateRoot` is designed to be used as a primary key
- Domain events can be used to track changes and implement event sourcing if needed
- The encapsulation of properties with private setters maintains the integrity of the domain model while still allowing ORM mappings

### Domain Events

The `DomainEvent` pattern is implemented to support:
- Eventual consistency across aggregates
- Integration with external systems
- Audit trails of state changes
- Potential event sourcing architecture

## Project Structure

- **Common/**: Contains base classes like `AggregateRoot`, domain exceptions, and event definitions
- **ExchangeRates/**: Domain models related to currency exchange rates
- **Users/**: Domain models related to user management

## Design Principles

This Domain Layer adheres to the following principles:

1. **Encapsulation**: Internal state is protected with private fields and property setters
2. **Immutability**: Where appropriate, immutable objects are used to prevent unintended state changes
3. **Rich Domain Model**: Business logic is embedded directly in the domain entities and value objects
4. **Self-Validation**: Domain objects validate their own state to maintain invariants
5. **Separation of Concerns**: Domain logic is isolated from application and infrastructure concerns

## Usage Guidelines

When working with the Domain Layer:

- Do not expose setters that would violate business rules
- Add domain events for significant state changes
- Keep external dependencies out of this layer
- Consider extending the aggregate roots for database persistence in future versions
