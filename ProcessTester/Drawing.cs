using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ClientHandler
{
    private List<SocketManager> guests;
    private Socket host;
    private Socket tablet;
    private byte[] buffer;
    private readonly string delimiter = "|";
    private string name;
    private bool connection;
    public ClientHandler(Socket _host, Socket _tablet)
    {
        host = _host;
        tablet = _tablet;

        this.buffer = new byte[1024];
        byte[] temp = Encoding.UTF8.GetBytes("@CONNECTION");
        host.Send(temp, temp.Length, SocketFlags.None);
        tablet.Send(temp, temp.Length, SocketFlags.None);

        guests = new List<SocketManager>();
    }
    public void runDrawing()
    {
        SocketManager hostSocket = new SocketManager();
        SocketManager tabletSocket = new SocketManager();

        int recvSize;
        hostSocket.StartSocket(host);
        tabletSocket.StartSocket(tablet);
        connection = true;
        Console.WriteLine("Loop Start");

        while (connection)
        {
            recvSize = tabletSocket.Receive(ref buffer, buffer.Length);
            if (recvSize > 0)
            {
                //string message = Encoding.UTF8.GetString(buffer, 0, recvSize);
                //analizeMessage(message);
                hostSocket.Send(buffer, recvSize);
                if(guests.Count > 0)
                {
                    foreach (var guest in guests)
                    {
                        guest.Send(buffer, recvSize);
                    }
                }
                clearBuffer(buffer, recvSize);
            }     
            Thread.Sleep(5);
        }

        Console.WriteLine("Socket Disconnection");
        
    }

    public void GuestEnter(Socket guest)
    {
        var temp = new SocketManager();
        temp.StartSocket(guest);
        guests.Add(temp);
    }

    private void clearBuffer(byte[] buffer, int size)
    {
        for (int i = 0; i < size; i++)
            buffer[i] = 0;
    }

    private void analizeMessage(string msg)
    {
        string[] tokens = msg.Split(delimiter);
        foreach (var token in tokens)
        {
            if (token == "") continue;

            if (token.Contains("@"))
            {
                StringBuilder sb = new StringBuilder(token);
                sb.Remove(0, 1);
                this.name = sb.ToString();
                Console.WriteLine(sb.ToString());
            }
            else
            {
                Console.WriteLine("relay data : " + token);
            }
        }
    }
}



/*
 public class ClientHandler
 {

     private Socket guest;
     private Socket host;
     private Socket tablet;
     private byte[] buffer;
     private readonly string delimiter = "|";
     private string name;
     public ClientHandler(Socket host, Socket tablet)
     {
         this.host = host;
         this.tablet = tablet;

         this.buffer = new byte[1024];
         byte[] temp = Encoding.UTF8.GetBytes("@CONNECTION");
         host.Send(temp, temp.Length, SocketFlags.None);
         tablet.Send(temp, temp.Length, SocketFlags.None);
     }
     public void runClient()
     {
         SocketManager hostSocket = new SocketManager();
         SocketManager tabletSocket = new SocketManager();

         int recvSize;
         hostSocket.StartSocket(host);
         tabletSocket.StartSocket(tablet);

         while (true)
         {
             recvSize = tabletSocket.Receive(ref buffer, buffer.Length);
             if (recvSize > 0)
             {
                 string message = Encoding.UTF8.GetString(buffer, 0, recvSize);
               //  analizeMessage(message);
                 hostSocket.Send(buffer, recvSize);
                 clearBuffer(buffer, recvSize);
             }

             Thread.Sleep(5);
         }

     }

     private void clearBuffer(byte[] buffer, int size)
     {
         for (int i = 0; i < size; i++)
             buffer[i] = 0;
     }

     private void analizeMessage(string msg)
     {
         string[] tokens = msg.Split(delimiter);
         foreach(var token in tokens)
         {
             if (token == "") continue;

             if (token.Contains("@"))
             {
                 StringBuilder sb = new StringBuilder(token);
                 sb.Remove(0, 1);
                 this.name = sb.ToString();
                 Console.WriteLine(sb.ToString());
             }
             else
             {
                 Console.WriteLine(name + " data : " + token);
             }
         }
     }
     /*

     stream = new NetworkStream(clientSocket);

     Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

     reader = new StreamReader(stream, encode);

     while (true)
     {
         string str = reader.ReadLine();
         if (str.IndexOf("<EOF>") > -1)
         {
             Console.WriteLine("Bye Bye");
             break;
         }
         Console.WriteLine(str);

         str += "\r\n";

         byte[] dataWrite = Encoding.Default.GetBytes(str);

         stream.Write(dataWrite, 0, dataWrite.Length);
     }
 }
 catch (Exception e)
 {
     Console.WriteLine(e.ToString());
 }
 finally
 {
     clientSocket.Close();
 }

 }
 */


