﻿using System;
using System.IO;
using System.Text;

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