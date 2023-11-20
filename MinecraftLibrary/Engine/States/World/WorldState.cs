using System.IO.Compression;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.States.World;

public sealed class WorldState : State<WorldState>
{
    private readonly Dictionary<Vector3i, ChunkState> _chunks = new();
    private readonly Dictionary<Vector2i, byte> _lights = new();
    private readonly Dictionary<ushort, PlayerState> _players = new();
    private readonly Dictionary<ushort, BlockParticleEntityState> _blockParticles = new();
    private readonly Dictionary<ushort, EntityType> _entityIdToType = new();
    private readonly Dictionary<Vector2i, byte> _lightUpdates = new();
    private readonly HashSet<ushort> _newEntities = new();
    private readonly Dictionary<ushort, KeyValuePair<EntityType, object>> _removedEntities = new();
    internal Random Random { get; }
    public ulong WorldTime;
    private long _prevRandomSeed;

    public long Seed { get; }

    public WorldState(long seed)
    {
        Seed = seed;
        Random = new Random(seed);
        _entityIdToType.EnsureCapacity(ushort.MaxValue);
        for (var i = ushort.MaxValue; i > 0; i++)
            _entityIdToType.Add(i, EntityType.Null);
        _entityIdToType.Add(0, EntityType.Null);
    }

    public WorldState()
    {
        Seed = -1;
        Random = new Random();
        _entityIdToType.EnsureCapacity(ushort.MaxValue);
        for (var i = ushort.MaxValue; i > 0; i++)
            _entityIdToType.Add(i, EntityType.Null);
        _entityIdToType.Add(0, EntityType.Null);
    }

