using System;

public class NavigationRequestedEvent
{
    public string ScenePath { get; }
    public UpMenuStatus Status { get; }
    public DateTime Timestamp { get; }

    public NavigationRequestedEvent(string scenePath, UpMenuStatus status)
    {
        ScenePath = scenePath;
        Status = status;
        Timestamp = DateTime.UtcNow;
    }
}

public class NavigationStatusChangedEvent
{
    public UpMenuStatus PreviousStatus { get; }
    public UpMenuStatus NewStatus { get; }
    public DateTime Timestamp { get; }

    public NavigationStatusChangedEvent(UpMenuStatus previousStatus, UpMenuStatus newStatus)
    {
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Timestamp = DateTime.UtcNow;
    }
}

public class NavigationCompletedEvent
{
    public string ScenePath { get; }
    public UpMenuStatus Status { get; }
    public DateTime Timestamp { get; }

    public NavigationCompletedEvent(string scenePath, UpMenuStatus status)
    {
        ScenePath = scenePath;
        Status = status;
        Timestamp = DateTime.UtcNow;
    }
}
