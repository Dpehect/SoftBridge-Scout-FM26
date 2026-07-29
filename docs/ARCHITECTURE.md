# Architecture

The backend follows a pragmatic Clean Architecture split:
- Domain: entities and invariants
- Application: use cases, DTOs and abstractions
- Infrastructure: EF Core, PostgreSQL and repositories
- API: HTTP boundary, middleware and composition root

The web application uses Next.js App Router with Server Components by default. Data-heavy pages are server-rendered and revalidated to keep client JavaScript low.
