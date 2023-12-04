using MinecraftLibrary.Engine.Blocks;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Input;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.Entities;

public sealed class Player : LivingEntity<PlayerState>
{
    private readonly Queue<KeyValuePair<ulong, ClientInput>> _inputQueue = new();
    public ulong LastInputProcessed { get; private set; }
    public bool FoundBlock { get; private set; }
    public Vector3i HitPosition { get; private set; }
    public BlockFaces HitFace { get; private set; }

    public Player(PlayerState state) : base(state)
    {
        State.Scale = EngineDefaults.PlayerSize;
    }

    public override void Tick()
    {
        base.Tick();
        FindClosestFace();
        if (_inputQueue.TryDequeue(out var helper))
        {
            LastInputProcessed = helper.Key;
            State.ApplyClientInput(helper.Value);
        }
        else
        {
            State.ApplyClientInput(new ClientInput());
        }

        ProcessCurrentPlayerInput();
    }

    public void AddInput(ulong inputId, ClientInput input)
    {
        _inputQueue.Enqueue(KeyValuePair.Create(inputId, input));
    }

    private void FindClosestFace()
    {
        FoundBlock = false;
        var forwardVector = EngineDefaults.GetFrontVector(State.Rotation.Y, State.Pitch);
        var position = new Vector3(State.Position.X, State.Position.Y + EngineDefaults.CameraOffset - EngineDefaults.PlayerSize.Y, State.Position.Z);
        var right = forwardVector.X > 0.0F;
        var up = forwardVector.Y > 0.0F;
        var forward = forwardVector.Z > 0.0F;
        var totalDistance = 0.0F;
        const float maxDistance = 4.0F;
        while (totalDistance <= maxDistance)
        {
            var xDistanceToComplete = float.Abs(forwardVector.X);
            if (right) xDistanceToComplete = EngineDefaults.GetNextWholeNumberDistance(position.X) / xDistanceToComplete;
            else xDistanceToComplete = EngineDefaults.GetPrevWholeNumberDistance(position.X) / xDistanceToComplete;
            var yDistanceToComplete = float.Abs(forwardVector.Y);
            if (up) yDistanceToComplete = EngineDefaults.GetNextWholeNumberDistance(position.Y) / yDistanceToComplete;
            else yDistanceToComplete = EngineDefaults.GetPrevWholeNumberDistance(position.Y) / yDistanceToComplete;
            var zDistanceToComplete = float.Abs(forwardVector.Z);
            if (forward) zDistanceToComplete = EngineDefaults.GetNextWholeNumberDistance(position.Z) / zDistanceToComplete;
            else zDistanceToComplete = EngineDefaults.GetPrevWholeNumberDistance(position.Z) / zDistanceToComplete;
            if (xDistanceToComplete < yDistanceToComplete && xDistanceToComplete < zDistanceToComplete)
            {
                totalDistance += float.Abs(xDistanceToComplete);
                if (totalDistance > maxDistance) return;

                position += xDistanceToComplete * forwardVector;
                HitPosition = right
                    ? new Vector3i((int)position.X, (int)position.Y, (int)position.Z)
                    : new Vector3i((int)position.X - 1, (int)position.Y, (int)position.Z);
                if (!World.GetInstance()!.GetBlockAt(HitPosition).IsSolid()) continue;

                FoundBlock = true;
                HitFace = right ? BlockFaces.West : BlockFaces.East;
                break;
            }

            if (yDistanceToComplete < zDistanceToComplete)
            {
                totalDistance += float.Abs(yDistanceToComplete);
                if (totalDistance > maxDistance) return;

                position += yDistanceToComplete * forwardVector;
                HitPosition = up
                    ? new Vector3i((int)position.X, (int)position.Y, (int)position.Z)
                    : new Vector3i((int)position.X, (int)position.Y - 1, (int)position.Z);
                if (!World.GetInstance()!.GetBlockAt(HitPosition).IsSolid()) continue;

                FoundBlock = true;
                HitFace = up ? BlockFaces.Bottom : BlockFaces.Top;
                break;
            }

            totalDistance += float.Abs(zDistanceToComplete);
            if (totalDistance > maxDistance) return;

            position += zDistanceToComplete * forwardVector;
            HitPosition = forward
                ? new Vector3i((int)position.X, (int)position.Y, (int)position.Z)
                : new Vector3i((int)position.X, (int)position.Y, (int)position.Z - 1);
            if (!World.GetInstance()!.GetBlockAt(HitPosition).IsSolid()) continue;

            FoundBlock = true;
            HitFace = forward ? BlockFaces.South : BlockFaces.North;
            break;
        }
    }

