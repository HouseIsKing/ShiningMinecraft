using System.IO.Compression;
using MinecraftLibrary.Engine.Blocks;
using MinecraftLibrary.Engine.Entities;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Engine.States.World;
using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine;

public sealed class World
{
    public static World Instance { get; private set; }
    
    public const int WorldWidth = 256;
    public const int WorldHeight = 64;
    public const int WorldDepth = 256;
    private readonly SortedList<ushort, Player> _players = new();
    private readonly SortedList<ushort, BlockParticleEntity> _blockParticles = new();
    private readonly List<PlayerState> _playersToSpawn = new();
    private readonly List<BlockParticleEntityState> _blockParticlesToSpawn = new();
    private readonly List<ushort> _playersToDespawn = new();
    private readonly List<ushort> _blockParticlesToDespawn = new();
    private readonly List<Box3> _blocksCollidingBuffer = new();
    public event EngineDefaults.EntityAddedHandler OnPlayerAdded;
    public event EngineDefaults.TickStartHandler OnTickStart;
    public event EngineDefaults.TickEndHandler OnTickEnd;
    private WorldState State { get; }

    public World(long seed)
    {
        if (Instance != null) throw new Exception("World already exists");

        State = new WorldState(seed, WorldWidth, WorldHeight, WorldDepth);
        Instance = this;
    }

    public World()
    {
        if (Instance != null) throw new Exception("World already exists");

        State = new WorldState(WorldWidth, WorldHeight, WorldDepth);
        Instance = this;
        State.EntityAdded += EntityAddedThroughState;
        State.EntityRemoved += EntityRemovedThroughState;
    }

    ~World()
    {
        Instance = null;
    }

    private void EntityAddedThroughState(ushort entityId)
    {
        switch (State.GetEntityType(entityId))
        {
            case EntityType.Player:
                _players.Add(entityId, new Player(State.GetEntity<PlayerState>(entityId)));
                OnPlayerAdded?.Invoke(entityId);
                break;
            case EntityType.Zombie:
                throw new NotImplementedException();
            case EntityType.BlockBreakParticle:
                throw new NotImplementedException();
            case EntityType.Null:
                throw new NotImplementedException();
        }
    }

    private void EntityRemovedThroughState(ushort entityId)
    {
        switch (State.GetEntityType(entityId))
        {
            case EntityType.Player:
                _players.Remove(entityId);
                break;
            case EntityType.Zombie:
                throw new NotImplementedException();
            case EntityType.BlockBreakParticle:
                throw new NotImplementedException();
            case EntityType.Null:
                throw new NotImplementedException();
        }
    }

    private void PreTick()
    {
        foreach (var state in _playersToSpawn)
        {
            state.DiscardChanges();
            State.RegisterPlayer(state);
            _players.Add(state.EntityId, new Player(state));
            OnPlayerAdded?.Invoke(state.EntityId);
        }

        foreach (var state in _blockParticlesToSpawn)
        {
            state.DiscardChanges();
            State.RegisterBlockParticle(state);
            _blockParticles.Add(state.EntityId, new BlockParticleEntity(state));
        }

        _blockParticlesToSpawn.Clear();
        _playersToSpawn.Clear();
    }

    private void TickEntities()
    {
        foreach (var player in _players.Values) player.Tick();
        foreach (var blockParticle in _blockParticles.Values) blockParticle.Tick();
    }

    private void TickWorld()
    {
        const int numTilesToTick = WorldWidth * WorldHeight * WorldDepth / 400;
        var random = State.Random;
        for (var i = 0; i < numTilesToTick; i++)
        {
            Vector3i pos = new(random.NextInt(WorldWidth), random.NextInt(WorldHeight), random.NextInt(WorldDepth));
            GetBlockAt(pos).Tick(this, pos);
        }
    }

    public void Tick(Packet packet, bool revertPacket)
    {
        packet.Reset();
        PreTick();
        OnTickStart?.Invoke();
        State.WorldTime += 1;
        TickEntities();
        TickWorld();
        PostTick();
        OnTickEnd?.Invoke();
        if (revertPacket)
            State.SerializeChangesToRevertPacket(packet);
        else
            State.SerializeChangesToChangePacket(packet);
    }

    public void SimulateTick(Packet packet, bool revertPacket)
    {
        packet.Reset();
        PreTick();
        State.WorldTime += 1;
        TickEntities();
        TickWorld();
        PostTick();
        if (revertPacket)
            State.SerializeChangesToRevertPacket(packet);
        else
            State.SerializeChangesToChangePacket(packet);
    }

