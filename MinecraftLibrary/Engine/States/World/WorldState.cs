using System.IO.Compression;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.States.World;

public sealed class WorldState : State<WorldState>
{
    private readonly Dictionary<Vector3i, ChunkState> _chunks = new();
    private readonly Dictionary<Vector2i, byte> _lights = new();
    private readonly SortedDictionary<ushort, PlayerState> _players = new();
    private readonly SortedDictionary<ushort, BlockParticleEntityState> _blockParticles = new();
    private readonly Dictionary<ushort, EntityType> _entityIdToType = new();
    private readonly HashSet<Vector3i> _chunkUpdates = new();
    private readonly Dictionary<Vector2i, byte> _lightUpdates = new();
    private readonly HashSet<ushort> _newEntities = new();
    private readonly Dictionary<ushort, KeyValuePair<EntityType, object>> _removedEntities = new();
    private readonly HashSet<ushort> _entityUpdates = new();
    private ulong _worldTime = 0;
    public Random Random { get; }

    public long Seed { get; }

    public ulong WorldTime
    {
        get => _worldTime;
        set
        {
            Changes.TryAdd((ushort)StateChange.WorldTime, _worldTime);
            _worldTime = value;
        }
    }

    public WorldState(long seed)
    {
        Seed = seed;
        Random = new Random(seed);
        Random.OnRandomUpdate += OnRandomUpdate;
        _entityIdToType.EnsureCapacity(ushort.MaxValue);
        for (var i = ushort.MaxValue; i > 0; i++)
            _entityIdToType.Add(i, EntityType.Null);
        _entityIdToType.Add(0, EntityType.Null);
    }

    public WorldState()
    {
        Seed = -1;
        Random = new Random();
        Random.OnRandomUpdate += OnRandomUpdate;
        _entityIdToType.EnsureCapacity(ushort.MaxValue);
        for (var i = ushort.MaxValue; i > 0; i++)
            _entityIdToType.Add(i, EntityType.Null);
        _entityIdToType.Add(0, EntityType.Null);
    }

    public override void Serialize(Packet packet)
    {
        packet.Write(_worldTime);
        packet.Write(Random.GetSeed());
        packet.Write(_chunks.Count);
        foreach (var chunk in _chunks.Values)
            chunk.Serialize(packet);
        packet.Write(_lights.Count);
        foreach (var light in _lights)
        {
            packet.Write(light.Key);
            packet.Write(light.Value);
        }

        packet.Write(_players.Count);
        foreach (var player in _players.Values)
            player.Serialize(packet);
    }

