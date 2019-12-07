
/// <summary>
/// 프로그램에서 사용하는 모든 특수문자와 명령어를 기록
/// Client와 동일한 값을 사용해야 정상 동작이 보장됨.
/// </summary>
public class AppData
{
    private static readonly char delimiter = '|';
    private static readonly char delimiterUI = '%';
    private static readonly char clientCommand = '#';
    private static readonly char serverCommand = '@';
    private static readonly string color = "CC->";
    private static readonly string backgroundClear = "BG->CLEAR";
    private static readonly string endOfLine = "EOL";
    private static readonly int bufferSize = 1024;

    public static char Delimiter => delimiter;
    public static char DelimiterUI => delimiterUI;
    public static char ClientCommand => clientCommand;
    public static char ServerCommand => serverCommand;
    public static string ColorCommand => color;
    public static string BackgroundClearCommand => backgroundClear;
    public static string EndOfLine => endOfLine;
    public static int BufferSize => bufferSize;
}

public static class CommendBook
{
    private static readonly string findRoom = AppData.ServerCommand + "FIND-ROOM";
    private static readonly string createRoom = AppData.ServerCommand + "CREATE-ROOM";
    private static readonly string enterRoom = AppData.ServerCommand + "ENTER-ROOM";
    private static readonly string startDrawing = AppData.ServerCommand + "START-DRAWING";
    private static readonly string guestDrawing = AppData.ServerCommand + "GUEST-DRAWING";
    private static readonly string errorMessage = AppData.ServerCommand + "ERROR";
    private static readonly string roomListHeader = AppData.ServerCommand + "ROOM-LIST";
    private static readonly string roomClosed = AppData.ServerCommand + "ROOMCLOSED";
    private static readonly string drawingRoomIsFull = AppData.ServerCommand + "ROOM-IS-FULL";
    private static readonly string colorCommend = AppData.ClientCommand + AppData.ColorCommand;
    private static readonly string clearBackgroundCommend = AppData.ClientCommand + AppData.BackgroundClearCommand;
    private static readonly string endOnLine = AppData.ClientCommand + AppData.EndOfLine;
    private static readonly string connection = AppData.ServerCommand + "CONNECTION";

    public static string FIND_ROOM => findRoom;
    public static string CREATE_ROOM => createRoom;
    public static string ENTER_ROOM => enterRoom;
    public static string HEADER_ROOMLIST => roomListHeader;
    public static string ROOM_CLOSED => roomClosed;
    public static string ERROR_MESSAGE => errorMessage;
    public static string START_DRAWING => startDrawing;
    public static string GUEST_DRAWING => guestDrawing;
    public static string ColorCommend => colorCommend;
    public static string ClearBackgroundCommend => clearBackgroundCommend;
    public static string EndOnLineCommend => endOnLine;
    public static string Connection => connection;
    public static string DRAWING_ROOM_FULL => drawingRoomIsFull;
}