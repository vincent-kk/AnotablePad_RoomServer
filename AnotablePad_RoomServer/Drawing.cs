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
    private string name;
    private bool connection;
    public ClientHandler(Socket _host, Socket _tablet, string _name)
    {
        host = _host;
        tablet = _tablet;
        name = _name;
        buffer = new byte[1024];
        var temp = Encoding.UTF8.GetBytes(CommendBook.Connection);
        host.Send(temp, temp.Length, SocketFlags.None);
        tablet.Send(temp, temp.Length, SocketFlags.None);

        guests = new List<SocketManager>();
    }
    public void RunDrawing()
    {
        SocketManager hostSocket = new SocketManager();
        SocketManager tabletSocket = new SocketManager();

        int recvSize;
        hostSocket.StartSocket(host);
        tabletSocket.StartSocket(tablet);
        connection = true;

        Console.WriteLine(name + " Room's Drawing Thread Start...");
        while (connection)
        {
            recvSize = tabletSocket.Receive(ref buffer, buffer.Length);
            if (recvSize > 0)
            {
                hostSocket.Send(buffer, recvSize);

                for (int i = 0; i < guests.Count; i++)
                {
                    if (guests[i].IsConnected) guests[i].Send(buffer, recvSize);
                    else guests.RemoveAt(i);
                }

                clearBuffer(buffer, recvSize);
            }
            Thread.Sleep(5);

            connection = hostSocket.IsConnected && tabletSocket.IsConnected;
        }

        buffer = Encoding.UTF8.GetBytes(CommendBook.ROOM_CLOSED);
        if (tabletSocket.IsConnected) tabletSocket.Send(buffer, buffer.Length);
        else if (hostSocket.IsConnected) hostSocket.Send(buffer, buffer.Length);
        foreach (var guest in guests)
            if (guest.IsConnected) guest.Send(buffer, buffer.Length);

        Thread.Sleep(100);

        if (tabletSocket.IsConnected) tabletSocket.Disconnect();
        else if (hostSocket.IsConnected) hostSocket.Disconnect();
        foreach (var guest in guests)
            if (guest.IsConnected) guest.Disconnect();

        Console.WriteLine("Close " + name + " Server...");
    }

    public void GuestEnter(Socket guest)
    {
        var sock = new SocketManager();
        sock.StartSocket(guest);
        var temp = Encoding.UTF8.GetBytes(CommendBook.Connection);
        sock.Send(temp, temp.Length);
        guests.Add(sock);
    }

    private void clearBuffer(byte[] buffer, int size)
    {
        for (int i = 0; i < size; i++)
            buffer[i] = 0;
    }
}
