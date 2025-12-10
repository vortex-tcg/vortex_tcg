FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /workspace

# Copie tout le code source
COPY . .

WORKDIR /workspace/apps/migrations

# Restore les dépendances
RUN dotnet restore

# Build le projet
RUN dotnet build -c Release

# Exécute le programme de migration
CMD ["dotnet", "run", "--no-build", "-c", "Release"]