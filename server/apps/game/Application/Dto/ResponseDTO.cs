using game.Application.Enum;

namespace game.Application.Dto;

public class responseDTO<T> {
    public Guid userId { get; set; }
    public Guid opponentId { get; set; }    
    public bool success { get; set; } = true;
    public ResponseCode code { get; set; } = ResponseCode.OK;
    public T data { get; set; } = default(T);
}