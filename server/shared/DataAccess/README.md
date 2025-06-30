## Base de données

Il faut installer .NET 9.0, .NET 8.0 via le site de .NET : https://aka.ms/dotnet/download

Il faut relancer l'éditeur de texte (VSCODE, DataGrip, ...)

Ensuite, il faut installer EF Core :
    Se placer dans server\apps
    Lancer : dotnet tool install --global dotnet-ef

Il faut relancer l'éditeur de texte (VSCODE, DataGrip, ...)

Enfin, il faut lancer la commande suivante :
    dotnet ef database update --startup-project ../../apps/game

**ATTENTION**, il est important de vérifier que votre DB dans Docker a un port.