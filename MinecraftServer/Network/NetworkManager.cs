using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using MinecraftLibrary.Network;

namespace MinecraftServer.Network;

public sealed class NetworkManager
{
    private readonly TcpListener _socket;
    private const int Port = 25565;
    private readonly ConcurrentDictionary<byte, ConnectionToClient> _clientsJoined = new();
    private ConcurrentQueue<byte> NextAvailableId = new();
    private readonly ConcurrentQueue<KeyValuePair<ConnectionToClient, Packet>> _incomingPackets = new();

    public NetworkManager()
    {
        for (var i = byte.MaxValue; i > 0; i--) NextAvailableId.Enqueue(i);
        NextAvailableId.Enqueue(0);
        _socket = new TcpListener(IPAddress.Any, Port);
        _socket.Start();
        var t = _socket.AcceptTcpClientAsync();
        t.ContinueWith(OnClientConnect);
    }

    private void OnClientConnect(Task<TcpClient> client)
    {
        client.Result.NoDelay = true;
        client.Result.ReceiveTimeout = 100;
        client.Result.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.TypeOfService, 0x18);
        client.Result.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
        client.Result.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
        NextAvailableId.TryDequeue(out var id);
        _clientsJoined.TryAdd(id, new ConnectionToClient(client.Result, this));
        var t = _socket.AcceptTcpClientAsync();
        t.ContinueWith(OnClientConnect);
    }

    internal void IncomingPacket(ConnectionToClient client, Packet packet)
    {
        packet.Unpack();
        _incomingPackets.Enqueue(KeyValuePair.Create(client, packet));
    }

    public ConnectionToClient GetNextReadyClient()
    {
        var result = _clientsJoined.FirstOrDefault(static connectionToClient => connectionToClient.Value.Ready);
        if (result.Value != null) _clientsJoined.TryRemove(result.Key, out _);
        return result.Value;
    }
    
    public KeyValuePair<ConnectionToClient, Packet>? GetNextIncomingPacket()
    {
        return _incomingPackets.TryDequeue(out var packet) ? packet : null;
    }
}