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
    private readonly int RoomCapacity = 4;

    /// <summary>
    /// 최초 생성시에 Host PC와 Tablet, 그리고 방의 이름을 입력받는다.
    /// 생성됨과 동시에 두 host에게 접속 성공을 통지한다.
    /// </summary>
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
    /// <summary>
    /// Thraed로 실행되는 부분. Tablet의 데이터를 받아서 host와 각 guest에게 뿌린다.
    /// 두 host 중 하나의 접속이 종료되면 루프를 탈출하며 잔류한 모든 인원에게 접속 종료를 통지한다.
    /// </summary>
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
    }

    /// <summary>
    /// Guest 추가시 접속 성공을 통지하고 List에 추가한다. 
    /// </summary>
    public void GuestEnter(Socket guest)
    {
        var sock = new SocketManager();
        sock.StartSocket(guest);
        var temp = Encoding.UTF8.GetBytes(CommendBook.Connection);
        sock.Send(temp, temp.Length);
        guests.Add(sock);
    }
    /// <summary>
    /// 현재 잔류하는 Guest의 수를 채크하여 수용 가능량을 초과하면 받지 않는다.
    /// </summary>
    public bool GuestEnterRequest()
    {
        return (guests.Count < RoomCapacity);
    }

    private void clearBuffer(byte[] buffer, int size)
    {
        for (int i = 0; i < size; i++)
            buffer[i] = 0;
    }
}