    private void SerializeChunks(Packet packet)
    {
        packet.Write(_chunks.Count);
        foreach (var chunk in _chunks.Values)
            chunk.Serialize(packet);
    }
    private void SerializeLights(Packet packet)
    {
        packet.Write(_lights.Count);
        foreach (var light in _lights)
        {
            packet.Write(light.Key);
            packet.Write(light.Value);
        }
    }
    private void SerializePlayers(Packet packet)
    {
        packet.Write(_players.Count);
        foreach (var player in _players.Values)
            player.Serialize(packet);
    }
    public override void Serialize(Packet packet)
    {
        packet.Write(WorldTime);
        packet.Write(Random.GetSeed());
        SerializeChunks(packet);
        SerializeLights(packet);
        SerializePlayers(packet);
    }
    public override void Deserialize(Packet packet)
    {
        packet.Read(out WorldTime);
        packet.Read(out long seed);
        Random.SetSeed(seed);
        DeserializeChunks(packet);
        DeserializeLights(packet);
        DeserializePlayers(packet);
    }
    private void DeserializeChunks(Packet packet)
    {
        packet.Read(out int chunkCount);
        _chunks.EnsureCapacity(chunkCount);
        for (var i = 0; i < chunkCount; i++)
        {
            packet.Read(out Vector3i chunkPosition);
            var chunk = new ChunkState(chunkPosition);
            chunk.Deserialize(packet);
            _chunks.Add(chunkPosition, chunk);
        }
    }
    private void DeserializeLights(Packet packet)
    {
        packet.Read(out int lightCount);
        _lights.EnsureCapacity(lightCount);
        for (var i = 0; i < lightCount; i++)
        {
            packet.Read(out Vector2i lightPosition);
            packet.Read(out byte lightLevel);
            _lights.Add(lightPosition, lightLevel);
        }
    }
    private void DeserializePlayers(Packet packet)
    {
        packet.Read(out int playerCount);
        for (var i = 0; i < playerCount; i++)
        {
            packet.Read(out ushort playerId);
            _entityIdToType[playerId] = EntityType.Player;
            var player = new PlayerState(playerId);
            player.Deserialize(packet);
            RegisterPlayer(player);
        }
    }
    public override void SerializeChangesToChangePacket(Packet changePacket)
    {
        changePacket.Write(WorldTime);
        changePacket.Write(Random.GetSeed());
        SerializeChangesChunks(changePacket);
        SerializeChangesLights(changePacket);
        SerializeChangesPlayers(changePacket);
        SerializeChangesEntity(changePacket);
    }
    private void SerializeChangesChunks(Packet changePacket)
    {
        changePacket.Write(_chunks.Values.Count(static state => state.IsDirty));
        foreach (var state in _chunks.Values.Where(static state => state.IsDirty))
        {
            changePacket.Write(state.ChunkPosition);
            state.SerializeChangesToChangePacket(changePacket);
        }
    }
    private void SerializeChangesLights(Packet changePacket)
    {
        changePacket.Write(_lightUpdates.Count);
        foreach (var lightUpdate in _lightUpdates)
        {
            changePacket.Write(lightUpdate.Key);
            changePacket.Write(_lights[lightUpdate.Key]);
        }
    }
    private void SerializeChangesPlayers(Packet changePacket)
    {
        var count = _players.Values.Count(static state => state.IsDirty);
        changePacket.Write(count);
        foreach (var entity in _players.Values.Where(static state => state.IsDirty))
        {
            changePacket.Write(entity.EntityId);
            entity.SerializeChangesToChangePacket(changePacket);
        }
    }
    private void SerializeChangesEntity(Packet changePacket)
    {
        changePacket.Write(_newEntities.Count);
        foreach (var newEntity in _newEntities)
        {
            changePacket.Write(newEntity);
            changePacket.Write((byte)_entityIdToType[newEntity]);
        }
        changePacket.Write(_removedEntities.Count);
        foreach (var removedEntity in _removedEntities)
        {
            changePacket.Write(removedEntity.Key);
            changePacket.Write((byte)EntityType.Null);
        }
    }
    public override void SerializeChangesToRevertPacket(Packet revertPacket)
    {
        revertPacket.Write(WorldTime - 1);
        revertPacket.Write(_prevRandomSeed);
        SerializeRevertChunks(revertPacket);
        SerializeRevertLights(revertPacket);
        SerializeRevertPlayers(revertPacket);
        SerializeRevertEntity(revertPacket);
    }
    private void SerializeRevertChunks(Packet revertPacket)
    {
        revertPacket.Write(_chunks.Values.Count(static state => state.IsDirty));
        foreach (var state in _chunks.Values.Where(static state => state.IsDirty))
        {
            revertPacket.Write(state.ChunkPosition);
            state.SerializeChangesToRevertPacket(revertPacket);
        }
    }
    private void SerializeRevertLights(Packet revertPacket)
    {
        revertPacket.Write(_lightUpdates.Count);
        foreach (var lightUpdate in _lightUpdates)
        {
            revertPacket.Write(lightUpdate.Key);
            revertPacket.Write(_lights[lightUpdate.Key]);
        }
    }
    private void SerializeRevertPlayers(Packet revertPacket)
    {
        var count = _players.Values.Count(static state => state.IsDirty);
        revertPacket.Write(count);
        foreach (var entity in _players.Values.Where(static state => state.IsDirty))
        {
            revertPacket.Write(entity.EntityId);
            entity.SerializeChangesToRevertPacket(revertPacket);
        }
    }
    private void SerializeRevertEntity(Packet revertPacket)
    {
        revertPacket.Write(_newEntities.Count);
        foreach (var newEntity in _newEntities)
        {
            revertPacket.Write(newEntity);
            revertPacket.Write((byte)EntityType.Null);
        }
        revertPacket.Write(_removedEntities.Count);
        foreach (var removedEntity in _removedEntities)
        {
            revertPacket.Write(removedEntity.Key);
            revertPacket.Write((byte)removedEntity.Value.Key);
            switch (removedEntity.Value.Value)
            {
                case EntityType.Player:
                    ((PlayerState)removedEntity.Value.Value).Serialize(revertPacket);
                    break;
                case EntityType.Zombie:
                    throw new NotImplementedException();
                case EntityType.BlockBreakParticle:
                    throw new NotImplementedException();
                case EntityType.Null:
                    throw new ArgumentOutOfRangeException(nameof(removedEntity.Value.Key), removedEntity.Value.Key, "Invalid Entity Type");
            }
        }
    }
    private void DeserializeChangesChunks(Packet changePacket)
    {
        changePacket.Read(out int chunkCount);
        for (var i = 0; i < chunkCount; i++)
        {
            changePacket.Read(out Vector3i chunkPosition);
            _chunks[chunkPosition].DeserializeChanges(changePacket);
        }
    }
    private void DeserializeChangesLights(Packet changePacket)
    {
        changePacket.Read(out int lightCount);
        for (var i = 0; i < lightCount; i++)
        {
            changePacket.Read(out Vector2i lightPosition);
            changePacket.Read(out byte lightLevel);
            _lights[lightPosition] = lightLevel;
        }
    }
    private void DeserializeChangesPlayers(Packet changePacket)
    {
        changePacket.Read(out int playerCount);
        for (var i = 0; i < playerCount; i++)
        {
            changePacket.Read(out ushort playerId);
            _players[playerId].DeserializeChanges(changePacket);
        }
    }
    private void DeserializeChangesEntityLeave(Packet changePacket)
    {
        changePacket.Read(out int removedEntityCount);
        for (var i = 0; i < removedEntityCount; i++)
        {
            changePacket.Read(out ushort entityId);
            changePacket.Read(out byte entityType);
            switch ((EntityType)entityType)
            {
                case EntityType.Null:
                    switch (_entityIdToType[entityId])
                    {
                        case EntityType.Null:
                            throw new ArgumentOutOfRangeException(nameof(entityId), entityId, "Invalid Entity Type");
                        case EntityType.Player:
                            UnregisterPlayer(entityId);
                            _entityIdToType[entityId] = EntityType.Null;
                            break;
                        case EntityType.Zombie:
                            throw new NotImplementedException();
                        case EntityType.BlockBreakParticle:
                            throw new NotImplementedException();
                    }

                    break;
                case EntityType.Player:
                    RegisterPlayer(new PlayerState(entityId));
                    _entityIdToType[entityId] = EntityType.Player;
                    break;
                case EntityType.Zombie:
                    throw new NotImplementedException();
                case EntityType.BlockBreakParticle:
                    throw new NotImplementedException();
            }
        }
    }
    private void DeserializeChangesEntityEnter(Packet changePacket)
    {
        changePacket.Read(out int newEntityCount);
        for (var i = 0; i < newEntityCount; i++)
        {
            changePacket.Read(out ushort entityId);
            changePacket.Read(out byte entityType);
            switch ((EntityType)entityType)
            {
                case EntityType.Null:
                    switch (_entityIdToType[entityId])
                    {
                        case EntityType.Null:
                            throw new ArgumentOutOfRangeException(nameof(entityId), entityId, "Invalid Entity Type");
                        case EntityType.Player:
                            UnregisterPlayer(entityId);
                            _entityIdToType[entityId] = EntityType.Null;
                            break;
                        case EntityType.Zombie:
                            throw new NotImplementedException();
                        case EntityType.BlockBreakParticle:
                            throw new NotImplementedException();
                    }

                    break;
                case EntityType.Player:
                    RegisterPlayer(new PlayerState(entityId));
                    _entityIdToType[entityId] = EntityType.Player;
                    break;
                case EntityType.Zombie:
                    throw new NotImplementedException();
                case EntityType.BlockBreakParticle:
                    throw new NotImplementedException();
            }
        }
    }
    public override void DeserializeChanges(Packet changePacket)
    {
        changePacket.Read(out WorldTime);
        changePacket.Read(out _prevRandomSeed);
        DeserializeChangesChunks(changePacket);
        DeserializeChangesLights(changePacket);
        DeserializeChangesPlayers(changePacket);
        DeserializeChangesEntityEnter(changePacket);
        DeserializeChangesEntityLeave(changePacket);
    }

