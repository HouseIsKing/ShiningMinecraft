using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using MinecraftLibrary.Network;

namespace MinecraftServer.Network;

public sealed class ConnectionToClient
{
    public string Username { get; private set; } = "";
    public ulong LastTickSent { get; set; }
    public ulong LastInputProcessed { get; set; }
    private readonly TcpClient _socket;
    private readonly NetworkManager _networkManager;
    private readonly Packet _packetRead = new(PacketHeader.ClientInputHeader);
    private readonly byte[] _headerBuffer = new byte[8];
    private byte[] _packetBuffer = Array.Empty<byte>();
    private Packet? _packetWrite = new(PacketHeader.ClientInputHeader);
    private readonly ConcurrentQueue<Packet> _outgoingPackets = new();
    private readonly Thread _sendThread;
    public bool Ready { get; private set; }

    public ConnectionToClient(TcpClient socket, NetworkManager networkManager)
    {
        _socket = socket;
        _networkManager = networkManager;
        _socket.Client.BeginReceive(_headerBuffer, 0, 8, SocketFlags.None, OnPacketHeader, null);
        _sendThread = new Thread(SendPacketsThread);
        _sendThread.Start();
    }

    private void OnPacketHeader(IAsyncResult result)
    {
        _packetRead.Reset();
        _packetRead.Header.Type = (PacketType)BitConverter.ToUInt32(_headerBuffer, 0);
        _packetRead.Header.Size = BitConverter.ToUInt32(_headerBuffer, 4);
        _packetBuffer = new byte[_packetRead.Header.Size];
        _socket.Client.BeginReceive(_packetBuffer, 0, (int)_packetRead.Header.Size, SocketFlags.None, OnPacketData, null);
    }

    private void OnPacketData(IAsyncResult result)
    {
        _packetRead.Write(_packetBuffer);
        switch (_packetRead.Header.Type)
        {
            case PacketType.PlayerId:
                if (!Ready)
                {
                    Username = Encoding.UTF8.GetString(_packetBuffer);
                    Ready = true;
                }

                break;
            case PacketType.ClientInput:
                _networkManager.IncomingPacket(this, _packetRead);
                break;
        }

        _socket.Client.BeginReceive(_headerBuffer, 0, 8, SocketFlags.None, OnPacketHeader, null);
    }

    public void SendPacket(Packet packet)
    {
        _outgoingPackets.Enqueue(packet);
    }

    ~ConnectionToClient()
    {
        _socket.Close();
        _sendThread.Join();
    }

    private void SendPacketsThread()
    {
        while (true)
        {
            if (!_outgoingPackets.TryDequeue(out _packetWrite)) continue;

            _socket.Client.Send(_packetWrite.Header.getBytes(), 0, 8, SocketFlags.None);
            _socket.Client.Send(_packetWrite.ReadAll(), 0, (int)_packetWrite.Header.Size, SocketFlags.None);
        }
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Username.GetHashCode();
    }
}