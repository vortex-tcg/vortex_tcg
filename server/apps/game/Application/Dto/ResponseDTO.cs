using System.Text.Json.Serialization;
using game.Application.Enum;

namespace game.Application.Dto;

public class responseDTO<T> {
    [JsonIgnore] public Guid userId { get; set; }
    [JsonIgnore]  public Guid opponentId { get; set; }    
    public bool success { get; set; } = true;
    public ResponseCode code { get; set; } = ResponseCode.OK;
    public T data { get; set; } = default(T);
    public T? opponentData { get; set; } = default(T);
}