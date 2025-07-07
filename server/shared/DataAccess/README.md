## Base de données

Il faut installer .NET 9.0, .NET 8.0 via le site de .NET : https://aka.ms/dotnet/download

Il faut relancer l'éditeur de texte (VSCODE, DataGrip, ...)

Ensuite, il faut installer EF Core :
    Se placer dans server\apps
    Lancer : dotnet tool install --global dotnet-ef

Il faut relancer l'éditeur de texte (VSCODE, DataGrip, ...)

**ATTENTION**, il est important de vérifier que votre DB dans Docker a un port.

Ensuite il faut rentrer dans le docker :
    docker exec -it server-game-1 bash

Puis vérifier que vous êtes bien dans le fichier game.

Maintenant, il faut lancer cette commande à la suite :
    dotnet tool install --global dotnet-ef --version 8.*
    export PATH="$PATH:/root/.dotnet/tools"
    echo 'export PATH="$PATH:/root/.dotnet/tools"' >> ~/.bashrc
    source ~/.bashrc

Pour update la bdd enfin :
dotnet ef database update --project /workspace/shared/DataAccess/DataAccess.csproj --startup-project /workspace/apps/game