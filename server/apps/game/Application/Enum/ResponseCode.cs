namespace game.Application.Enum;

public enum ResponseCode
{
    OK = 200,
    NotFound = 404,
    SUCCESS_POSE_CARTE = 100,  
    SUCCESS_PHASE_CHANGED = 101,
    CODE_TAKEN = 1001,
    ROOM_FULL = 1002,
    NOT_FOUND = 1003,
    MATCH_FOUND = 1000,

    NOT_IN_ROOM = 1101,
    NOT_YOUR_TURN = 1102,

    UNKNOWN_ERROR = 1500
}
