namespace game.Domaine.Interface;

public interface IEvent
{
    string Name { get; }

    T GetData<T>();
}