    public override void Deserialize(Packet packet)
    {
        packet.Read(out _worldTime);
        packet.Read(out long seed);
        Random.SetSeed(seed);
        packet.Read(out int chunkCount);
        _chunks.EnsureCapacity(chunkCount);
        for (var i = 0; i < chunkCount; i++)
        {
            packet.Read(out Vector3i chunkPosition);
            var chunk = new ChunkState(chunkPosition);
            chunk.Deserialize(packet);
            _chunks.Add(chunkPosition, chunk);
        }

        packet.Read(out int lightCount);
        _lights.EnsureCapacity(lightCount);
        for (var i = 0; i < lightCount; i++)
        {
            packet.Read(out Vector2i lightPosition);
            packet.Read(out byte lightLevel);
            _lights.Add(lightPosition, lightLevel);
        }

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

    protected override void SerializeChangeToChangePacket(Packet changePacket, ushort change)
    {
        switch ((StateChange)change)
        {
            case StateChange.WorldTime:
                changePacket.Write(_worldTime);
                break;
            case StateChange.WorldChunk:
                changePacket.Write(_chunkUpdates.Count);
                foreach (var chunkUpdate in _chunkUpdates)
                {
                    changePacket.Write(chunkUpdate);
                    _chunks[chunkUpdate].SerializeChangesToChangePacket(changePacket);
                }

                break;
            case StateChange.WorldLight:
                changePacket.Write(_lightUpdates.Count);
                foreach (var lightUpdate in _lightUpdates.Keys)
                {
                    changePacket.Write(lightUpdate);
                    changePacket.Write(_lights[lightUpdate]);
                }

                break;
            case StateChange.WorldEntity:
                changePacket.Write(_entityUpdates.Count);
                foreach (var entityId in _entityUpdates)
                {
                    changePacket.Write(entityId);
                    switch (_entityIdToType[entityId])
                    {
                        case EntityType.Null:
                            throw new ArgumentOutOfRangeException(nameof(entityId), entityId, "Invalid Entity Type");
                        case EntityType.Player:
                            _players[entityId].SerializeChangesToChangePacket(changePacket);
                            break;
                        case EntityType.Zombie:
                            throw new NotImplementedException();
                        case EntityType.BlockBreakParticle:
                            throw new NotImplementedException();
                    }
                }

                break;
            case StateChange.WorldEntityEnter:
                changePacket.Write(_newEntities.Count);
                foreach (var newEntity in _newEntities)
                {
                    changePacket.Write(newEntity);
                    changePacket.Write((byte)_entityIdToType[newEntity]);
                }

                break;
            case StateChange.WorldEntityLeave:
                changePacket.Write(_removedEntities.Count);
                foreach (var removedEntity in _removedEntities.Keys)
                {
                    changePacket.Write(removedEntity);
                    changePacket.Write((byte)EntityType.Null);
                }

                break;
        }
    }

    protected override void SerializeChangeToRevertPacket(Packet revertPacket, ushort change)
    {
        switch ((StateChange)change)
        {
            case StateChange.WorldTime:
                revertPacket.Write((ulong)Changes[change]);
                break;
            case StateChange.WorldChunk:
                revertPacket.Write(_chunkUpdates.Count);
                foreach (var chunkUpdate in _chunkUpdates)
                {
                    revertPacket.Write(chunkUpdate);
                    _chunks[chunkUpdate].SerializeChangesToRevertPacket(revertPacket);
                }

                break;
            case StateChange.WorldLight:
                revertPacket.Write(_lightUpdates.Count);
                foreach (var lightUpdate in _lightUpdates)
                {
                    revertPacket.Write(lightUpdate.Key);
                    revertPacket.Write(lightUpdate.Value);
                }

                break;
            case StateChange.WorldEntity:
                revertPacket.Write(_entityUpdates.Count);
                foreach (var entityId in _entityUpdates)
                {
                    revertPacket.Write(entityId);
                    switch (_entityIdToType[entityId])
                    {
                        case EntityType.Null:
                            throw new ArgumentOutOfRangeException(nameof(entityId), entityId, "Invalid Entity Type");
                        case EntityType.Player:
                            _players[entityId].SerializeChangesToRevertPacket(revertPacket);
                            break;
                        case EntityType.Zombie:
                            throw new NotImplementedException();
                        case EntityType.BlockBreakParticle:
                            throw new NotImplementedException();
                    }
                }

                break;
            case StateChange.WorldEntityEnter:
                revertPacket.Write(_newEntities.Count);
                foreach (var newEntity in _newEntities)
                {
                    revertPacket.Write(newEntity);
                    revertPacket.Write((byte)EntityType.Null);
                }

                break;
            case StateChange.WorldEntityLeave:
                revertPacket.Write(_removedEntities.Count);
                foreach (var removedEntity in _removedEntities)
                {
                    revertPacket.Write(removedEntity.Key);
                    revertPacket.Write((byte)removedEntity.Value.Key);
                    switch (removedEntity.Value.Key)
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

                break;
        }
    }

    protected override void DeserializeChange(Packet changePacket, ushort change)
    {
        switch ((StateChange)change)
        {
            case StateChange.WorldTime:
                changePacket.Read(out _worldTime);
                break;
            case StateChange.WorldChunk:
                changePacket.Read(out int size);
                for (var i = 0; i < size; i++)
                {
                    changePacket.Read(out Vector3i chunkPosition);
                    _chunks[chunkPosition].DeserializeChanges(changePacket);
                }

                break;
            case StateChange.WorldLight:
                changePacket.Read(out size);
                for (var i = 0; i < size; i++)
                {
                    changePacket.Read(out Vector2i lightPosition);
                    changePacket.Read(out byte lightLevel);
                    _lights[lightPosition] = lightLevel;
                }

                break;
            case StateChange.WorldEntity:
                changePacket.Read(out size);
                for (var i = 0; i < size; i++)
                {
                    changePacket.Read(out ushort entityId);
                    switch (_entityIdToType[entityId])
                    {
                        case EntityType.Null:
                            throw new ArgumentOutOfRangeException(nameof(entityId), entityId, "Invalid Entity Type");
                        case EntityType.Player:
                            _players[entityId].DeserializeChanges(changePacket);
                            break;
                        case EntityType.Zombie:
                            throw new NotImplementedException();
                        case EntityType.BlockBreakParticle:
                            throw new NotImplementedException();
                    }
                }

                break;
            case StateChange.WorldEntityEnter:
                changePacket.Read(out size);
                for (var i = 0; i < size; i++)
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

                break;
            case StateChange.WorldEntityLeave:
                changePacket.Read(out size);
                for (var i = 0; i < size; i++)
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
                            _entityIdToType[entityId] = EntityType.Player;
                            RegisterPlayer(new PlayerState(entityId));
                            _players[entityId].Deserialize(changePacket);
                            break;
                        case EntityType.Zombie:
                            throw new NotImplementedException();
                        case EntityType.BlockBreakParticle:
                            throw new NotImplementedException();
                    }
                }

                break;
        }
    }

