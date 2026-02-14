using System;

public class HomeState
{
    private bool isSearching;
    private string currentStatus;
    private string deckId;

    public event Action<bool> OnSearchingStateChanged;
    public event Action<string> OnStatusChanged;

    public bool IsSearching => isSearching;
    public string CurrentStatus => currentStatus;
    public string DeckId => deckId;

    public HomeState(string deckId)
    {
        this.deckId = deckId;
        this.isSearching = false;
        this.currentStatus = string.Empty;
    }

    public void SetSearching(bool searching)
    {
        if (isSearching != searching)
        {
            isSearching = searching;
            OnSearchingStateChanged?.Invoke(searching);
        }
    }

    public void SetStatus(string status)
    {
        if (currentStatus != status)
        {
            currentStatus = status;
            OnStatusChanged?.Invoke(status);
        }
    }

    public void SetDeckId(string deckId)
    {
        this.deckId = deckId;
    }

    public void Reset()
    {
        SetSearching(false);
        SetStatus(string.Empty);
    }
}
