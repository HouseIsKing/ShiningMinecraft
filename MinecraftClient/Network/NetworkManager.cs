using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MinecraftLibrary.Network;

namespace MinecraftClient.Network;

public sealed class NetworkManager
{
    private readonly Socket _socket;
    private PacketHeader _headerRead;
    private PacketHeader _headerWrite;
    private uint _readCounter;
    private uint _writeCounter;
    private readonly byte[] _packetBuffer = new byte[16777216];
    private readonly byte[] _packetBufferWrite = new byte[1024];
    private readonly ConcurrentQueue<Packet> _outgoingPackets = new();
    private Task _sendTask = Task.CompletedTask;
    private readonly ConcurrentQueue<Packet> _incomingPackets = new();

    public NetworkManager(IPAddress ipAddr, ushort port, string name)
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.NoDelay = true;
        _socket.ReceiveTimeout = 100;
        _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.TypeOfService, 0x18);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
        _socket.Connect(new IPEndPoint(ipAddr, port));
        Console.WriteLine("Connected to server");
        var idPacket = new Packet(PacketHeader.PlayerIdPacket, Encoding.UTF8.GetBytes(name.ToArray()));
        _socket.Send(idPacket.Header.GetBytes());
        _socket.Send(idPacket.ReadAll());
        var t = _socket.ReceiveAsync(new ArraySegment<byte>(_packetBuffer, 0, 8));
        t.ContinueWith(OnPacketHeader);
    }

    private void OnPacketHeader(Task<int> result)
    {
        Task<int> t;
        if (result.Result + _readCounter < 8)
        {
            _readCounter += (uint)result.Result;
            t = _socket.ReceiveAsync(new ArraySegment<byte>(_packetBuffer, (int)_readCounter, 8 - (int)_readCounter));
            t.ContinueWith(OnPacketHeader);
            return;
        }

        _readCounter = 0;
        _headerRead.Type = (PacketType)BitConverter.ToUInt32(_packetBuffer, 0);
        _headerRead.Size = BitConverter.ToUInt32(_packetBuffer, 4);
        t = _socket.ReceiveAsync(new ArraySegment<byte>(_packetBuffer, 0, (int)_headerRead.Size));
        t.ContinueWith(OnPacketData);
    }

    private void OnPacketData(Task<int> result)
    {
        Task<int> t;
        if (result.Result + _readCounter < _headerRead.Size)
        {
            _readCounter += (uint)result.Result;
            t = _socket.ReceiveAsync(new ArraySegment<byte>(_packetBuffer, (int)_readCounter,
                (int)(_headerRead.Size - _readCounter)));
            t.ContinueWith(OnPacketData);
            return;
        }

        _readCounter = 0;
        var packet = new Packet(_headerRead,
            new ArraySegment<byte>(_packetBuffer, 0, (int)_headerRead.Size));
        packet.Unpack();
        _incomingPackets.Enqueue(packet);
        t = _socket.ReceiveAsync(new ArraySegment<byte>(_packetBuffer, 0, 8));
        t.ContinueWith(OnPacketHeader);
    }

    internal void SendPacket(Packet packet)
    {
        packet.Package();
        _outgoingPackets.Enqueue(packet);
        if (!_sendTask.IsCompleted) return;
        _outgoingPackets.TryDequeue(out packet);
        _writeCounter = 0;
        if (packet != null)
        {
            _headerWrite = packet.Header;
            Buffer.BlockCopy(packet.Header.GetBytes(), 0, _packetBufferWrite, 0, 8);
            Buffer.BlockCopy(packet.ReadAll(), 0, _packetBufferWrite, 8, (int)packet.Header.Size);
        }

        var t = _socket.SendAsync(new ArraySegment<byte>(_packetBufferWrite, 0, (int)_headerWrite.Size + 8),
            SocketFlags.None);
        _sendTask = t.ContinueWith(OnPacketSend);
    }

    private void OnPacketSend(Task<int> result)
    {
        if (result.Result + _writeCounter < _headerWrite.Size + 8)
        {
            _writeCounter += (uint)result.Result;
            var t = _socket.SendAsync(
                new ArraySegment<byte>(_packetBufferWrite, (int)_writeCounter,
                    (int)_headerWrite.Size + 8 - (int)_writeCounter), SocketFlags.None);
            _sendTask = t.ContinueWith(OnPacketSend);
        }
        else
        {
            if (!_outgoingPackets.TryDequeue(out var packet)) return;
            _writeCounter = 0;
            _headerWrite = packet.Header;
            Buffer.BlockCopy(packet.Header.GetBytes(), 0, _packetBufferWrite, 0, 8);
            Buffer.BlockCopy(packet.ReadAll(), 0, _packetBufferWrite, 8, (int)packet.Header.Size);
            var t = _socket.SendAsync(new ArraySegment<byte>(_packetBufferWrite, 0, (int)_headerWrite.Size + 8),
                SocketFlags.None);
            _sendTask = t.ContinueWith(OnPacketSend);
        }
    }

    ~NetworkManager()
    {
        _socket.Close();
    }

    public Packet GetNextIncomingPacket()
    {
        return _incomingPackets.TryDequeue(out var packet) ? packet : null;
    }
}