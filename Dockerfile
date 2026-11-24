# syntax=docker/dockerfile:1
# Tax Buddy web app (ADR 012: a Docker container on an OCI Compute VM, behind Caddy; RELEASE).
# Build from the repository root, usually through deploy/oci/compose.yaml:
#   docker build --build-arg VERSION=$(git rev-parse --short HEAD) -t taxbuddy-web .
# Stages: the Tailwind styles (Node), the published app and its EF migration bundle (the SDK that global.json pins),
# a migrations image, and the runtime image (the default target). Images build for the machine they're built on, so
# building on the Arm (Ampere) VM gives an arm64 image.

FROM node:22-bookworm-slim AS styles
WORKDIR /src/src/TaxBuddy.Web
COPY src/TaxBuddy.Web/package.json src/TaxBuddy.Web/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY src/TaxBuddy.Web/Styles ./Styles
COPY src/TaxBuddy.Web/Components ./Components
RUN node_modules/.bin/tailwindcss -i Styles/app.css -o wwwroot/app.css --minify

FROM mcr.microsoft.com/dotnet/sdk:11.0.100 AS build
# MSBuild reads environment variables as properties: the styles come from the stage above (TaxBuddy.Web.csproj).
ENV SkipTailwind=true
WORKDIR /src
COPY global.json Directory.Build.props dotnet-tools.json ./
COPY src/TaxBuddy.Web/TaxBuddy.Web.csproj src/TaxBuddy.Web/
RUN dotnet restore src/TaxBuddy.Web/TaxBuddy.Web.csproj && dotnet tool restore
COPY src/TaxBuddy.Web/ src/TaxBuddy.Web/
COPY --from=styles /src/src/TaxBuddy.Web/wwwroot/app.css src/TaxBuddy.Web/wwwroot/app.css
RUN dotnet publish src/TaxBuddy.Web/TaxBuddy.Web.csproj -c Release --no-restore -o /app
# RELEASE step 3: database changes are applied by this bundle before the new build runs, never at app start.
RUN dotnet ef migrations bundle --project src/TaxBuddy.Web --configuration Release --output /bundle/efbundle --force

FROM mcr.microsoft.com/dotnet/aspnet:11.0.0 AS migrations
WORKDIR /bundle
COPY --from=build /bundle/efbundle ./
USER $APP_UID
# The connection string comes from the environment (deploy/oci/.env.web), never from the image.
ENTRYPOINT ["sh", "-c", "exec ./efbundle --connection \"$ConnectionStrings__TaxBuddy\""]

FROM mcr.microsoft.com/dotnet/aspnet:11.0.0 AS runtime
ARG VERSION=unknown
# HTTP inside the container; Caddy terminates HTTPS and forwards the scheme, which ASP.NET Core then trusts.
# Data-protection keys go to /keys, a volume that outlives the container (DeploymentSetup).
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    DataProtection__KeysPath=/keys \
    Site__Version=$VERSION
WORKDIR /app
COPY --from=build /app ./
RUN mkdir -p /keys && chown "$APP_UID" /keys
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "TaxBuddy.Web.dll"]
