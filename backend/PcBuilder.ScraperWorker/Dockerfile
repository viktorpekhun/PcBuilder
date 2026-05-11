FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all csproj files and restore (include transitive dependencies)
COPY PcBuilder.ScraperWorker/PcBuilder.ScraperWorker.csproj PcBuilder.ScraperWorker/
COPY PcBuilder.SharedKernel/PcBuilder.SharedKernel.csproj PcBuilder.SharedKernel/
COPY PcBuilder.Persistence/PcBuilder.Persistence.csproj PcBuilder.Persistence/
COPY PcBuilder.Contracts/PcBuilder.Contracts.csproj PcBuilder.Contracts/
COPY Modules/Components/Components.Domain/Components.Domain.csproj Modules/Components/Components.Domain/
COPY Modules/Components/Components.Application/Components.Application.csproj Modules/Components/Components.Application/
COPY Modules/Components/Components.Infrastructure/Components.Infrastructure.csproj Modules/Components/Components.Infrastructure/
COPY Modules/Auth/Auth.Domain/Auth.Domain.csproj Modules/Auth/Auth.Domain/
COPY Modules/Auth/Auth.Application/Auth.Application.csproj Modules/Auth/Auth.Application/
COPY Modules/Auth/Auth.Infrastructure/Auth.Infrastructure.csproj Modules/Auth/Auth.Infrastructure/
COPY Modules/PcBuilds/PcBuilds.Domain/PcBuilds.Domain.csproj Modules/PcBuilds/PcBuilds.Domain/
COPY Modules/PcBuilds/PcBuilds.Application/PcBuilds.Application.csproj Modules/PcBuilds/PcBuilds.Application/
COPY Modules/PcBuilds/PcBuilds.Infrastructure/PcBuilds.Infrastructure.csproj Modules/PcBuilds/PcBuilds.Infrastructure/
COPY Modules/Scraping/Scraping.Application/Scraping.Application.csproj Modules/Scraping/Scraping.Application/
COPY Modules/Scraping/Scraping.Infrastructure/Scraping.Infrastructure.csproj Modules/Scraping/Scraping.Infrastructure/
COPY Modules/Notifications/Notifications.Domain/Notifications.Domain.csproj Modules/Notifications/Notifications.Domain/
COPY Modules/Notifications/Notifications.Application/Notifications.Application.csproj Modules/Notifications/Notifications.Application/
COPY Modules/Notifications/Notifications.Infrastructure/Notifications.Infrastructure.csproj Modules/Notifications/Notifications.Infrastructure/
COPY Modules/Moderation/Moderation.Domain/Moderation.Domain.csproj Modules/Moderation/Moderation.Domain/
COPY Modules/Moderation/Moderation.Application/Moderation.Application.csproj Modules/Moderation/Moderation.Application/
COPY Modules/Moderation/Moderation.Infrastructure/Moderation.Infrastructure.csproj Modules/Moderation/Moderation.Infrastructure/
RUN dotnet restore PcBuilder.ScraperWorker/PcBuilder.ScraperWorker.csproj

# Copy everything and build
COPY . .
RUN dotnet publish PcBuilder.ScraperWorker/PcBuilder.ScraperWorker.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PcBuilder.ScraperWorker.dll"]
