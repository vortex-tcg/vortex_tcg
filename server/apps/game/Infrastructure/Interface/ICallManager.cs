using game.Application.Dto;

namespace game.Application.Enum
{
    public interface ICallManager
    {
        Task CallAsync<MatchFoundSelfDto, MatchFoundOpponentDto>(responseDTO<MatchFoundSelfDto, MatchFoundOpponentDto> response, CancellationToken ct = default);
    }
}
