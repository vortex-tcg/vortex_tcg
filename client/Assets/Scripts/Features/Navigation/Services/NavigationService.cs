using UnityEngine;

public class NavigationService
{
    private NavigationState navigationState;
    private EventBus eventBus = EventBus.Instance;

    public NavigationService(NavigationState state)
    {
        navigationState = state;

        eventBus.Subscribe<NavigationRequestedEvent>(OnNavigationRequested);
    }

    ~NavigationService()
    {
        eventBus.Unsubscribe<NavigationRequestedEvent>(OnNavigationRequested);
    }

    private void OnNavigationRequested(NavigationRequestedEvent evt)
    {
        NavigateToScene(evt.ScenePath, evt.Status);
    }

    public void NavigateToScene(string scenePath, UpMenuStatus newStatus)
    {
        UpMenuStatus previousStatus = navigationState.GetStatus();
        navigationState.SetStatus(newStatus);

        eventBus.Publish(new NavigationStatusChangedEvent(previousStatus, newStatus));

        LoadingScreen.Load(scenePath, loadMenu: true, unloadMenu: false);

        eventBus.Publish(new NavigationCompletedEvent(scenePath, newStatus));
    }

    public UpMenuStatus GetCurrentStatus()
    {
        return navigationState.GetStatus();
    }
}
