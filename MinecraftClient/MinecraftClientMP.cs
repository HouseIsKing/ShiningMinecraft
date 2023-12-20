using System.Diagnostics;
using System.Net;
using MinecraftClient.Network;
using MinecraftLibrary.Engine;
using MinecraftLibrary.Input;
using MinecraftLibrary.Network;
using OpenTK.Windowing.Common;

namespace MinecraftClient;

public sealed class MinecraftClientMp : MinecraftClient
{
    private readonly NetworkManager _networkManager;
    private readonly SortedDictionary<uint, ClientInput> _inputHistory = new();
    private readonly Packet _packetToRevert = new(new PacketHeader(PacketType.WorldChange));
    
    public MinecraftClientMp(IPAddress address, string name) : base(new World())
    {
        _networkManager = new NetworkManager(address, 25565, name);
        InitiateWorld();
    }

    private void InitiateWorld()
    {
        var incomingPacket = _networkManager.GetNextIncomingPacket();
        while (incomingPacket == null) incomingPacket = _networkManager.GetNextIncomingPacket();
        incomingPacket.Read(out ushort entityId);
        incomingPacket = _networkManager.GetNextIncomingPacket();
        while (incomingPacket == null) incomingPacket = _networkManager.GetNextIncomingPacket();
        World.Deserialize(incomingPacket);
        Player = World.GetPlayer(entityId).State;
        WorldRenderer.PlayerRenderer.Player = Player;
        
    }

    protected override void PreTick()
    {
        base.PreTick();
        _inputHistory.Add(InputId, Input);
        var packetToSend = new Packet(PacketHeader.ClientInputHeader);
        packetToSend.Write(InputId);
        packetToSend.Write(Input);
        _networkManager.SendPacket(packetToSend);
        Input = new ClientInput();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        var packet = _networkManager.GetNextIncomingPacket();
        while (packet != null)
        {
            PacketReceived(packet);
            packet = _networkManager.GetNextIncomingPacket();
        }
    }

    private void PacketReceived(Packet packet)
    {
        var start = Stopwatch.GetTimestamp();
        _packetToRevert.Reset();
        for (var i = 0; i < _inputHistory.Count; i++) World.DeserializeChanges(PacketHistory[(InputId - (ulong)i) % (ulong)PacketHistory.Length]);
        World.DeserializeChanges(packet);
        World.SerializeChangesToRevertPacket(_packetToRevert);
        WorldRenderer.ApplyTickChanges(_packetToRevert);
        _inputHistory.Remove((uint)packet.Header.Type);
        if (_inputHistory.Count > 15)
        {
            _inputHistory.Clear();
            Ticker -= EngineDefaults.TickRate * 15;
        }
        else
        {
            _packetToRevert.Reset();
            foreach (var input in _inputHistory)
            {
                World.GetPlayer(Player.EntityId).AddInput(input.Key, input.Value);
                World.SimulateTick(PacketHistory[input.Key % (ulong)PacketHistory.Length], true);
            }
            World.SerializeChangesToRevertPacket(_packetToRevert);
            WorldRenderer.ApplyTickChanges(_packetToRevert);
        }
        _networkManager.ReturnPacket(packet);
        var timeTook = (float)Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        Console.WriteLine($"Packet took {timeTook} ms");
    }
}