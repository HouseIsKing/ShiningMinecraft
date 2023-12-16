using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using MinecraftLibrary.Network;

namespace MinecraftServer.Network;

public sealed class ConnectionToClient
{
    internal string Username { get; private set; } = "";
    private readonly TcpClient _socket;
    private readonly NetworkManager _networkManager;
    private PacketHeader _headerRead;
    private PacketHeader _headerWrite;
    private uint _readCounter;
    private uint _writeCounter;
    private readonly byte[] _packetBuffer = new byte[1024];
    private readonly byte[] _packetBufferWrite = new byte[16777216];
    private readonly ConcurrentQueue<Packet> _outgoingPackets = new();
    private Task _sendTask = Task.CompletedTask;
    internal bool Ready { get; private set; }

    internal ConnectionToClient(TcpClient socket, NetworkManager networkManager)
    {
        _socket = socket;
        _networkManager = networkManager;
        var t = _socket.Client.ReceiveAsync(new ArraySegment<byte>(_packetBuffer, 0, 8));
        t.ContinueWith(OnPacketHeader);
    }

    private void OnPacketHeader(Task<int> result)
    {
        Task<int> t;
        if (result.Result + _readCounter < 8)
        {
            _readCounter += (uint)result.Result;
            t = _socket.Client.ReceiveAsync(new ArraySegment<byte>(_packetBuffer, (int)_readCounter, 8 - (int)_readCounter));
            t.ContinueWith(OnPacketHeader);
            return;
        }

        _readCounter = 0;
        _headerRead.Type = (PacketType)BitConverter.ToUInt32(_packetBuffer, 0);
        _headerRead.Size = BitConverter.ToUInt32(_packetBuffer, 4);
        t = _socket.Client.ReceiveAsync(new ArraySegment<byte>(_packetBuffer, 0, (int)_headerRead.Size));
        t.ContinueWith(OnPacketData);
    }

    private void OnPacketData(Task<int> result)
    {
        Task<int> t;
        if (result.Result + _readCounter < _headerRead.Size)
        {
            _readCounter += (uint)result.Result;
            t = _socket.Client.ReceiveAsync(new ArraySegment<byte>(_packetBuffer, (int)_readCounter, (int)(_headerRead.Size - _readCounter)));
            t.ContinueWith(OnPacketData);
            return;
        }

        _readCounter = 0;
        switch (_headerRead.Type)
        {
            case PacketType.PlayerId:
                if (!Ready)
                {
                    Username = Encoding.UTF8.GetString(_packetBuffer, 0, (int)_headerRead.Size);
                    Ready = true;
                }
                break;
            case PacketType.ClientInput:
                _networkManager.IncomingPacket(this, new Packet(_headerRead, new ArraySegment<byte>(_packetBuffer, 0, (int)_headerRead.Size)));
                break;
        }
        t = _socket.Client.ReceiveAsync(new ArraySegment<byte>(_packetBuffer, 0, 8));
        t.ContinueWith(OnPacketHeader);
    }

    internal void SendPacketPacked(Packet packet)
    {
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

        var t = _socket.Client.SendAsync(new ArraySegment<byte>(_packetBufferWrite, 0, (int)_headerWrite.Size + 8), SocketFlags.None);
        _sendTask = t.ContinueWith(OnPacketSend);
    }

    internal void SendPacket(Packet packet)
    {
        packet.Package();
        SendPacketPacked(packet);
    }

    private void OnPacketSend(Task<int> result)
    {
        if (result.Result + _writeCounter < _headerWrite.Size + 8)
        {
            _writeCounter += (uint)result.Result;
            var t = _socket.Client.SendAsync(new ArraySegment<byte>(_packetBufferWrite, (int)_writeCounter, (int)_headerWrite.Size + 8 - (int)_writeCounter), SocketFlags.None);
            _sendTask = t.ContinueWith(OnPacketSend);
        }
        else
        {
            if (!_outgoingPackets.TryDequeue(out var packet)) return;
            _writeCounter = 0;
            _headerWrite = packet.Header;
            Buffer.BlockCopy(packet.Header.GetBytes(), 0, _packetBufferWrite, 0, 8);
            Buffer.BlockCopy(packet.ReadAll(), 0, _packetBufferWrite, 8, (int)packet.Header.Size);
            var t = _socket.Client.SendAsync(new ArraySegment<byte>(_packetBufferWrite, 0, (int)_headerWrite.Size + 8), SocketFlags.None);
            _sendTask = t.ContinueWith(OnPacketSend);
        }
    }

    ~ConnectionToClient()
    {
        _socket.Close();
    }

    public override int GetHashCode()
    {
        return Username.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is ConnectionToClient client && client.Username == Username;
    }
}