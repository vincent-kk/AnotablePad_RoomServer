using System;
using System.Net.Sockets;
using System.Threading;

class SocketManager
{
    private Socket socket;

    private PacketQueue sendQueue;

    private PacketQueue receiveQueue;

    protected bool dispatchThreadLoop = false;

    protected Thread dispatchThread = null;

    private bool isConnected = false;

    private static int BUFFERSIZE = 1024;

    public delegate void EventHandler(NetEventState state);

    private EventHandler handler;
    public bool IsConnected { get => isConnected; }

    public SocketManager()
    {
        sendQueue = new PacketQueue();
        receiveQueue = new PacketQueue();
    }

    public bool StartSocket(Socket socket)
    {
        try
        {
            this.socket = socket;
            isConnected = true;
        }
        catch
        {
            Console.WriteLine("Socket Fail");
            return false;
        }

        return LaunchDispatchThread();
    }


    /*
     * summery : 송신 요청시 송신큐에 단순 추가. 이후 매 Dispatch 마다 실제 송신 
     */
    public int Send(byte[] data, int size)
    {
        if (sendQueue == null)
        {
            return 0;
        }
        return sendQueue.Enqueue(data, size);
    }

    /*
     * summery : 수신 요청시 수신 큐에서 바로 추출. 매 Dispatch 마다 실제 수신 후 큐에 저장 
     */
    public int Receive(ref byte[] buffer, int size)
    {
        if (receiveQueue == null)
        {
            return 0;
        }

        return receiveQueue.Dequeue(ref buffer, size);
    }


    /*
     * summery : 소켓 종료시, 실제로 소켓을 닫고 이벤트를 통지함.
     */
    public void Disconnect()
    {
        isConnected = false;

        if (socket != null)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }
        if (handler != null)
        {
            NetEventState state = new NetEventState();
            state.type = NetEventType.Disconnect;
            state.result = NetEventResult.Success;
            handler(state);
        }
    }


    private bool LaunchDispatchThread()
    {
        try
        {
            // Dispatch용 스레드 생성.
            dispatchThreadLoop = true;
            dispatchThread = new Thread(new ThreadStart(Dispatch));
            dispatchThread.Start();
        }
        catch
        {
            Console.WriteLine("Cannot launch dispatchThread.");
            return false;
        }
        return true;
    }

    /*
     * summery : Dispatch Thread 동작. 50ms마다 수신과 송신 작업 반복. 
     */
    public void Dispatch()
    {
        while (dispatchThreadLoop)
        {
            // 클라이언트와의 송수신 처리를 합니다.
            if (socket != null && IsConnected == true)
            {
                DispatchReceive();
                DispatchSend();
            }
            Thread.Sleep(50);
        }
    }

    /*
     * summery : 실제 송신 동작. 큐에서 메시지를 꺼내서 송신 
     */
    void DispatchSend()
    {
        try
        {
            // 송신처리.
            if (socket.Poll(0, SelectMode.SelectWrite))
            {
                byte[] buffer = new byte[BUFFERSIZE];

                int sendSize = sendQueue.Dequeue(ref buffer, buffer.Length);
                while (sendSize > 0)
                {
                    socket.Send(buffer, sendSize, SocketFlags.None);
                    sendSize = sendQueue.Dequeue(ref buffer, buffer.Length);
                }
            }
        }
        catch
        {
            return;
        }
    }

    /*
     * summery : 실제 수신 절차. 메시지를 수신하여 큐에 저장.
     */
    void DispatchReceive()
    {
        // 수신처리.
        try
        {
            while (socket.Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[BUFFERSIZE];
                int recvSize = socket.Receive(buffer, buffer.Length, SocketFlags.None);
                if (recvSize == 0) Disconnect();
                else if (recvSize > 0) receiveQueue.Enqueue(buffer, recvSize);
            }
        }
        catch
        {
            return;
        }
    }

    /*
     * summery : 이벤트 헨들러 델리게이트 추가.
     */
    public void RegisterEventHandler(EventHandler handler)
    {
        this.handler += handler;
    }


    /*
     * summery : 이벤트 헨들러 델리게이트 제거
     */
    public void UnregisterEventHandler(EventHandler handler)
    {
        this.handler -= handler;
    }
}
