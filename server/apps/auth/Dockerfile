FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /workspace

COPY . .

WORKDIR /workspace/apps/auth

RUN dotnet restore
# Install the deps

EXPOSE 5238

RUN apt-get update && apt-get install -y inotify-tools
# Install deps to watch files changing

CMD ["sh", "-c", "while true; do dotnet run --urls http://0.0.0.0:5238 & PID=$!; inotifywait -r -e modify /app --exclude '(bin|obj)'; kill $PID; wait $PID; echo 'Resta'Restarting...'; done"]
# Check the changes and rerun the container to apply it