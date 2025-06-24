FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./

EXPOSE 5238

ENV ASPNETCORE_ENVIRONMENT=Development

RUN apt-get update && apt-get install -y inotify-tools

CMD ["sh", "-c", "while true; do dotnet run --urls http://0.0.0.0:5238 & PID=$!; inotifywait -r -e modify /app --exclude '(bin|obj)'; kill $PID; wait $PID; echo 'Restarting...'; done"]