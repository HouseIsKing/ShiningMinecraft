using System.IO.Compression;
using MinecraftLibrary.Engine.Blocks;
using MinecraftLibrary.Engine.Entities;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Engine.States.World;
using MinecraftLibrary.Network;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine;

public class World
{
    private static World? _instance;
    
    private const int WorldWidth = 256;
    private const int WorldHeight = 64;
    private const int WorldDepth = 256;
    private readonly SortedList<ushort, Player> _players = new();
    private readonly SortedList<ushort, BlockParticleEntity> _blockParticles = new();
    private readonly List<PlayerState> _playersToSpawn = new();
    private readonly List<BlockParticleEntityState> _blockParticlesToSpawn = new();
    private readonly List<ushort> _playersToDespawn = new();
    private readonly List<ushort> _blockParticlesToDespawn = new();
    public event EngineDefaults.ChunkAddedHandler? OnChunkAdded;
    public event EngineDefaults.PlayerAddedHandler? OnPlayerAdded;
    private WorldState State { get; }
    public static World? GetInstance()
    {
        return _instance;
    }

    public World(long seed)
    {
        if (_instance != null) throw new Exception("World already exists");

        State = new WorldState(seed);
        _instance = this;
        GenerateChunks(WorldWidth / EngineDefaults.ChunkWidth, WorldHeight / EngineDefaults.ChunkHeight,
            WorldDepth / EngineDefaults.ChunkDepth);
    }

    public World()
    {
        if (_instance != null) throw new Exception("World already exists");

        State = new WorldState();
        _instance = this;
        GenerateChunks(WorldWidth / EngineDefaults.ChunkWidth, WorldHeight / EngineDefaults.ChunkHeight,
            WorldDepth / EngineDefaults.ChunkDepth);
    }

    ~World()
    {
        _instance = null;
    }

    public void Run()
    {
        
    }

    public void NewTick()
    {
        
    }

    protected void PreTick()
    {
        foreach (var state in _playersToSpawn)
        {
            State.RegisterPlayer(state);
            _players.Add(state.EntityId, new Player(state));
        }

        foreach (var state in _blockParticlesToSpawn)
        {
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
        PreTick();
        State.WorldTime += 1;
        TickEntities();
        TickWorld();
        PostTick();
        if (revertPacket)
            State.SerializeChangesToRevertPacket(packet);
        else
            State.SerializeChangesToChangePacket(packet);
        State.FinalizeChanges();
    }

    protected void PostTick()
    {
        foreach (var id in _playersToDespawn)
        {
            State.UnregisterPlayer(id);
            _players.Remove(id);
        }

        foreach (var id in _blockParticlesToDespawn)
        {
            State.UnregisterBlockParticle(id);
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
        var chunkPos = new Vector3i(pos.X / EngineDefaults.ChunkWidth, pos.Y / EngineDefaults.ChunkHeight,
            pos.Z / EngineDefaults.ChunkDepth);
        if (pos.X < 0) chunkPos.X -= EngineDefaults.ChunkWidth;
        if (pos.Y < 0) chunkPos.Y -= EngineDefaults.ChunkHeight;
        if (pos.Z < 0) chunkPos.Z -= EngineDefaults.ChunkDepth;
        return State.GetChunkAt(chunkPos);
    }

    public PlayerState SpawnPlayer()
    {
        var state = State.AddPlayer();
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
        State.RemovePlayer(id);
        _playersToDespawn.Add(id);
    }

    public void DespawnBlockParticleEntity(ushort id)
    {
        State.RemoveBlockParticle(id);
        _blockParticlesToDespawn.Add(id);
    }

    public Block GetBlockAt(Vector3i pos)
    {
        if (IsOutOfBounds(pos))
            return EngineDefaults.Blocks[(int)BlockType.Air];

        var chunk = GetChunkAt(pos);
        var indexVector = pos - chunk.ChunkPosition;
        return EngineDefaults.Blocks[(int)chunk.GetBlockAt(EngineDefaults.GetIndexFromVector(indexVector))];
    }

    public List<Box3> GetBlocksColliding(Box3 collider)
    {
        var min = new Vector3i((int)collider.Min.X, (int)collider.Min.Y, (int)collider.Min.Z);
        var max = new Vector3i((int)collider.Max.X + 1, (int)collider.Max.Y + 1, (int)collider.Max.Z + 1);
        var blocks = new List<Box3>();
        if (collider.Min.X < 0) min.X -= 1;
        if (collider.Min.Y < 0) min.Y -= 1;
        if (collider.Min.Z < 0) min.Z -= 1;
        for (var x = min.X; x < max.X; x++)
        for (var y = min.Y; y < max.Y; y++)
        for (var z = min.Z; z < max.Z; z++)
        {
            var pos = new Vector3i(x, y, z);
            var block = GetBlockAt(pos);
            if (block.IsSolid()) blocks.Add(new Box3(pos + block.BlockBounds.Min, pos + block.BlockBounds.Max));
        }

        return blocks;
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
        ChunkState chunk = GetChunkAt(pos);
        var indexVector = pos - chunk.ChunkPosition;
        chunk.SetBlockAt(EngineDefaults.GetIndexFromVector(indexVector), block);
    }

    private static bool IsOutOfBounds(Vector3i pos)
    {
        return pos.X < 0 || pos.Y < 0 || pos.Z < 0 || pos.X >= WorldWidth || pos.Y >= WorldHeight ||
               pos.Z >= WorldDepth;
    }

    private void GenerateChunks(ushort amountX, ushort amountY, ushort amountZ)
    {
        for (var x = 0; x < amountX; x++)
        for (var z = 0; z < amountZ; z++)
        {
            for (var y = 0; y < amountY; y++)
            {
                var pos = new Vector3i(x * EngineDefaults.ChunkWidth, y * EngineDefaults.ChunkHeight,
                    z * EngineDefaults.ChunkDepth);
                State.AddChunk(pos);
                OnChunkAdded?.Invoke(State.GetChunkAt(pos));
            }

            for (var i = 0; i < EngineDefaults.ChunkWidth; i++)
            for (var j = 0; j < EngineDefaults.ChunkDepth; j++)
                State.AddLight(new Vector2i(x * EngineDefaults.ChunkWidth + i, z * EngineDefaults.ChunkDepth + j));
        }
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
    }

    public byte GetBrightnessAt(Vector3i pos)
    {
        return (byte)(State.GetLight(pos.Xz) > pos.Y ? 1u : 0u);
    }

    public Random GetWorldRandom()
    {
        return State.Random;
    }
}