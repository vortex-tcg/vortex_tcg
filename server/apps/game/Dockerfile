#-----
# BUILD
#-----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . ./

RUN dotnet restore

RUN dotnet publish -c Release -o out

#-----
# RUN
#-----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/out .

EXPOSE 8000

ENTRYPOINT ["dotnet", "game.dll"]
