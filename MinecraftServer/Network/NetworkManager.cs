using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Network;

namespace MinecraftServer.Network;

internal sealed class NetworkManager
{
    private readonly TcpListener _socket;
    private const int Port = 25565;
    private readonly ConcurrentDictionary<byte, ConnectionToClient> _clientsJoined = new();
    private readonly ConcurrentQueue<ConnectionToClient> _clientsDisconnected = new();
    private readonly ConcurrentQueue<byte> _nextAvailableId = new();
    private readonly ConcurrentQueue<KeyValuePair<ConnectionToClient, Packet>> _incomingPackets = new();
    private readonly ConcurrentQueue<Packet> _availablePackets = new();

    internal NetworkManager()
    {
        for (var i = 0; i < EngineDefaults.PacketHistorySize; i++) _availablePackets.Enqueue(new Packet(new PacketHeader(PacketType.WorldChange)));
        for (var i = byte.MaxValue; i > 0; i--) _nextAvailableId.Enqueue(i);
        _nextAvailableId.Enqueue(0);
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
        _nextAvailableId.TryDequeue(out var id);
        _clientsJoined.TryAdd(id, new ConnectionToClient(client.Result, this));
        var t = _socket.AcceptTcpClientAsync();
        t.ContinueWith(OnClientConnect);
    }
    
    internal void DisconnectClient(ConnectionToClient client)
    {
        _clientsDisconnected.Enqueue(client);
    }

    internal void IncomingPacket(ConnectionToClient client, Packet packet)
    {
        packet.Unpack();
        _incomingPackets.Enqueue(KeyValuePair.Create(client, packet));
    }

    internal ConnectionToClient GetNextReadyClient()
    {
        var result = _clientsJoined.FirstOrDefault(static connectionToClient => connectionToClient.Value.Ready);
        if (result.Value != null) _clientsJoined.TryRemove(result.Key, out _);
        _nextAvailableId.Enqueue(result.Key);
        return result.Value;
    }
    
    internal ConnectionToClient GetNextClientDisconnected()
    {
        return _clientsDisconnected.TryDequeue(out var result) ? result : null;
    }

    internal KeyValuePair<ConnectionToClient, Packet>? GetNextIncomingPacket()
    {
        return _incomingPackets.TryDequeue(out var packet) ? packet : null;
    }
    
    internal void ReturnPacket(Packet packet)
    {
        _availablePackets.Enqueue(packet);
    }
    
    internal Packet GetNextAvailablePacket()
    {
        return _availablePackets.TryDequeue(out var packet) ? packet : new Packet(new PacketHeader(PacketType.WorldChange));
    }
}