    private void PostTick()
    {
        foreach (var id in _playersToDespawn)
        {
            State.UnregisterEntity(id);
            _players.Remove(id);
        }

        foreach (var id in _blockParticlesToDespawn)
        {
            State.UnregisterEntity(id);
            _blockParticles.Remove(id);
        }

        _playersToDespawn.Clear();
        _blockParticlesToDespawn.Clear();
    }

    public void SaveWorld()
    {
        GZipStream stream = new(File.Create("world.dat"), CompressionMode.Compress);
        State.SaveWorld(stream);
        stream.Close();
    }
    
    public void LoadWorld()
    {
        GZipStream stream = new(File.OpenRead("world.dat"), CompressionMode.Decompress);
        State.LoadWorld(stream);
        stream.Close();
    }

    public ChunkState GetChunkAt(Vector3i pos)
    {
        return State.GetChunkAt(pos);
    }

    public PlayerState SpawnPlayer()
    {
        var state = State.AddPlayer();
        state.Scale = EngineDefaults.PlayerSize;
        _playersToSpawn.Add(state);
        return state;
    }
    
    public BlockParticleEntityState SpawnBlockParticleEntity()
    {
        var state = State.AddBlockParticleEntity();
        _blockParticlesToSpawn.Add(state);
        return state;
    }

    public void DespawnPlayer(ushort id)
    {
        State.RemoveEntity(id);
        _playersToDespawn.Add(id);
    }

    public void DespawnBlockParticleEntity(ushort id)
    {
        State.RemoveEntity(id);
        _blockParticlesToDespawn.Add(id);
    }

    public Block GetBlockAt(Vector3i pos)
    {
        if (IsOutOfBounds(pos))
            return Block.GetBlock(BlockType.Air);

        var chunk = GetChunkAt(pos);
        return Block.GetBlock(chunk.GetBlockAt(EngineDefaults.GetIndexFromVector(pos)));
    }

    public List<Box3> GetBlocksColliding(Box3 collider)
    {
        _blocksCollidingBuffer.Clear();
        var min = new Vector3i((int)collider.Min.X, (int)collider.Min.Y, (int)collider.Min.Z);
        var max = new Vector3i((int)collider.Max.X + 1, (int)collider.Max.Y + 1, (int)collider.Max.Z + 1);
        if (collider.Min.X < 0) min.X -= 1;
        if (collider.Min.Y < 0) min.Y -= 1;
        if (collider.Min.Z < 0) min.Z -= 1;
        for (var x = min.X; x < max.X; x++)
        for (var y = min.Y; y < max.Y; y++)
        for (var z = min.Z; z < max.Z; z++)
        {
            var pos = new Vector3i(x, y, z);
            var block = GetBlockAt(pos);
            if (block.IsSolid()) _blocksCollidingBuffer.Add(new Box3(pos + block.BlockBounds.Min, pos + block.BlockBounds.Max));
        }

        return _blocksCollidingBuffer;
    }
    
    public void BreakBlock(Vector3i pos)
    {
        var block = GetBlockAt(pos);
        block.OnBreak(this, pos);
        SetBlockAt(pos, BlockType.Air);
    }

    public void SetBlockAt(Vector3i pos, BlockType block)
    {
        if (IsOutOfBounds(pos)) return;
        var chunk = GetChunkAt(pos);
        chunk.SetBlockAt(EngineDefaults.GetIndexFromVector(pos), block);
        RecalculateLight(pos, block);
    }

    public bool IsOutOfBounds(Vector3i pos)
    {
        var baseVector = State.BaseVector;
        return pos.X < baseVector.X || pos.Y < baseVector.Y || pos.Z < baseVector.Z || pos.X >= WorldWidth || pos.Y >= WorldHeight ||
               pos.Z >= WorldDepth;
    }

