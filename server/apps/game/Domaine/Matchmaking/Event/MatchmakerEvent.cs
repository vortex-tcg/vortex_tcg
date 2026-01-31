using game.Domaine.Interface;

namespace game.Domaine.Matchmaking;

public sealed class MatchmakerEvent : IEvent
{
    public const string FOUND = "FOUND";

    public string Name { get; }

    private readonly object _data;

    public MatchmakerEvent(string name, object data)
    {
        Name = name;
        _data = data;
    }

    public T GetData<T>() => (T)_data;
}
