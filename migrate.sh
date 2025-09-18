#!/bin/sh
set -e

# This file will check if their is any changes in the models
# then automaticly create a new migration or not
DATAACCESS=/workspace/server/shared/DataAccess/DataAccess.csproj
GAME=/workspace/server/apps/game/game.csproj

cd /workspace

echo "==> Applying existing migrations..."
dotnet ef database update --project "$DATAACCESS" --startup-project "$GAME"

echo "==> Checking for model changes..."

if dotnet ef migrations has-pending-model-changes --project "$DATAACCESS" --startup-project "$GAME" >/dev/null 2>&1; then
    MIGRATION_NAME="AutoMigration_$(date +%s)"
    echo "==> Model changes detected. Creating migration: $MIGRATION_NAME"
    
    dotnet ef migrations add "$MIGRATION_NAME" --project "$DATAACCESS" --startup-project "$GAME"
    
    echo "==> Applying new migration..."
    dotnet ef database update --project "$DATAACCESS" --startup-project "$GAME"
else
    echo "==> No model changes detected."
fi

echo "==> Migration check completed."

if [ $# -gt 0 ]; then
    CORRECTED_ARGS=""
    for arg in "$@"; do
        case "$arg" in
            "apps/game/game.csproj")
                CORRECTED_ARGS="$CORRECTED_ARGS /workspace/server/apps/game/game.csproj"
                ;;
            *)
                CORRECTED_ARGS="$CORRECTED_ARGS $arg"
                ;;
        esac
    done
    
    exec $CORRECTED_ARGS
fi