# CC.Referrals.Feature

## Project Overview
- **Purpose**: manage customer-referral invitations where REST endpoints handle mutations (create, validate, accept) and GraphQL exposes read-optimized queries.
- **Tech stack**: .NET 9, ASP.NET Core, HotChocolate GraphQL, EF Core InMemory provider, xUnit for tests.
- **Data flow**: Application layer coordinates fraud checks and link/token generation, Infrastructure wires EF Core and seeds a demo user (`Sam`).

## Architecture At A Glance
- `Domain`: entity and DTO definitions (`Referral`, `UserApp`, request objects, enums).
- `Application`: business services (`ReferralsServices`, `LinkServices`, `UserServices`) plus abstractions (`IFraudService`, `IReferralService`, etc.). Includes a mock fraud service for local use.
- `Infrastructure`: EF Core `AppDbContext`, seeding via `ApplicationDbContextInitialiser`, service registrations.
- `api`: ASP.NET Core host exposing REST controllers and HotChocolate GraphQL schema. Swagger and CORS enabled by default.
- `Application.Tests`: xUnit suite targeting the referral workflow.

## Running The App Locally
- Prerequisite: .NET 9 SDK installed.
- Restore dependencies: `dotnet restore CC.Referrals.Feature.sln`
- Launch the API (HTTPS by default): `dotnet run --project api/api.csproj`
- Swagger UI: `https://localhost:5001/swagger`
- GraphQL IDE (Banana Cake Pop/Altair friendly): `https://localhost:5001/graphql`
- Stop with `Ctrl+C`. The InMemory database resets on each run and reseeds the default user.

## Seed Data
- A single `UserApp` record is created at startup with display name `Sam` and referral code `SAM-3F4X9K`. All REST mutations resolve the active user through `UserServices`, so keep this record if you customize the seed.

## REST Endpoints
- `POST /referrals`  
  Creates a referral for the active user. Example:
  ```bash
  curl -X POST https://localhost:5001/referrals \
       -H "Content-Type: application/json" \
       -d '{"campaign":"spring-cashback"}' --insecure
  ```
  Response includes link, token, slug, expiry, and persisted metadata.
- `POST /referrals/ReferralValidation`  
  Validates a referral slug/token pairing. Example:
  ```bash
  curl -X POST https://localhost:5001/referrals/ReferralValidation \
       -H "Content-Type: application/json" \
       -d '{"referralCode":"SAM-3F4X9K","slug":"<slug-from-create>"}' --insecure
  ```
- `POST /referrals/{tokenID}/accept`  
  Consumes a referral token and marks it as accepted.

## GraphQL Queries
- List referrals with paging/filtering/sorting (HotChocolate middleware enabled):
  ```graphql
  query ListReferrals {
    referral(first: 10, order: { status: ASC }) {
      nodes {
        id
        referralCode
        status
        slug
        expiresAt
      }
      pageInfo {
        hasNextPage
        endCursor
      }
      totalCount
    }
  }
  ```
- Fetch a referral by ID:
  ```graphql
  query ReferralById($id: UUID!) {
    geReferralById(id: $id) {
      id
      referralCode
      status
      tokenConsumedUtc
    }
  }
  ```
  Execute against `https://localhost:5001/graphql`. Apply filters like `where: { status: { eq: ACCEPTED } }` courtesy of `[UseFiltering]`.

## Application.Tests Overview
- **CreateAsync_PersistsReferralWithExpectedValues**  
  Validates that `ReferralsServices.CreateAsync` enforces fraud checks, persists the referral, generates link/token/slug values, and stamps the 30-day expiration.
- **ValidateSlug_ReturnsExpectedResult_WhenSignatureValidAndReferralExists**  
  Confirms slug validation succeeds only when the anti-fraud signature passes and an unexpired pending referral exists, returning the stored token for downstream use.
- **ValidateSlug_ReturnsFalse_WhenSignatureRejected**  
  Ensures slug validation fails fast when the upstream fraud service rejects the vendor signature, preventing referral lookup.

Run the suite with:
```bash
dotnet test Application.Tests/Application.Tests.csproj
```
