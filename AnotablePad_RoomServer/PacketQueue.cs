using System;
using System.Collections.Generic;
using System.IO;

public class PacketQueue
{
    struct PacketInfo
    {
        public int offset;
        public int size;
    };

    private MemoryStream _streamBuffer;

    private List<PacketInfo> _offsetList;

    private int _offset = 0;


    // 
    public PacketQueue()
    {
        _streamBuffer = new MemoryStream();
        _offsetList = new List<PacketInfo>();
    }

    // 
    public int Enqueue(byte[] data, int size)
    {
        PacketInfo info = new PacketInfo();

        info.offset = _offset;
        info.size = size;

        _offsetList.Add(info);

        _streamBuffer.Position = _offset;
        _streamBuffer.Write(data, 0, size);
        _streamBuffer.Flush();
        _offset += size;

        return size;
    }

    public int Dequeue(ref byte[] buffer, int size)
    {

        if (_offsetList.Count <= 0)
        {
            return -1;
        }

        PacketInfo info = _offsetList[0];

        int dataSize = Math.Min(size, info.size);
        _streamBuffer.Position = info.offset;
        int recvSize = _streamBuffer.Read(buffer, 0, dataSize);

        if (recvSize > 0)
        {
            _offsetList.RemoveAt(0);
        }

        if (_offsetList.Count == 0)
        {
            Clear();
            _offset = 0;
        }

        return recvSize;
    }

    public void Clear()
    {
        byte[] buffer = _streamBuffer.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);

        _streamBuffer.Position = 0;
        _streamBuffer.SetLength(0);
    }
}

