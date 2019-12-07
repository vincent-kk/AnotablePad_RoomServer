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

            //실행 매개변수는 pipe name과 방의 이름이다.
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

            pipe.Connect();
            var ss = new StreamString(pipe);
            if (ss.ReadString() == "NameServer::StartRoom") //>>1
            {
                port = ss.ReadString(); //>>4
                try
                {
                    listenerManager = new TcpListenerManager(port);
                    listenerManager.StartListening();
                    while (true)
                    {
                        //Drawing Server에 연결되면 Client는 최초로 자신에 대해 전송한다.
                        Socket temp = listenerManager.TcpListener.AcceptSocket();
                        int recvSize = temp.Receive(buffer, buffer.Length, SocketFlags.None);
                        if (recvSize > 0)
                        {
                            string message = Encoding.UTF8.GetString(buffer, 0, recvSize);
                            string[] toks = message.Split(AppData.Delimiter);
                            foreach (var tok in toks)
                            {
                                if (tok == "") continue;
                                if (tok.Contains(AppData.ServerCommand))
                                {
                                    if (tok == AppData.ServerCommand+"Host")
                                    {
                                        host = temp;
                                    }
                                    else if (tok == AppData.ServerCommand + "Tablet")
                                    {
                                        tablet = temp;
                                    }
                                    else if (tok == AppData.ServerCommand + "Guest") { }
                                }
                            }
                        }
                        else
                        {
                            var message = Encoding.UTF8.GetBytes("@FAIL");
                            temp.Send(buffer, buffer.Length, SocketFlags.None);
                            continue;
                        }
                        // host와 host tablet이 접속해야 실제 Drawing을 시작한다.
                        if (host == null || tablet == null)
                            continue;
                        else
                        {
                            if (clientThread == null)
                            {
                                cHandler = new ClientHandler(host, tablet, RoomName);
                                clientThread = new Thread(new ThreadStart(cHandler.RunDrawing));
                                clientThread.Start();

                                //Drawing Thread를 Join하고 Listen loop를 끝내기 위한 Observer Thread
                                var observer = new ThreadObserver(clientThread, listenerManager);
                                observerThread = new Thread(new ThreadStart(observer.runObserving));
                                observerThread.Start();
                            }
                            else
                            {
                                //현재 포함된 Guest의 수를 확인하고 지정된 수보다 적으면 접속, 아니면 실패를 통지
                                if (cHandler.GuestEnterRequest())
                                {
                                    cHandler.GuestEnter(temp);
                                }
                                else
                                {
                                    var message = Encoding.UTF8.GetBytes(CommendBook.DRAWING_ROOM_FULL);
                                    temp.Send(message, message.Length, SocketFlags.None);
                                }
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
                        listenerManager.StopListening();
                    observerThread.Join();
                }
                Console.WriteLine("$RoomServer::" + RoomName + " => Closed");
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


