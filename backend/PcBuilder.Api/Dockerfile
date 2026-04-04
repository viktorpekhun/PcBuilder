FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all csproj files and restore
COPY PcBuilder.Api/PcBuilder.Api.csproj PcBuilder.Api/
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
RUN dotnet restore PcBuilder.Api/PcBuilder.Api.csproj

# Copy everything and build
COPY . .
RUN dotnet publish PcBuilder.Api/PcBuilder.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PcBuilder.Api.dll"]
