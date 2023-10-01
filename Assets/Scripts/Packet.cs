using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>서버에서 클라이언트로 보낸 패킷 종류</summary>
public enum ServerPackets
{
    Welcome = 1,
    SpawnPlayer = 2,
    PlayerPosition = 3,
    PlayerRotation = 4,
    UDPTest = 5
}

/// <summary>클라이언트에서 서버로 보낸 패킷 종류</summary>
public enum ClientPackets
{
    WelcomeReceived = 1,
    PlayerMovement = 2,
    UDPTestReceived = 3,
}

public class Packet : IDisposable
{
    private List<byte> buffer;
    private byte[] readableBuffer;
    private int readPos;

    /// <summary>빈 패킷 생성 (without an ID).</summary>
    public Packet()
    {
        buffer = new List<byte>(); // Intitialize buffer
        readPos = 0; // Set readPos to 0
    }

    /// <summary>패킷 ID를 통한 패킷 생성. 데이터 전송을 위해서 사용함</summary>
    /// <param name="_id">packet ID.</param>
    public Packet(int _id)
    {
        buffer = new List<byte>(); // Intitialize buffer
        readPos = 0; // Set readPos to 0

        Write(_id); // Write packet id to the buffer
    }

    /// <summary>읽을 수 있는 데이터를 통해 패킷 생성. 데이터 받기를 위해서 사용함.</summary>
    /// <param name="_data">bytes to add to the packet.</param>
    public Packet(byte[] _data)
    {
        buffer = new List<byte>(); // Intitialize buffer
        readPos = 0; // Set readPos to 0

        SetBytes(_data);
    }

    #region Functions
    public void SetBytes(byte[] _data)
    {
        Write(_data);
        readableBuffer = buffer.ToArray();
    }
    public void WriteLength()
    {
        buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
    }
    public void InsertInt(int _value)
    {
        buffer.InsertRange(0, BitConverter.GetBytes(_value));
    }
    public byte[] ToArray()
    {
        readableBuffer = buffer.ToArray();
        return readableBuffer;
    }
    public int Length()
    {
        return buffer.Count;
    }
    public int UnreadLength()
    {
        return Length() - readPos;
    }

    /// <summary>패킷 인스턴스 재활용을 위한 리셋.</summary>
    /// <param name="_shouldReset">Whether or not to reset the packet.</param>
    public void Reset(bool _shouldReset = true)
    {
        //왜 이렇게 해야하냐..?? TCP는 데이터가 stream형식으로 와서 내가 보낸 패킷이 다른놈이랑 붙어있었을수도 있고,
        //패킷이 중간에 짤려서 다음에 나머지가 올수도 있음...
        if (_shouldReset)
        {
            buffer.Clear();
            readableBuffer = null;
            readPos = 0;
        }
        else
        {
            readPos -= 4; // 아직 읽을게 남아있었으므로 int크기 만큼 뒤로 감
        }
    }
    #endregion

    #region Write Data
    public void Write(byte _value)
    {
        buffer.Add(_value);
    }
    public void Write(byte[] _value)
    {
        buffer.AddRange(_value);
    }
    public void Write(short _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    public void Write(int _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    public void Write(long _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    public void Write(float _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    public void Write(bool _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }
    public void Write(string _value)
    {
        Write(_value.Length);
        buffer.AddRange(Encoding.UTF8.GetBytes(_value));
    }
    public void Write(Vector3 _value)
    {
        Write(_value.x);
        Write(_value.y);
        Write(_value.z);
    }
    public void Write(Quaternion _value)
    {
        Write(_value.x);
        Write(_value.y);
        Write(_value.z);
        Write(_value.w);
    }
    #endregion

    #region Read Data
    public byte ReadByte(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            // 아직 안읽은 byte가 있다면
            byte _value = readableBuffer[readPos];
            if (_moveReadPos)
            {
                readPos += 1;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'byte'!");
        }
    }

    public byte[] ReadBytes(int _length, bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            byte[] _value = buffer.GetRange(readPos, _length).ToArray();
            if (_moveReadPos)
            {
                readPos += _length;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'byte[]'!");
        }
    }
    public short ReadShort(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            short _value = BitConverter.ToInt16(readableBuffer, readPos);
            if (_moveReadPos)
            {
                readPos += 2;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'short'!");
        }
    }
    public int ReadInt(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            int _value = BitConverter.ToInt32(readableBuffer, readPos);
            if (_moveReadPos)
            {
                readPos += 4; // readPos 를 4 늘린다. 왜??? int32 는 4바이트니까.
            }
            return _value; //byte에서 변환된 int값 반환
        }
        else
        {
            throw new Exception("Could not read value of type 'int'!");
        }
    }
    public long ReadLong(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            long _value = BitConverter.ToInt64(readableBuffer, readPos);
            if (_moveReadPos)
            {
                readPos += 8;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'long'!");
        }
    }
    public float ReadFloat(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            float _value = BitConverter.ToSingle(readableBuffer, readPos);
            if (_moveReadPos)
            {
                readPos += 4;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'float'!");
        }
    }
    public bool ReadBool(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            bool _value = BitConverter.ToBoolean(readableBuffer, readPos);
            if (_moveReadPos)
            {
                readPos += 1;
            }
            return _value;
        }
        else
        {
            throw new Exception("Could not read value of type 'bool'!");
        }
    }
    public string ReadString(bool _moveReadPos = true)
    {
        try
        {
            int _length = ReadInt();
            string _value = Encoding.UTF8.GetString(readableBuffer, readPos, _length);
            if (_moveReadPos && _value.Length > 0)
            {
                readPos += _length;//string은 앞에 byte로 저장해둔 _length값을 토대로 그 길이만큼 readPos증가
            }
            return _value;
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }
    public Vector3 ReadVector3(bool _moveReadPos = true)
    {
        return new Vector3(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
    }
    public Quaternion ReadQuaternion(bool _moveReadPos = true)
    {
        return new Quaternion(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
    }
    #endregion

    private bool disposed = false;

    protected virtual void Dispose(bool _disposing)
    {
        if (!disposed)
        {
            if (_disposing)
            {
                buffer = null;
                readableBuffer = null;
                readPos = 0;
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}