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

    private Thread workerThread = null;

    private bool isConnected = false;

    private bool isTablet = false;

    private static int BUFFERSIZE = 1024;

    // 델리게이트 쓰는법을 알아보자
    public delegate void EventHandler(NetEventState state);

    private EventHandler handler;


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

        return LaunchdispatchThread();
    }

    // 송신처리.
    public int Send(byte[] data, int size)
    {
        if (sendQueue == null)
        {
            return 0;
        }
        return sendQueue.Enqueue(data, size);
    }

    // 수신처리.
    public int Receive(ref byte[] buffer, int size)
    {
        if (receiveQueue == null)
        {
            return 0;
        }

        return receiveQueue.Dequeue(ref buffer, size);
    }


    public void Disconnect()
    {
        isConnected = false;

        if (socket != null)
        {
            // 소켓 클로즈.
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }

        // 끊김을 통지합니다.
        if (handler != null)
        {
            NetEventState state = new NetEventState();
            state.type = NetEventType.Disconnect;
            state.result = NetEventResult.Success;
            handler(state);
        }
    }


    private bool LaunchdispatchThread()
    {
        try
        {
            // Dispatch용 스레드 시작.
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


    // 스레드 측 송수신 처리.
    public void Dispatch()
    {
        Console.WriteLine("Dispatch dispatchThread started.");

        while (dispatchThreadLoop)
        {
            // 클라이언트와의 송수신 처리를 합니다.
            if (socket != null && isConnected == true)
            {
                DispatchReceive();
                DispatchSend();
            }
            Thread.Sleep(50);
        }
        Console.WriteLine("Dispatch dispatchThread ended.");
    }


    // 스레트 측 송신처리.
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

    // 스레드 측 수신처리.
    void DispatchReceive()
    {
        // 수신처리.
        try
        {
            while (socket.Poll(0, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[BUFFERSIZE];

                int recvSize = socket.Receive(buffer, buffer.Length, SocketFlags.None);
                if (recvSize == 0)
                {
                    // 끊기.
                    Console.WriteLine("Disconnect recv from client.");
                    Disconnect();
                }
                else if (recvSize > 0)
                {
                    receiveQueue.Enqueue(buffer, recvSize);
                }
            }
        }
        catch
        {
            return;
        }
    }
    // 이벤트 통지 함수 등록.
    public void RegisterEventHandler(EventHandler handler)
    {
        this.handler += handler;
    }

    // 이벤트 통지 함수 삭제.
    public void UnregisterEventHandler(EventHandler handler)
    {
        this.handler -= handler;
    }

    // 접속 확인.
    public bool IsConnected()
    {
        return isConnected;
    }


    public Thread GetThread()
    {
        return workerThread;
    }

    public void SetThread(Thread thread)
    {
        workerThread = thread;
    }


}