    private void PlaceBlock()
    {
        var blockToPlace = Block.GetBlock(State.CurrentSelectedBlock);
        var blockBox = blockToPlace.BlockBounds;
        switch (HitFace)
        {
            case BlockFaces.Bottom:
                blockBox.Translate(HitPosition - Vector3i.UnitY);
                break;
            case BlockFaces.Top:
                blockBox.Translate(HitPosition + Vector3i.UnitY);
                break;
            case BlockFaces.East:
                blockBox.Translate(HitPosition + Vector3i.UnitX);
                break;
            case BlockFaces.West:
                blockBox.Translate(HitPosition - Vector3i.UnitX);
                break;
            case BlockFaces.North:
                blockBox.Translate(HitPosition + Vector3i.UnitZ);
                break;
            case BlockFaces.South:
                blockBox.Translate(HitPosition - Vector3i.UnitZ);
                break;
        }

        if (blockToPlace.IsSolid() && EngineDefaults.IsIntersecting(GetBoundingBox(), blockBox)) return;

        World.GetInstance()?.SetBlockAt((Vector3i)blockBox.Min, State.CurrentSelectedBlock);
    }

    private void ProcessCurrentPlayerInput()
    {
        var input = State.PlayerInput;
        State.JumpInput = input.IsKeyPressed(KeySet.Jump);
        State.Pitch = Math.Clamp(State.Pitch + input.GetMouseY(), -89.0f, 89.0f);
        State.Rotation = State.Rotation with { Y = State.Rotation.Y + input.GetMouseX() };
        if (input.IsKeyHold(KeySet.Up)) State.VerticalInput = 1;
        else if (input.IsKeyHold(KeySet.Down)) State.VerticalInput = -1;
        if (input.IsKeyHold(KeySet.Left)) State.HorizontalInput = -1;
        else if (input.IsKeyHold(KeySet.Right)) State.HorizontalInput = 1;
        if (input.IsKeyPressed(KeySet.One)) State.CurrentSelectedBlock = BlockType.Stone;
        if (input.IsKeyPressed(KeySet.Two)) State.CurrentSelectedBlock = BlockType.Dirt;
        if (input.IsKeyPressed(KeySet.Three)) State.CurrentSelectedBlock = BlockType.Cobblestone;
        if (input.IsKeyPressed(KeySet.Four)) State.CurrentSelectedBlock = BlockType.Planks;
        if (input.IsKeyPressed(KeySet.Five)) State.CurrentSelectedBlock = BlockType.Sapling;
        if (input.IsKeyPressed(KeySet.RightMouseButton)) State.Mode = !State.Mode;
        if (input.IsKeyPressed(KeySet.LeftMouseButton) && FoundBlock)
        {
            if (State.Mode)
                PlaceBlock();
            else
                World.GetInstance()?.BreakBlock(HitPosition);
        }

        if (!input.IsKeyPressed(KeySet.Reset)) return;

        var random = World.GetInstance()?.GetWorldRandom() ?? throw new InvalidOperationException();
        State.Position = new Vector3(random.NextFloat() * 256.0f, 67.0f, random.NextFloat() * 256.0f);
    }
}