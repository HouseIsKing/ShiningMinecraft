using System.IO.Compression;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.States.World;

public sealed class WorldState : State<WorldState>
{
    private Vector3i _baseVector;
    private readonly ChunkState[,,] _chunks;
    private ushort _dirtyChunksCount;
    private readonly LightChunkState[,] _lights;
    private ushort _dirtyLightsCount;
    private readonly Dictionary<ushort, object> _entities = new();
    private ushort _dirtyEntitiesCount;
    private readonly Dictionary<ushort, EntityType> _entityIdToType = new();
    private readonly HashSet<ushort> _newEntities = new();
    private readonly Dictionary<ushort, KeyValuePair<EntityType, object>> _removedEntities = new();
    internal Random Random { get; }
    public ulong WorldTime;
    private long _prevRandomSeed;

    public long Seed { get; }

    public WorldState(long seed, ushort maxX, ushort maxY, ushort maxZ)
    {
        _chunks = new ChunkState[maxX / EngineDefaults.ChunkWidth, maxY / EngineDefaults.ChunkHeight, maxZ / EngineDefaults.ChunkDepth];
        _lights = new LightChunkState[maxX, maxZ];
        Seed = seed;
        Random = new Random(seed);
        _entityIdToType.EnsureCapacity(ushort.MaxValue);
        for (var i = ushort.MaxValue; i > 0; i--) _entityIdToType.Add(i, EntityType.Null);
        _entityIdToType.Add(0, EntityType.Null);
    }

    public WorldState(ushort maxX, ushort maxY, ushort maxZ)
    {
        _chunks = new ChunkState[maxX / EngineDefaults.ChunkWidth, maxY / EngineDefaults.ChunkHeight, maxZ / EngineDefaults.ChunkDepth];
        _lights = new LightChunkState[maxX, maxZ];
        Seed = -1;
        Random = new Random();
        _entityIdToType.EnsureCapacity(ushort.MaxValue);
        for (var i = ushort.MaxValue; i > 0; i--) _entityIdToType.Add(i, EntityType.Null);
        _entityIdToType.Add(0, EntityType.Null);
    }

