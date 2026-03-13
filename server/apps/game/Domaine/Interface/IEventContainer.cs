namespace game.Domaine.Interface;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


public interface IEventContainer
{
    IReadOnlyList<IEvent> PullEvents(CancellationToken ct = default);
}
