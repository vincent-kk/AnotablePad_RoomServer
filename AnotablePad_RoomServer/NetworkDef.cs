
// 이벤트 종류.
public enum NetEventType
{
    Connect,
    Disconnect, 
    SendError,
    ReceiveError,
}

// 이벤트 결과.
public enum NetEventResult
{
    Failure = 1,
    Success = 0,
}

// 이벤트 상태 통지.
public class NetEventState
{
    public NetEventType type;
    public NetEventResult result;
    
}
