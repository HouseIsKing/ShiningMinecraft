using System.Runtime.InteropServices;
using System.Text;
using MinecraftLibrary.Input;
using MinecraftLibrary.Engine;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Network;

public class Packet
{
    private PacketHeader Header;
    private readonly List<byte> Data = new(4096);
    private int DataPos = 0;

    public Packet(PacketHeader header)
    {
        Header = header;
    }

    public Packet(PacketHeader header, IEnumerable<byte> data) : this(header)
    {
        Data.AddRange(data);
    }

    public void Write(IEnumerable<byte> data)
    {
        Data.AddRange(data);
    }

    public void Write(string s)
    {
        Write(Encoding.UTF8.GetBytes(s));
        Data.Add(0);
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
        Write(BitConverter.GetBytes(item));
    }

    public void Write(byte item)
    {
        Data.Add(item);
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
        Write(BitConverter.GetBytes(data));
    }
    
    public void Write(Vector2i data)
    {
        Write(data.X);
        Write(data.Y);
    }

    public void Read(out IEnumerable<byte> data, int count)
    {
        data = Data.GetRange(DataPos, count);
        DataPos += count;
    }

    public void Read(out IEnumerable<byte> data)
    {
        Read(out data, Data.Count - DataPos);
    }

    public void Read(out string s)
    {
        Read(out var data, DataPos - Data.FindIndex(DataPos, b => b == 0) + 1);
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
        item = Data[0];
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

    public void Reset()
    {
        DataPos = 0;
    }
}