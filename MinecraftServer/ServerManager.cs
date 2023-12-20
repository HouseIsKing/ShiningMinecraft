using System.Diagnostics;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Engine.Entities;
using MinecraftLibrary.Network;
using MinecraftServer.Network;
using OpenTK.Mathematics;

namespace MinecraftServer;

internal sealed class ServerManager
{
    private readonly World _world = new();
    private double _tickTimer;
    private readonly Dictionary<ConnectionToClient, Player> _players = new();
    private (ushort, ConnectionToClient) _newlyJoinedClient = (0, null);
    private readonly NetworkManager _networkManager = new();
    private readonly Packet[] _packetHistory = new Packet[EngineDefaults.PacketHistorySize];
    private long _lastTickTime;

    internal ServerManager()
    {
        _world.GenerateLevel();
        Console.WriteLine("Server Started");
        for (var i = 0; i < _packetHistory.Length; i++) _packetHistory[i] = new Packet(new PacketHeader(PacketType.WorldChange));
        _world.OnTickStart += PreTick;
    }

    ~ServerManager()
    {
        _world.SaveWorld();
    }

    private void PreTick()
    {
        if (_newlyJoinedClient.Item2 != null)
        {
            var worldPacket = new Packet(new PacketHeader(PacketType.WorldState));
            _world.Serialize(worldPacket);
            _newlyJoinedClient.Item2.SendPacket(worldPacket);
            _players.Add(_newlyJoinedClient.Item2, _world.GetPlayer(_newlyJoinedClient.Item1));
            _newlyJoinedClient = (0, null);
        }

        var newClient = _networkManager.GetNextReadyClient();
        if (newClient != null)
        {
            var state = _world.SpawnPlayer();
            state.Position = new Vector3(5.0f, 70.0f, 5.0f);
            var packetToSend = new Packet(PacketHeader.PlayerIdPacket);
            packetToSend.Write(state.EntityId);
            newClient.SendPacket(packetToSend);
            _newlyJoinedClient = (state.EntityId, newClient);
        }
        
        var disconnectedClient = _networkManager.GetNextClientDisconnected();
        if (disconnectedClient != null)
        {
            _world.DespawnPlayer(_players[disconnectedClient].State.EntityId);
            _players.Remove(disconnectedClient);
        }

        var incomingPacket = _networkManager.GetNextIncomingPacket();
        while (incomingPacket != null)
        {
            var (client, packet) = incomingPacket.Value;
            switch (packet.Header.Type)
            {
                case PacketType.ClientInput:
                    packet.Read(out uint inputId);
                    var input = EngineDefaults.FromBytes(packet.ReadLeft());
                    _players[client].AddInput(inputId, input);
                    break;
            }
            _networkManager.ReturnPacket(packet);
            incomingPacket = _networkManager.GetNextIncomingPacket();
        }
    }

    private void PostTick()
    {
        var packet = _packetHistory[_world.GetWorldTime() % (ulong)_packetHistory.Length];
        foreach (var connection in _players)
        {
            packet.Header.Type = (PacketType)(connection.Value.LastInputProcessed);
            connection.Key.SendPacketPacked(packet);
        }
    }

    internal void Run()
    {
        if (_lastTickTime == 0) _lastTickTime = Stopwatch.GetTimestamp();
        var i = (int)(_tickTimer / EngineDefaults.TickRate);
        for (; i > 0; i--)
        {
            var start = Stopwatch.GetTimestamp();
            _world.Tick(_packetHistory[(_world.GetWorldTime() + 1ul) % (ulong)_packetHistory.Length], false);
            _packetHistory[_world.GetWorldTime() % (ulong)_packetHistory.Length].Package();
            PostTick();
            _tickTimer -= EngineDefaults.TickRate; 
            var timeTook = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            Console.WriteLine($"Tick took {timeTook} milliseconds");
        }
        
        //_tickTimer -= _tickTimer / EngineDefaults.TickRate;
        while (Stopwatch.GetElapsedTime(_lastTickTime).TotalMilliseconds < 0.1)
        {
        }

        var timer = Stopwatch.GetTimestamp();
        _tickTimer += new TimeSpan(timer - _lastTickTime).TotalSeconds;
        _lastTickTime = timer;
    }
}