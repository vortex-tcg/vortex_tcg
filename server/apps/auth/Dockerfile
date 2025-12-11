# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /workspace

COPY . .

WORKDIR /workspace/apps/auth

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5238

CMD ["dotnet", "auth.dll", "--urls", "http://0.0.0.0:5238"]