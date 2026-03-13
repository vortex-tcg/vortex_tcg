using System.Text.Json.Serialization;
using game.Application.Enum;

namespace game.Application.Dto;

public class responseDTO<TSelf, TOpponent>{
    [JsonIgnore] public Guid userId { get; set; }
    [JsonIgnore]  public Guid opponentId { get; set; }    
    public bool success { get; set; } = true;
    public ResponseCode code { get; set; } = ResponseCode.OK;
    public TSelf? data { get; set; } = default;
    public TOpponent? opponentData { get; set; } = default;
}