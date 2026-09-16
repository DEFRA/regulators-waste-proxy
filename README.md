# Regulators waste proxy

A .NET 10 YARP reverse proxy prepared for the CDP build and deployment approach.

## Behaviour

- `GET /health` is handled by this service and returns `200 OK` with `{ "message": "success" }`.
- `GET /health/all` checks every configured downstream destination's `/health` endpoint. It returns `200 OK` only
  when they are all healthy, otherwise `503 Service Unavailable` with the per-destination result.
- The container listens on `PORT` (default `8085`) and includes `curl` for the CDP platform health check.

`/health/all` is protected by an exact, non-empty `X-Health-Check-Token` API-key header. The key defaults to empty
(which rejects every request), so deployments must configure it with the `Health__All__ApiKey` environment variable.
`Health__All__DownstreamTimeoutMilliseconds` optionally overrides the five-second downstream timeout.

## Routes

The proxy forwards requests to two downstream services:

| Route | Path | Downstream |
|---|---|---|
| `RegulatorsWasteDashboard` | `/regulator/{**catch-all}` | Regulators waste dashboard |
| `RegulatorsCertificatesOfCompliance` | `/certificates-of-compliance/{**catch-all}` | Certificates of compliance service |

The `/regulator` prefix is stripped before forwarding to the dashboard, and `X-Forwarded-Prefix: /regulator` is set
so the downstream app can construct its own URLs correctly. Similarly, `/certificates-of-compliance` is stripped
before forwarding to the certificates service with `X-Forwarded-Prefix: /certificates-of-compliance`.

Each destination defaults to `https://unconfigured.invalid/` and must be overridden before startup via environment
variable, for example:

```text
ReverseProxy__Clusters__RegulatorsWasteDashboard__Destinations__Primary__Address=https://dashboard.example/
ReverseProxy__Clusters__RegulatorsCertificatesOfCompliance__Destinations__Primary__Address=https://certificates.example/
```

Startup fails if any destination still has the unconfigured placeholder address.

## Shuttering a path

The proxy can temporarily replace a configured YARP route with a locally served DEFRA holding page. Every request
that matches a shuttered route, including pages and assets below its public path, returns `503 Service Unavailable`,
`text/html`, and `Cache-Control: no-store`; the request is not sent to YARP or its downstream service. There is no
redirect. `/health` and `/health/all` cannot be shuttered, preserving the CDP health-check contract.

Shuttering is configured on the YARP route itself, so its `Match:Path` remains the single source of truth for the
public path. Set the route's `Metadata:Shuttered` value to `true` or `false`:

```json
{
  "ReverseProxy": {
    "Routes": {
      "RegulatorsCertificatesOfCompliance": {
        "Metadata": {
          "Shuttered": true
        }
      }
    }
  }
}
```

Or via environment variable:

```text
ReverseProxy__Routes__RegulatorsCertificatesOfCompliance__Metadata__Shuttered=true
```

The holding-page body comes from an HTML fragment whose filename is derived from the route's `ClusterId`, converted
to kebab case. For example, `RegulatorsCertificatesOfCompliance` uses
[`regulators-certificates-of-compliance.html`](src/ReverseProxy/Shuttering/Pages/regulators-certificates-of-compliance.html),
which is inserted inside the shared DEFRA page shell. A holding-page fragment must exist for every configured route;
startup fails if one is missing.

### Shuttering observability

Every response served from a shuttered route emits the CloudWatch Embedded Metric Format counter
`ShutteredResponse`, tagged with its YARP route ID. Configure the deployment with the following setting:

```text
AWS_EMF_NAMESPACE=regulators-waste-proxy
```

## Run locally

```sh
dotnet restore regulators-waste-proxy.slnx
dotnet run --project src/ReverseProxy
```

The local development configuration in `appsettings.Development.json` points the clusters at:

- Dashboard: `https://localhost:7154/`
- Certificates of compliance: `http://localhost:3000/`

Then check the local endpoint:

```sh
curl http://localhost:8085/health
```

## Compose demonstration

```sh
docker compose up --build -d --wait
```

## Tests

Run the unit tests without Docker:

```sh
dotnet test tests/ReverseProxy.Tests/ReverseProxy.Tests.csproj --no-restore
```

Start the Compose environment before running the routing integration tests:

```sh
docker compose up --build -d --wait
dotnet test tests/ReverseProxy.IntegrationTests/ReverseProxy.IntegrationTests.csproj --no-restore
```

## Code quality

SonarCloud analysis runs after the validation, publish, and hot-fix jobs. It uses the repository `SONAR_TOKEN` secret
and reports coverage from both the unit and routing integration test projects to the
[Regulators Waste Proxy project](https://sonarcloud.io/project/overview?id=DEFRA_regulators-waste-proxy).