    public void GenerateLevel()
    {
        var heightMap1 = new PerlinNoise(0);
        var heightMap2 = new PerlinNoise(1);
        var firstHeightMap = heightMap1.Generate(WorldWidth, WorldDepth);
        var secondHeightMap = heightMap1.Generate(WorldWidth, WorldDepth);
        var cliffMap = heightMap2.Generate(WorldWidth, WorldDepth);
        var rockMap = heightMap2.Generate(WorldWidth, WorldDepth);
        var random = State.Random;

        for (var x = 0; x < WorldWidth; x++)
        for (var y = 0; y < WorldHeight; y++)
        for (var z = 0; z < WorldDepth; z++)
        {
            var loc = x + z * WorldWidth;
            var firstHeightValue = firstHeightMap[loc];
            var secondHeightValue = secondHeightMap[loc];
            if (cliffMap[loc] < 128) secondHeightValue = firstHeightValue;

            var maxLevelHeight = Math.Max(secondHeightValue, firstHeightValue) / 8 + WorldHeight / 3;
            var maxRockHeight = Math.Min(rockMap[loc] / 8 + WorldHeight / 3, maxLevelHeight - 2);
            var blockType = BlockType.Air;

            if (y == maxLevelHeight) blockType = BlockType.Grass;
            if (y < maxLevelHeight) blockType = BlockType.Dirt;
            if (y <= maxRockHeight) blockType = BlockType.Stone;
            SetBlockAt(new Vector3i(x, y, z), blockType);
        }

        const int count = WorldDepth * WorldHeight * WorldWidth / 256 / 64;
        for (var i = 0; i < count; i++)
        {
            var x = random.NextFloat() * WorldWidth;
            var y = random.NextFloat() * WorldHeight;
            var z = random.NextFloat() * WorldDepth;
            var length = (int)(random.NextFloat() + random.NextFloat() * 150.0f);
            var dir1 = random.NextFloat() * MathHelper.TwoPi;
            var dir1Change = 0.0f;
            var dir2 = random.NextFloat() * MathHelper.TwoPi;
            var dir2Change = 0.0f;
            for (var l = 0; l < length; l++)
            {
                x += MathF.Sin(dir1) * MathF.Cos(dir2);
                y += MathF.Sin(dir2);
                z += MathF.Cos(dir1) * MathF.Cos(dir2);
                dir1 += dir1Change * 0.2f;
                dir1Change *= 0.9f;
                dir1Change += random.NextFloat() - random.NextFloat();
                dir2 += dir2Change * 0.5f;
                dir2 *= 0.5f;
                dir2Change *= 0.9f;
                dir2Change += random.NextFloat() - random.NextFloat();
                var size = MathF.Sin(l * MathHelper.Pi / length) * 2.5f + 1.0f;
                for (var xx = (int)(x - size); xx < x + size; xx++)
                for (var yy = (int)(y - size); yy < y + size; yy++)
                for (var zz = (int)(z - size); zz < z + size; zz++)
                {
                    var dx = xx - x;
                    var dy = yy - y;
                    var dz = zz - z;
                    Vector3i pos = new(xx, yy, zz);
                    if (dx * dx + dy * dy * 2.0f + dz * dz < size * size && xx >= 1 && yy >= 1 && zz >= 1 && xx < WorldWidth - 1 && yy < WorldHeight - 1 && zz < WorldDepth - 1 && GetBlockAt(pos).Type == BlockType.Stone)
                        SetBlockAt(pos, BlockType.Air);
                }
            }
        }
        State.DiscardChanges();
    }
    
    private void RecalculateLight(Vector3i blockPlaced, BlockType type)
    {
        var currentLight = State.GetLight(blockPlaced.Xz);
        if (blockPlaced.Y < currentLight || blockPlaced.Y == 0) return;
        Block block = Block.GetBlock(type);
        if (blockPlaced.Y > currentLight)
        {
            if (block.IsBlockingLight())
                State.SetLight(blockPlaced.Xz, (byte)blockPlaced.Y);
            return;
        }
        if (block.IsBlockingLight()) return;
        for (currentLight -= 1; currentLight > 0; currentLight--)
            if (GetBlockAt(new Vector3i(blockPlaced.X, currentLight, blockPlaced.Z)).IsBlockingLight())
                break;
        State.SetLight(blockPlaced.Xz, currentLight);
    }

    public byte GetBrightnessAt(Vector3i pos)
    {
        if (IsOutOfBounds(pos)) return 0;
        return (byte)(GetLightAt(pos.Xz) < pos.Y ? 1 : 0);
    }

    public byte GetLightAt(Vector2i pos)
    {
        return State.GetLight(pos);
    }

    public Random GetWorldRandom()
    {
        return State.Random;
    }

    public ulong GetWorldTime()
    {
        return State.WorldTime;
    }
    
    public void Serialize(Packet packet)
    {
        State.Serialize(packet);
    }
    
    public void Deserialize(Packet packet)
    {
        State.Deserialize(packet);
    }
    
    public void DeserializeChanges(Packet packet)
    {
        State.DeserializeChanges(packet);
    }
    
    public void DiscardChanges()
    {
        State.DiscardChanges();
    }

    public Player GetPlayer(ushort id)
    {
        return _players[id];
    }
    
    public static ushort GetMaxChunksCount()
    {
        return WorldWidth / EngineDefaults.ChunkWidth * WorldHeight / EngineDefaults.ChunkHeight *
            WorldDepth / EngineDefaults.ChunkDepth;
    }
    
    public Vector3i GetBaseVector()
    {
        return State.BaseVector;
    }
    
    public void SerializeChangesToRevertPacket(Packet packet)
    {
        State.SerializeChangesToRevertPacket(packet);
    }
}