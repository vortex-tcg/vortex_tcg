FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /workspace

COPY migrate.sh /workspace/migrate.sh
RUN chmod +x /workspace/migrate.sh

COPY . .

WORKDIR /workspace/server/apps/game

RUN dotnet restore
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

EXPOSE 3306

RUN apt-get update && apt-get install -y inotify-tools

ENTRYPOINT ["sh","/workspace/migrate.sh"]
CMD ["dotnet", "run", "--project", "apps/game/game.csproj"]