# Architecture des Events

## Vue d'ensemble

Chaque feature possède un dossier `Events/` qui contient :
- Les définitions d'événements spécifiques à la feature
- Un EventBus partagé pour la communication découplée

## Architecture

```
Features/
├── Authentication/
│   ├── Events/
│   │   ├── AuthenticationEvents.cs
│   │   └── EventBus.cs
│   ├── Services/
│   ├── States/
│   └── UI/
├── Navigation/
│   ├── Events/
│   │   └── NavigationEvents.cs
│   ├── Services/
│   ├── States/
│   └── UI/
```

## Principes

### 1. Découplage
- L'UI publie des événements sans connaître les services
- Les services écoutent et publient des événements sans connaître l'UI
- Les composants communiquent via l'EventBus

### 2. Flux de données

**Exemple Authentication :**
```
UI → LoginRequestedEvent → Service → LoginSuccessEvent → UI
```

**Exemple Navigation :**
```
UI → NavigationRequestedEvent → Service → NavigationStatusChangedEvent → UI
```

## Utilisation

### S'abonner à un événement

```csharp
EventBus eventBus = EventBus.Instance;

// Dans OnEnable ou le constructeur
eventBus.Subscribe<LoginSuccessEvent>(OnLoginSuccess);

private void OnLoginSuccess(LoginSuccessEvent evt)
{
    Debug.Log($"Login réussi : {evt.Token}");
}
```

### Se désabonner

```csharp
// Dans OnDisable ou le destructeur
eventBus.Unsubscribe<LoginSuccessEvent>(OnLoginSuccess);
```

### Publier un événement

```csharp
EventBus eventBus = EventBus.Instance;
eventBus.Publish(new LoginRequestedEvent(email, password));
```

## Événements disponibles

### Authentication
- `LoginRequestedEvent` - Demande de connexion
- `LoginSuccessEvent` - Connexion réussie
- `LoginFailedEvent` - Échec de connexion
- `ValidateLoginInputEvent` - Validation des entrées

### Navigation
- `NavigationRequestedEvent` - Demande de navigation
- `NavigationStatusChangedEvent` - Changement de statut
- `NavigationCompletedEvent` - Navigation terminée

## Bonnes pratiques

1. **Toujours se désabonner** : Évite les fuites mémoire
2. **Événements immutables** : Les propriétés doivent être en lecture seule
3. **Nommer clairement** : Utiliser des suffixes comme Event, Requested, Completed
4. **Documentation** : Commenter la signification de chaque événement
5. **Timestamp** : Inclure un DateTime pour le debug

## Avantages

✅ **Découplage** : UI et Services indépendants  
✅ **Testabilité** : Facile de mocker les événements  
✅ **Flexibilité** : Ajout de nouveaux listeners sans modifier l'existant  
✅ **Maintenabilité** : Flux de données clair et traçable  
✅ **Debugging** : Tous les événements passent par l'EventBus
