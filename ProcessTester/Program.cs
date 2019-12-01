using System;
using System.IO;
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

            string port;

            byte[] buffer = new byte[128];

            if (args.Length == 1)
            {
                pipeName = args[0];
                Console.WriteLine("Pipe Name is {0}", pipeName);
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
            Console.WriteLine("Connected to Name Server.");

            var ss = new StreamString(pipe);
            // Validate the server's signature string.
            if (ss.ReadString() == "@NameServer:StartRoom") //>>1
            {
                // Print the file to the screen.

                port = ss.ReadString(); //>>4

                Console.WriteLine(port);
                try
                {
                    listenerManager = new TcpListenerManager(port);

                    listenerManager.TcpListener.Start();

                    Console.WriteLine("MuliThread Starting : Waiting for connections...");

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
                                    if (tok == "@Host-PC")
                                    {
                                        host = temp;
                                        Console.WriteLine("Host PC Connection");
                                    }
                                    else if (tok == "@Tablet")
                                    {
                                        tablet = temp;
                                        Console.WriteLine("Tablet Connection");
                                    }
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
                                cHandler = new ClientHandler(host, tablet);
                                clientThread = new Thread(new ThreadStart(cHandler.RunDrawing));
                                clientThread.Start();
                                ss.WriteString("$RoomServer:Started");

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
                catch (SocketException exp)
                {
                    Console.WriteLine("Drawing Thread is Join");
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
                Console.WriteLine("$RoomServer:Closed");
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

/*
public class PipeClient
{
    private static int numClients = 1;

    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            if (args[0] == "spawnclient")
            {
                var pipeClient =
                    new NamedPipeClientStream(".", "testpipe",
                        PipeDirection.InOut, PipeOptions.None,
                        TokenImpersonationLevel.Impersonation);

                Console.WriteLine("Connecting to server...\n");
                pipeClient.Connect();

                var ss = new StreamString(pipeClient);
                // Validate the server's signature string.
                if (ss.ReadString() == "I am the one true server!")
                {
                    // The client security token is sent with the first write.
                    // Send the name of the file whose contents are returned
                    // by the server.
                    ss.WriteString("d:\\textfile.txt");

                    // Print the file to the screen.
                    Console.Write(ss.ReadString());
                }
                else
                {
                    Console.WriteLine("Server could not be verified.");
                }
                pipeClient.Close();
                // Give the client process some time to display results before exiting.
                Thread.Sleep(4000);
            }
        }
        else
        {
            Console.WriteLine("\n*** Named pipe client stream with impersonation example ***\n");
            StartClients();
        }
    }

    // Helper function to create pipe client processes
    private static void StartClients()
    {
        string currentProcessName = Environment.CommandLine;
        currentProcessName = Path.ChangeExtension(currentProcessName, ".exe");
        Process[] plist = new Process[numClients];

        Console.WriteLine("Spawning client processes...\n");

        if (currentProcessName.Contains(Environment.CurrentDirectory))
        {
            currentProcessName = currentProcessName.Replace(Environment.CurrentDirectory, String.Empty);
        }

        // Remove extra characters when launched from Visual Studio
        currentProcessName = currentProcessName.Replace("\\", String.Empty);
        currentProcessName = currentProcessName.Replace("\"", String.Empty);

        int i;
        for (i = 0; i < numClients; i++)
        {
            // Start 'this' program but spawn a named pipe client.
            plist[i] = Process.Start(currentProcessName, "spawnclient");
        }
        while (i > 0)
        {
            for (int j = 0; j < numClients; j++)
            {
                if (plist[j] != null)
                {
                    if (plist[j].HasExited)
                    {
                        Console.WriteLine($"Client process[{plist[j].Id}] has exited.");
                        plist[j] = null;
                        i--;    // decrement the process watch count
                    }
                    else
                    {
                        Thread.Sleep(250);
                    }
                }
            }
        }
        Console.WriteLine("\nClient processes finished, exiting.");
    }
}
*/
// Defines the data protocol for reading and writing strings on our stream.
public class StreamString
{
    private Stream ioStream;
    private UTF8Encoding streamEncoding;

    public StreamString(Stream ioStream)
    {
        this.ioStream = ioStream;
        streamEncoding = new UTF8Encoding();
    }

    public string ReadString()
    {
        int len;
        len = ioStream.ReadByte() * 256;
        len += ioStream.ReadByte();
        var inBuffer = new byte[len];
        ioStream.Read(inBuffer, 0, len);

        return streamEncoding.GetString(inBuffer);
    }

    public int WriteString(string outString)
    {
        byte[] outBuffer = streamEncoding.GetBytes(outString);
        int len = outBuffer.Length;
        if (len > UInt16.MaxValue)
        {
            len = (int)UInt16.MaxValue;
        }
        ioStream.WriteByte((byte)(len / 256));
        ioStream.WriteByte((byte)(len & 255));
        ioStream.Write(outBuffer, 0, len);
        ioStream.Flush();

        return outBuffer.Length + 2;
    }

    public int WriteByte(byte[] outString)
    {
        byte[] outBuffer = outString;
        int len = outBuffer.Length;
        ioStream.Write(outBuffer, 0, len);
        ioStream.Flush();
        return len;
    }

    public byte[] ReadByte()
    {
        int len = 0;
        len = ioStream.ReadByte();
        byte[] inBuffer = new byte[len];
        ioStream.Read(inBuffer, 0, len);
        return inBuffer;
    }
}