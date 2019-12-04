using System;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RoomServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string pipeName;
            TcpListenerManager listenerManager = null;
            Socket host = null;
            Socket tablet = null;

            ClientHandler cHandler = null;
            Thread clientThread = null;
            Thread observerThread = null;
            string RoomName;
            string port;

            byte[] buffer = new byte[128];

            if (args.Length == 2)
            {
                RoomName = args[0];
                pipeName = args[1];
            }
            else
            {
                Console.WriteLine("Argument is invalied");
                foreach (var arg in args)
                    Console.WriteLine(arg);
                Environment.Exit(0);
                return;
            }

            NamedPipeClientStream pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            // Connect to the pipe or wait until the pipe is available.
            Console.WriteLine("Attempting to connect to Name Server...");
            pipe.Connect();
            Console.WriteLine("Connected to Name Server with pipe : " + pipeName);

            var ss = new StreamString(pipe);
            // Validate the server's signature string.
            if (ss.ReadString() == "@NameServer:StartRoom") //>>1
            {
                port = ss.ReadString(); //>>4
                try
                {
                    listenerManager = new TcpListenerManager(port);
                    listenerManager.TcpListener.Start();
                    while (true)
                    {
                        Socket temp = listenerManager.TcpListener.AcceptSocket();
                        int recvSize = temp.Receive(buffer, buffer.Length, SocketFlags.None);
                        if (recvSize > 0)
                        {
                            string message = Encoding.UTF8.GetString(buffer, 0, recvSize);
                            string[] toks = message.Split("|");
                            foreach (var tok in toks)
                            {
                                if (tok == "") continue;
                                if (tok.Contains("@"))
                                {
                                    if (tok == "@Host")
                                    {
                                        host = temp;
                                    }
                                    else if (tok == "@Tablet")
                                    {
                                        tablet = temp;
                                    }
                                    else if (tok == "@Guest") { }
                                }
                            }
                        }
                        else
                        {
                            buffer = Encoding.UTF8.GetBytes("@FAIL");
                            temp.Send(buffer, buffer.Length, SocketFlags.None);
                            continue;
                        }

                        if (host == null || tablet == null)
                            continue;
                        else
                        {
                            if (clientThread == null)
                            {
                                cHandler = new ClientHandler(host, tablet, RoomName);
                                clientThread = new Thread(new ThreadStart(cHandler.RunDrawing));
                                clientThread.Start();
                                ss.WriteString("$RoomServer-" + RoomName + ":Started");

                                var observer = new ThreadObserver(clientThread, listenerManager);
                                observerThread = new Thread(new ThreadStart(observer.runObserving));
                                observerThread.Start();
                            }
                            else
                            {
                                cHandler.GuestEnter(temp);
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                    Console.WriteLine(RoomName+" Drawing Thread Join");
                }
                catch (Exception exp)
                {
                    Console.WriteLine("Exception : " + exp.Message);
                }
                finally
                {
                    if (listenerManager.IsListening)
                        listenerManager.TcpListener.Stop();
                    observerThread.Join();
                }
                Console.WriteLine("$RoomServer-" + RoomName + ":Closed");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("Server could not be verified.");
            }
            pipe.Close();
            Environment.Exit(0);
        }
    }
}

public class TcpListenerManager
{
    private bool isListening;
    private TcpListener tcpListener;
    public bool IsListening { get => isListening; set => isListening = value; }
    public TcpListener TcpListener { get => tcpListener; set => tcpListener = value; }
    public TcpListenerManager(string port)
    {
        TcpListener = new TcpListener(IPAddress.Any, Int32.Parse(port));
        IsListening = true;
    }
}
