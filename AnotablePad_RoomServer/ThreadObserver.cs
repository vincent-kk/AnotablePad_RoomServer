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
