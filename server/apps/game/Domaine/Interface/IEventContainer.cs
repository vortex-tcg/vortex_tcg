namespace game.Domaine.Interface;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


public interface IEventContainer
{
    Task<IReadOnlyList<IEvent>> PullEventsAsync(CancellationToken ct = default);
}
