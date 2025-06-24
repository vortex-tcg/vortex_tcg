FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./

EXPOSE 8000

RUN apt-get update && apt-get install -y inotify-tools

CMD ["sh", "-c", "while true; do dotnet run --urls http://0.0.0.0:8000 & PID=$!; inotifywait -r -e modify /app --exclude '(bin|obj)'; kill $PID; wait $PID; echo 'Restarting...'; done"]
