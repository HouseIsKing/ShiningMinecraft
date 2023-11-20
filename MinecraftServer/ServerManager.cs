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
    private float _tickTimer;
    private readonly Dictionary<ConnectionToClient, Player> _players = new();
    private readonly Dictionary<ushort, ConnectionToClient> _newlyJoinedClients = new();
    private readonly NetworkManager _networkManager = new();
    private readonly Packet[] _packetHistory = new Packet[EngineDefaults.PacketHistorySize];

    internal ServerManager()
    {
        _world.GenerateLevel();
        Console.WriteLine("Server Started");
        for (var i = 0; i < _packetHistory.Length; i++)
        {
            _packetHistory[i] = new Packet(new PacketHeader(PacketType.WorldChange));
        }
        _world.OnTickStart += PreTick;
    }

    ~ServerManager()
    {
        _world.SaveWorld();
    }

    private void PreTick()
    {
        var newClient = _networkManager.GetNextReadyClient();
        if (newClient != null)
        {
            var state = _world.SpawnPlayer();
            state.Position = new Vector3(0, 70.0f, 0.0f);
            _newlyJoinedClients.Add(state.EntityId, newClient);
        }

        var incomingPacket = _networkManager.GetNextIncomingPacket();
        if (incomingPacket == null) return;

        var (client, packet) = incomingPacket.Value;
        switch (packet.Header.Type)
        {
            case PacketType.ClientInput:
                packet.Read(out ulong inputId);
                var input = EngineDefaults.FromBytes(packet.ReadLeft());
                var player = _players[client];
                player.AddInput(inputId, input);
                break;
        }
    }

    private void PostTick()
    {
        foreach (var connection in _players.Where(static connection =>
                     connection.Key.LastInputProcessed != connection.Value.LastInputProcessed))
        {
            for (var i = connection.Key.LastTickSent + 1; i <= _world.GetWorldTime(); i++)
                connection.Key.SendPacket(_packetHistory[i % (ulong)_packetHistory.Length]);
            connection.Key.LastTickSent = _world.GetWorldTime();
            connection.Key.LastInputProcessed = connection.Value.LastInputProcessed;
        }

        if (_newlyJoinedClients.Count <= 0) return;

        var packet = new Packet(new PacketHeader(PacketType.WorldState));
        _world.Serialize(packet);
        packet.WriteDataLength();
        foreach (var connection in _newlyJoinedClients)
        {
            connection.Value.SendPacket(packet);
            _players.Add(connection.Value, _world.GetPlayer(connection.Key));
        }

        _newlyJoinedClients.Clear();
    }

    internal void Run()
    {
        var start = Stopwatch.GetTimestamp();
        for (var i = 0; i < _tickTimer / EngineDefaults.TickRate; i++)
        {
            _world.Tick(_packetHistory[(_world.GetWorldTime() + 1ul) % (ulong)_packetHistory.Length], false);
            PostTick();
            _tickTimer -= EngineDefaults.TickRate;
        }

        //_tickTimer -= _tickTimer / EngineDefaults.TickRate;
        var timeTook = (float)Stopwatch.GetElapsedTime(start).TotalSeconds;
        _tickTimer += timeTook;
        if (timeTook > 0.001f) Console.WriteLine($"Tick took {timeTook} seconds");
    }
}