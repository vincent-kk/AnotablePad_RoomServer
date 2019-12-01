using System;
using System.Threading;

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
        listener.TcpListener.Stop();
    }
}

