public class NavigationState
{
    private UpMenuStatus currentStatus;
    private EventBus eventBus = EventBus.Instance;

    public NavigationState(UpMenuStatus initialStatus = UpMenuStatus.MAIN)
    {
        currentStatus = initialStatus;
    }

    public void SetStatus(UpMenuStatus status)
    {
        if (currentStatus != status)
        {
            UpMenuStatus previousStatus = currentStatus;
            currentStatus = status;

            eventBus.Publish(new NavigationStatusChangedEvent(previousStatus, status));
        }
    }

    public UpMenuStatus GetStatus()
    {
        return currentStatus;
    }
}
