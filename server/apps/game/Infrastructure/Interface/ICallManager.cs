using game.Application.Dto;

namespace game.Application.Enum
{
    public interface ICallManager
    {
        Task CallAsync<T>(responseDTO<T> response, CancellationToken ct = default);
    }
}