    private void SerializeChunks(Packet packet)
    {
        packet.Write(_chunks.GetLength(0));
        packet.Write(_chunks.GetLength(1));
        packet.Write(_chunks.GetLength(2));
        foreach (var chunk in _chunks) chunk.Serialize(packet);
    }
    private void SerializeLights(Packet packet)
    {
        packet.Write(_lights.GetLength(0));
        packet.Write(_lights.GetLength(1));
        foreach (var light in _lights) light.Serialize(packet);
    }
    private void SerializeEntities(Packet packet)
    {
        packet.Write(_entities.Count);
        foreach (var entity in _entities)
            switch (_entityIdToType[entity.Key])
            {
                default:
                    throw new Exception("Invalid Entity Type");
                case EntityType.Player:
                    ((PlayerState)entity.Value).Serialize(packet);
                    break;
                case EntityType.Zombie:
                    throw new NotImplementedException();
                case EntityType.BlockBreakParticle:
                    throw new NotImplementedException();
            }
    }
    public override void Serialize(Packet packet)
    {
        packet.Write(WorldTime);
        packet.Write(Random.GetSeed());
        SerializeChunks(packet);
        SerializeLights(packet);
        SerializeEntities(packet);
    }
    public override void Deserialize(Packet packet)
    {
        packet.Read(out WorldTime);
        packet.Read(out long seed);
        Random.SetSeed(seed);
        DeserializeChunks(packet);
        DeserializeLights(packet);
        DeserializeEntities(packet);
    }
    private void DeserializeChunks(Packet packet)
    {
        packet.Read(out int i);
        packet.Read(out int j);
        packet.Read(out int k);
        for (var a = 0; a < i; a++)
        for (var b = 0; b < j; b++)
        for (var c = 0; c < k; c++)
        {
            packet.Read(out Vector3i chunkPosition);
            var chunk = new ChunkState(chunkPosition);
            chunk.OnChange += ChunkChanged;
            chunk.Deserialize(packet);
            _chunks[a, b, c] = chunk;
        }
    }
    private void DeserializeLights(Packet packet)
    {
        packet.Read(out int i);
        packet.Read(out int j);
        for (var a = 0; a < i; a++)
        for (var b = 0; b < j; b++)
        {
            packet.Read(out Vector2i lightPosition);
            var light = new LightChunkState(lightPosition);
            light.OnChange += LightChanged;
            light.Deserialize(packet);
            _lights[a, b] = light;
        }
    }
    private void DeserializeEntities(Packet packet)
    {
        packet.Read(out int entityCount);
        for (var i = 0; i < entityCount; i++)
        {
            packet.Read(out ushort entityId);
            packet.Read(out byte entityType);
            _entityIdToType[entityId] = (EntityType)entityType;
            switch ((EntityType)entityType)
            {
                case EntityType.Player:
                    var player = new PlayerState(entityId);
                    player.OnChange += EntityChanged;
                    player.Deserialize(packet);
                    RegisterPlayer(player);
                    break;
                case EntityType.Zombie:
                    throw new NotImplementedException();
                case EntityType.BlockBreakParticle:
                    throw new NotImplementedException();
                default:
                    throw new Exception("Invalid Entity Type");
            }
        }
    }
    public override void SerializeChangesToChangePacket(Packet changePacket)
    {
        base.SerializeChangesToChangePacket(changePacket);
        changePacket.Write(WorldTime);
        changePacket.Write(Random.GetSeed());
        SerializeChangesChunks(changePacket);
        SerializeChangesLights(changePacket);
        SerializeChangesEntities(changePacket);
        SerializeChangesEntityAddition(changePacket);
    }
    private void SerializeChangesChunks(Packet changePacket)
    {
        changePacket.Write(_dirtyChunksCount);
        for (byte i = 0; i < _chunks.GetLength(0); i++)
        for (byte j = 0; j < _chunks.GetLength(1); j++)
        for (byte k = 0; k < _chunks.GetLength(2); k++)
            if (_chunks[i, j, k].IsDirty)
            {
                changePacket.Write(i);
                changePacket.Write(j);
                changePacket.Write(k);
                _chunks[i, j, k].SerializeChangesToChangePacket(changePacket);
            }

        _dirtyChunksCount = 0;
    }
    private void SerializeChangesLights(Packet changePacket)
    {
        changePacket.Write(_dirtyLightsCount);
        for (ushort i = 0; i < _lights.GetLength(0); i++)
        for (ushort j = 0; j < _lights.GetLength(1); j++)
            if (_lights[i, j].IsDirty)
            {
                changePacket.Write(i);
                changePacket.Write(j);
                _lights[i, j].SerializeChangesToChangePacket(changePacket);
            }

        _dirtyLightsCount = 0;
    }
    private void SerializeChangesEntities(Packet changePacket)
    {
        changePacket.Write(_dirtyEntitiesCount);
        foreach (var entity in _entities)
            switch (_entityIdToType[entity.Key])
            {
                default:
                    throw new Exception("Invalid Entity Type");
                case EntityType.Player:
                    var player = (PlayerState)entity.Value;
                    if (player.IsDirty)
                    {
                        changePacket.Write(player.EntityId);
                        player.SerializeChangesToChangePacket(changePacket);
                    }
                    break;
                case EntityType.Zombie:
                    throw new NotImplementedException();
                case EntityType.BlockBreakParticle:
                    throw new NotImplementedException();
            }

        _dirtyEntitiesCount = 0;
    }
    private void SerializeChangesEntityAddition(Packet changePacket)
    {
        changePacket.Write(_newEntities.Count);
        foreach (var newEntity in _newEntities)
        {
            changePacket.Write(newEntity);
            changePacket.Write((byte)_entityIdToType[newEntity]);
        }
        _newEntities.Clear();
        changePacket.Write(_removedEntities.Count);
        foreach (var removedEntity in _removedEntities)
        {
            changePacket.Write(removedEntity.Key);
            changePacket.Write((byte)EntityType.Null);
        }
        _removedEntities.Clear();
    }
    public override void SerializeChangesToRevertPacket(Packet revertPacket)
    {
        base.SerializeChangesToRevertPacket(revertPacket);
        revertPacket.Write(WorldTime - 1);
        revertPacket.Write(_prevRandomSeed);
        SerializeRevertChunks(revertPacket);
        SerializeRevertLights(revertPacket);
        SerializeRevertEntities(revertPacket);
        SerializeRevertEntityAddition(revertPacket);
    }
    private void SerializeRevertChunks(Packet revertPacket)
    {
        revertPacket.Write(_dirtyChunksCount);
        for (byte i = 0; i < _chunks.GetLength(0); i++)
        for (byte j = 0; j < _chunks.GetLength(1); j++)
        for (byte k = 0; k < _chunks.GetLength(2); k++)
            if (_chunks[i, j, k].IsDirty)
            {
                revertPacket.Write(i);
                revertPacket.Write(j);
                revertPacket.Write(k);
                _chunks[i, j, k].SerializeChangesToRevertPacket(revertPacket);
            }

        _dirtyChunksCount = 0;
    }
    private void SerializeRevertLights(Packet revertPacket)
    {
        revertPacket.Write(_dirtyLightsCount);
        for (ushort i = 0; i < _lights.GetLength(0); i++)
        for (ushort j = 0; j < _lights.GetLength(1); j++)
            if (_lights[i, j].IsDirty)
            {
                revertPacket.Write(i);
                revertPacket.Write(j);
                _lights[i, j].SerializeChangesToRevertPacket(revertPacket);
            }

        _dirtyLightsCount = 0;
    }
    private void SerializeRevertEntities(Packet revertPacket)
    {
        revertPacket.Write(_dirtyEntitiesCount);
        foreach (var entity in _entities)
            switch (_entityIdToType[entity.Key])
            {
                default:
                    throw new Exception("Invalid Entity Type");
                case EntityType.Player:
                    var player = (PlayerState)entity.Value;
                    if (player.IsDirty)
                    {
                        revertPacket.Write(player.EntityId);
                        player.SerializeChangesToRevertPacket(revertPacket);
                    }
                    break;
                case EntityType.Zombie:
                    throw new NotImplementedException();
                case EntityType.BlockBreakParticle:
                    throw new NotImplementedException();
            }

        _dirtyEntitiesCount = 0;
    }
    private void SerializeRevertEntityAddition(Packet revertPacket)
    {
        revertPacket.Write(_newEntities.Count);
        foreach (var newEntity in _newEntities)
        {
            revertPacket.Write(newEntity);
            revertPacket.Write((byte)EntityType.Null);
        }
        _newEntities.Clear();
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
        _removedEntities.Clear();
    }
    private void DeserializeChangesChunks(Packet changePacket)
    {
        changePacket.Read(out ushort chunkCount);
        for (var i = 0; i < chunkCount; i++)
        {
            changePacket.Read(out byte a);
            changePacket.Read(out byte b);
            changePacket.Read(out byte c);
            _chunks[a, b, c].DeserializeChanges(changePacket);
        }
    }
    private void DeserializeChangesLights(Packet changePacket)
    {
        changePacket.Read(out ushort lightCount);
        for (var i = 0; i < lightCount; i++)
        {
            changePacket.Read(out ushort a);
            changePacket.Read(out ushort b);
            _lights[a, b].DeserializeChanges(changePacket);
        }
    }
    private void DeserializeChangesEntities(Packet changePacket)
    {
        changePacket.Read(out ushort entityCount);
        for (var i = 0; i < entityCount; i++)
        {
            changePacket.Read(out ushort entityId);
            switch (_entityIdToType[entityId])
            {
                default:
                    throw new Exception("Invalid Entity Type");
                case EntityType.Player:
                    ((PlayerState)_entities[entityId]).DeserializeChanges(changePacket);
                    break;
                case EntityType.Zombie:
                    throw new NotImplementedException();
                case EntityType.BlockBreakParticle:
                    throw new NotImplementedException();
            }
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
                    UnregisterEntity(entityId);
                    _entityIdToType[entityId] = EntityType.Null;
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
                    UnregisterEntity(entityId);
                    _entityIdToType[entityId] = EntityType.Null;
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
        DeserializeChangesEntities(changePacket);
        DeserializeChangesEntityEnter(changePacket);
        DeserializeChangesEntityLeave(changePacket);
    }

    public override void DiscardChanges()
    {
        base.DiscardChanges();
        _dirtyChunksCount = 0;
        _dirtyLightsCount = 0;
        _dirtyEntitiesCount = 0;
        foreach (var chunk in _chunks) chunk.DiscardChanges();
        foreach (var light in _lights) light.DiscardChanges();
        foreach (var entity in _entities)
            switch (_entityIdToType[entity.Key])
            {
                default:
                    throw new Exception("Invalid Entity Type");
                case EntityType.Player:
                    ((PlayerState)entity.Value).DiscardChanges();
                    break;
                case EntityType.Zombie:
                    throw new NotImplementedException();
                case EntityType.BlockBreakParticle:
                    throw new NotImplementedException();
            }
        _removedEntities.Clear();
        _newEntities.Clear();
    }

    public void AddChunk(Vector3i chunkPosition)
    {
        ChunkState chunk = new(chunkPosition);
        chunk.OnChange += ChunkChanged;
        var pos = chunkPosition - _baseVector;
        _chunks[pos.X / EngineDefaults.ChunkWidth, pos.Y / EngineDefaults.ChunkHeight, pos.Z / EngineDefaults.ChunkDepth] = chunk;
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

    public void RemoveEntity(ushort entityId)
    {
        _removedEntities.Add(entityId, KeyValuePair.Create(_entityIdToType[entityId], _entities[entityId]));
        _entityIdToType[entityId] = EntityType.Null;
    }

    public void RegisterPlayer(PlayerState state)
    {
        state.DidSpawn = true;
        state.OnChange += EntityChanged;
        _entities.Add(state.EntityId, state);
    }

    public void RegisterBlockParticle(BlockParticleEntityState state)
    {
        state.DidSpawn = true;
        state.OnChange += EntityChanged;
        _entities.Add(state.EntityId, state);
    }

    public void UnregisterEntity(ushort entityId)
    {
        _entities.Remove(entityId);
    }

    private static bool GetNextEmptyEntityId(KeyValuePair<ushort, EntityType> entity)
    {
        return entity.Value == EntityType.Null;
    }

    public void AddLight(Vector2i lightPos)
    {
        var pos = lightPos - _baseVector.Xz;
        _lights[pos.X, pos.Y] = new LightChunkState(lightPos);
        _lights[pos.X, pos.Y].OnChange += LightChanged;
    }

    public void SetLight(Vector2i lightPos, byte newValue)
    {
        var pos = lightPos - _baseVector.Xz;
        _lights[pos.X, pos.Y].LightLevel = newValue;
    }

    public EntityType GetEntityType(ushort entityId)
    {
        return _entityIdToType[entityId];
    }

    public ChunkState GetChunkAt(Vector3i chunkPosition)
    {
        var pos = chunkPosition - _baseVector;
        return _chunks[pos.X / EngineDefaults.ChunkWidth, pos.Y / EngineDefaults.ChunkHeight, pos.Z / EngineDefaults.ChunkDepth];
    }

    public void SaveWorld(GZipStream stream)
    {
        stream.Write(BitConverter.GetBytes(_chunks.GetLength(0)));
        stream.Write(BitConverter.GetBytes(_chunks.GetLength(1)));
        stream.Write(BitConverter.GetBytes(_chunks.GetLength(2)));
        var packet = new Packet(new PacketHeader(PacketType.SaveWorld));
        foreach (var chunk in _chunks) chunk.Serialize(packet);
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
        
        packet.Read(out int i);
        packet.Read(out int j);
        packet.Read(out int k);
        for (var a = 0; a < i; a++)
        for (b = 0; b < j; b++)
        for (var c = 0; c < k; c++)
        {
            packet.Read(out Vector3i chunkPosition);
            var chunk = new ChunkState(chunkPosition);
            chunk.OnChange += ChunkChanged;
            chunk.Deserialize(packet);
            _chunks[a, b, c] = chunk;
        }
    }

    public byte GetLight(Vector2i pos)
    {
        var finalPos = pos - _baseVector.Xz;
        return _lights[finalPos.X, finalPos.Y].LightLevel;
    }

    private void ChunkChanged()
    {
        _dirtyChunksCount++;
    }
    
    private void LightChanged()
    {
        _dirtyLightsCount++;
    }

    private void EntityChanged()
    {
        _dirtyEntitiesCount++;
    }
    
    public Vector3i GetBaseVector()
    {
        return _baseVector;
    }
}