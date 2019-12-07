using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/// <summary>
/// Drawing Thread의 종료를 대기하여 실제로 종료시키고 사용된 스레드를 정리하는 역할을 함.
/// Drawing이 종료된다는 것은 방이 닫힌다는 의미이므로 리스너 역시 종료시킨다.
/// </summary>
class ThreadObserver
{
    private Thread thread;
    private TcpListenerManager listener;

    public Thread Thread { get => thread; set => thread = value; }
    public TcpListenerManager Listener { get => listener; set => listener = value; }

    public ThreadObserver(Thread thread, TcpListenerManager listener)
    {
        Thread = thread;
        Listener = listener;
    }
    public void runObserving()
    {
        Thread.Join();
        listener.StopListening();
    }
}

/// <summary>
/// TCP Listener 부분을 따로 관리하는 Class
/// </summary>

public class TcpListenerManager
{
    private bool isListening;
    private TcpListener tcpListener;
    public bool IsListening { get => isListening; set => isListening = value; }
    public TcpListener TcpListener { get => tcpListener; set => tcpListener = value; }
    public TcpListenerManager(string port)
    {
        TcpListener = new TcpListener(IPAddress.Any, Int32.Parse(port));

    }
    public void StartListening()
    {
        TcpListener.Start();
        IsListening = true;
    }
    public void StopListening()
    {
        TcpListener.Stop();
        IsListening = false;
    }



}