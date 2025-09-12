FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /workspace

COPY . .

WORKDIR /workspace/apps/game

RUN dotnet restore
# Install the deps

EXPOSE 8000

RUN apt-get update && apt-get install -y inotify-tools
# Install deps to watch files changing

CMD ["sh", "-c", "while true; do dotnet run --urls http://0.0.0.0:8000 & PID=$!; inotifywait -r -e modify /workspace/apps/game --exclude '(bin|obj)'; kill $PID; wait $PID; echo 'Restarting...'; done"]
# Check the changes and rerun the container to apply it