    public override void FinalizeChanges()
    {
        base.FinalizeChanges();
        foreach (var chunk in _chunks.Values.Where(static chunk => chunk.IsDirty)) chunk.FinalizeChanges();
        foreach (var player in _players.Values.Where(static player => player.IsDirty)) player.FinalizeChanges();
        foreach (var blockParticle in _blockParticles.Values.Where(static particle => particle.IsDirty)) blockParticle.FinalizeChanges();
        _lightUpdates.Clear();
        _newEntities.Clear();
        _removedEntities.Clear();
    }

    public void AddChunk(Vector3i chunkPosition)
    {
        ChunkState chunk = new(chunkPosition);
        _chunks.Add(chunkPosition, chunk);
    }

    public PlayerState AddPlayer()
    {
        var result = _entityIdToType.First(GetNextEmptyEntityId);
        _entityIdToType[result.Key] = EntityType.Player;
        _newEntities.Add(result.Key);
        return new PlayerState(result.Key);
    }

    public BlockParticleEntityState AddBlockParticleEntity()
    {
        var result = _entityIdToType.First(GetNextEmptyEntityId);
        _entityIdToType[result.Key] = EntityType.BlockBreakParticle;
        _newEntities.Add(result.Key);
        return new BlockParticleEntityState(result.Key);
    }

