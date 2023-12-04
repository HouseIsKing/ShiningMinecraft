using System.Runtime.InteropServices;
using System.Text;
using MinecraftLibrary.Input;
using MinecraftLibrary.Engine;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Network;

public sealed class Packet(PacketHeader header)
{
    public PacketHeader Header = header;
    private readonly List<byte> _data = new(4096);
    private int _dataPos = 0;

    public Packet(PacketHeader header, IEnumerable<byte> data) : this(header)
    {
        _data.AddRange(data);
    }

    public void Write(IEnumerable<byte> data)
    {
        _data.AddRange(data);
    }

    public void Write(string s)
    {
        Write(Encoding.UTF8.GetBytes(s));
        _data.Add(0);
    }

    public void Write(float item)
    {
        Write(BitConverter.GetBytes(item));
    }

    public void Write(long item)
    {
        Write(BitConverter.GetBytes(item));
    }

    public void Write(int item)
    {
        Write(BitConverter.GetBytes(item));
    }

    public void Write(ulong item)
    {
        Write(BitConverter.GetBytes(item));
    }

    public void Write(uint item)
    {
        Write(BitConverter.GetBytes(item));
    }

    public void Write(ushort item)
    {
        Write((byte)(item & 0xFF));
        Write((byte)(item >> 8));
    }

    public void Write(byte item)
    {
        _data.Add(item);
    }

    public void Write(ClientInput data)
    {
        Write(EngineDefaults.GetBytes(data));
    }

    public void Write(Vector3i data)
    {
        Write(data.X);
        Write(data.Y);
        Write(data.Z);
    }

    public void Write(Vector3 data)
    {
        Write(data.X);
        Write(data.Y);
        Write(data.Z);
    }

    public void Write(bool data)
    {
        Write(Convert.ToByte(data));
    }
    
    public void Write(Vector2i data)
    {
        Write(data.X);
        Write(data.Y);
    }

    public void Read(out IEnumerable<byte> data, int count)
    {
        data = _data.GetRange(_dataPos, count);
        _dataPos += count;
    }

    public void Read(out IEnumerable<byte> data)
    {
        Read(out data, _data.Count - _dataPos);
    }

    public void Read(out string s)
    {
        Read(out var data, _dataPos - _data.FindIndex(_dataPos, b => b == 0) + 1);
        s = Encoding.UTF8.GetString(data.ToArray());
    }

    public void Read(out float item)
    {
        Read(out var data, sizeof(float));
        item = BitConverter.ToSingle(data.ToArray());
    }

    public void Read(out long item)
    {
        Read(out var data, sizeof(long));
        item = BitConverter.ToInt64(data.ToArray());
    }

    public void Read(out int item)
    {
        Read(out var data, sizeof(int));
        item = BitConverter.ToInt32(data.ToArray());
    }

    public void Read(out ulong item)
    {
        Read(out var data, sizeof(ulong));
        item = BitConverter.ToUInt64(data.ToArray());
    }

    public void Read(out uint item)
    {
        Read(out var data, sizeof(uint));
        item = BitConverter.ToUInt32(data.ToArray());
    }

    public void Read(out ushort item)
    {
        Read(out var data, sizeof(ushort));
        item = BitConverter.ToUInt16(data.ToArray());
    }

    public void Read(out byte item)
    {
        item = _data[_dataPos];
        _dataPos++;
    }

    public void Read(out ClientInput data)
    {
        Read(out var bytes, Marshal.SizeOf<ClientInput>());
        data = EngineDefaults.FromBytes(bytes.ToArray());
    }

    public void Read(out Vector3i data)
    {
        Read(out data.X);
        Read(out data.Y);
        Read(out data.Z);
    }
    
    public void Read(out Vector3 data)
    {
        Read(out data.X);
        Read(out data.Y);
        Read(out data.Z);
    }

    public void Read(out bool data)
    {
        Read(out var bytes, sizeof(bool));
        data = BitConverter.ToBoolean(bytes.ToArray());
    }
    
    public void Read(out Vector2i data)
    {
        Read(out data.X);
        Read(out data.Y);
    }

    public byte[] ReadAll()
    {
        return _data.ToArray();
    }

    public byte[] ReadLeft()
    {
        return _data.GetRange(_dataPos, _data.Count - _dataPos).ToArray();
    }

    public void ResetRead()
    {
        _dataPos = 0;
    }

    public void Reset()
    {
        _dataPos = 0;
        _data.Clear();
    }

    public void WriteDataLength()
    {
        Header.Size = (uint)_data.Count;
    }
}