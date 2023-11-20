using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using MinecraftLibrary.Network;

namespace MinecraftServer.Network;

public class NetworkManager
{
    private readonly TcpListener _socket;
    private const int Port = 25565;
    private readonly ConcurrentQueue<ConnectionToClient> _clientsJoined = new();
    private readonly ConcurrentQueue<KeyValuePair<ConnectionToClient, Packet>> _incomingPackets = new();

    public NetworkManager()
    {
        _socket = new TcpListener(IPAddress.Any, Port);
        _socket.Start();
        _socket.BeginAcceptTcpClient(OnClientConnect, null);
    }

    private void OnClientConnect(IAsyncResult result)
    {
        var client = _socket.EndAcceptTcpClient(result);
        client.NoDelay = true;
        client.ReceiveTimeout = 100;
        client.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.TypeOfService, 0x18);
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 100);
        _clientsJoined.Enqueue(new ConnectionToClient(client, this));
        _socket.BeginAcceptTcpClient(OnClientConnect, null);
    }

    internal void IncomingPacket(ConnectionToClient client, Packet packet)
    {
        _incomingPackets.Enqueue(KeyValuePair.Create(client, packet));
    }

    public ConnectionToClient? GetNextReadyClient()
    {
        return _clientsJoined.FirstOrDefault(static connectionToClient => connectionToClient.Ready);
    }
    
    public KeyValuePair<ConnectionToClient, Packet>? GetNextIncomingPacket()
    {
        return _incomingPackets.TryDequeue(out var packet) ? packet : null;
    }
}