    public void RemovePlayer(ushort entityId)
    {
        _removedEntities.Add(entityId, KeyValuePair.Create(EntityType.Player, (object)_players[entityId]));
        _entityIdToType[entityId] = EntityType.Null;
    }

    public void RemoveBlockParticle(ushort entityId)
    {
        _removedEntities.Add(entityId, KeyValuePair.Create(EntityType.BlockBreakParticle, (object)_blockParticles[entityId]));
        _entityIdToType[entityId] = EntityType.Null;
    }

    public void RegisterPlayer(PlayerState state)
    {
        _players.Add(state.EntityId, state);
    }

    public void RegisterBlockParticle(BlockParticleEntityState state)
    {
        _blockParticles.Add(state.EntityId, state);
    }

    public void UnregisterPlayer(ushort entityId)
    {
        _players.Remove(entityId);
    }
    
    public void UnregisterBlockParticle(ushort entityId)
    {
        _blockParticles.Remove(entityId);
    }

    private static bool GetNextEmptyEntityId(KeyValuePair<ushort, EntityType> entity)
    {
        return entity.Value == EntityType.Null;
    }

    public void AddLight(Vector2i lightPos)
    {
        _lights.Add(lightPos, 0);
    }

    public void SetLight(Vector2i lightPos, byte newValue)
    {
        _lightUpdates.TryAdd(lightPos, _lights[lightPos]);
        _lightUpdates[lightPos] = newValue;
    }

    public EntityType GetEntityType(ushort entityId)
    {
        return _entityIdToType[entityId];
    }

    public ChunkState GetChunkAt(Vector3i chunkPosition)
    {
        return _chunks[chunkPosition];
    }

    public void SaveWorld(GZipStream stream)
    {
        stream.Write(BitConverter.GetBytes(_chunks.Count));
        var packet = new Packet(new PacketHeader(PacketType.SaveWorld));
        foreach (var chunk in _chunks.Values) chunk.Serialize(packet);
        stream.Write(packet.ReadAll());
    }

    public void LoadWorld(GZipStream stream)
    {
        var packet = new Packet(new PacketHeader(PacketType.LoadWorld));
        var b = stream.ReadByte();
        do
        {
            packet.Write(b);
            b = stream.ReadByte();
        } while (b != -1);

        packet.Read(out int chunkCount);
        for (var i = 0; i < chunkCount; i++)
        {
            packet.Read(out Vector3i chunkPosition);
            _chunks[chunkPosition].Deserialize(packet);
        }
    }

    public byte GetLight(Vector2i pos)
    {
        return _lights[pos];
    }
}