    public override void FinalizeChanges()
    {
        base.FinalizeChanges();
        _chunkUpdates.Clear();
        _lightUpdates.Clear();
        _newEntities.Clear();
        _removedEntities.Clear();
        _entityUpdates.Clear();
    }

    public void AddChunk(Vector3i chunkPosition)
    {
        ChunkState chunk = new(chunkPosition);
        _chunks.Add(chunkPosition, chunk);
        chunk.OnChunkUpdate += OnChunkUpdate;
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
        state.OnEntityUpdate += OnEntityUpdate;
        _players.Add(state.EntityId, state);
    }

    public void RegisterBlockParticle(BlockParticleEntityState state)
    {
        state.OnEntityUpdate += OnEntityUpdate;
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

    private void OnEntityUpdate(ushort entityId)
    {
        _entityUpdates.Add(entityId);
    }

    public void AddLight(Vector2i lightPos)
    {
        _lights.Add(lightPos, 0);
    }

    private void OnChunkUpdate(Vector3i chunkPosition, ushort change, BlockType blockType)
    {
        _chunkUpdates.Add(chunkPosition);
        RecalculateLight(chunkPosition + EngineDefaults.GetVectorFromIndex(change), blockType);
    }

    private void RecalculateLight(Vector3i blockPlaced, BlockType type)
    {
        var currentLight = _lights[blockPlaced.Xz];
        if (blockPlaced.Y < currentLight) return;

        OnLightUpdate(blockPlaced.Xz);
        if (EngineDefaults.Blocks[(int)type].IsBlockingLight())
        {
            _lights[blockPlaced.Xz] = (byte)blockPlaced.Y;
        }
        else
        {
            for (currentLight -= 1; currentLight > 0; currentLight--)
                if (Engine.World.GetInstance()!.GetBlockAt(new Vector3i(blockPlaced.X, currentLight, blockPlaced.Z))
                    .IsBlockingLight())
                {
                    _lights[blockPlaced.Xz] = currentLight;
                    return;
                }

            _lights[blockPlaced.Xz] = 0;
        }
    }

    private void OnLightUpdate(Vector2i lightPosition)
    {
        _lightUpdates[lightPosition] = _lights[lightPosition];
    }

    private void OnRandomUpdate()
    {
        Changes.TryAdd((ushort)StateChange.WorldRandomSeed, Random.GetSeed());
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