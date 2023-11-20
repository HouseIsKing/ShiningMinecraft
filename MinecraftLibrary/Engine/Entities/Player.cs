using MinecraftLibrary.Engine.Blocks;
using MinecraftLibrary.Engine.States.Entities;
using MinecraftLibrary.Input;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.Entities;

public sealed class Player : LivingEntity<PlayerState>
{
    private readonly Queue<KeyValuePair<ulong, ClientInput>> _inputQueue = new();
    public ulong LastInputProcessed { get; private set; }
    private bool _foundBlock;
    private Vector3i _hitPosition;
    private BlockFaces _hitFace;

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
            State.PlayerInput.ApplyClientInput(helper.Value);
        }
        else
        {
            State.PlayerInput.ApplyClientInput(new ClientInput());
        }

        ProcessCurrentPlayerInput();
    }

    public void AddInput(ulong inputId, ClientInput input)
    {
        _inputQueue.Enqueue(KeyValuePair.Create(inputId, input));
    }

    private void FindClosestFace()
    {
        _foundBlock = false;
        var forwardVector = EngineDefaults.GetFrontVector(State.Rotation.Y, State.Rotation.X);
        var position = new Vector3(State.Position.X,
            State.Position.Y + EngineDefaults.CameraOffset - EngineDefaults.PlayerSize.Y, State.Position.Z);
        var right = forwardVector.X > 0.0F;
        var up = forwardVector.Y > 0.0F;
        var forward = forwardVector.Z > 0.0F;
        var totalDistance = 0.0F;
        const float maxDistance = 4.0F;
        while (totalDistance <= maxDistance)
        {
            var xDistanceToComplete = forwardVector.X;
            if (right)
                xDistanceToComplete = EngineDefaults.GetNextWholeNumberDistance(position.X) / xDistanceToComplete;
            else xDistanceToComplete = EngineDefaults.GetPrevWholeNumberDistance(position.X) / xDistanceToComplete;
            var yDistanceToComplete = forwardVector.Y;
            if (up) yDistanceToComplete = EngineDefaults.GetNextWholeNumberDistance(position.Y) / yDistanceToComplete;
            else yDistanceToComplete = EngineDefaults.GetPrevWholeNumberDistance(position.Y) / yDistanceToComplete;
            var zDistanceToComplete = forwardVector.Z;
            if (forward)
                zDistanceToComplete = EngineDefaults.GetNextWholeNumberDistance(position.Z) / zDistanceToComplete;
            else zDistanceToComplete = EngineDefaults.GetPrevWholeNumberDistance(position.Z) / zDistanceToComplete;
            if (xDistanceToComplete < yDistanceToComplete && xDistanceToComplete < zDistanceToComplete)
            {
                totalDistance += xDistanceToComplete;
                if (totalDistance > maxDistance) return;

                position += xDistanceToComplete * forwardVector;
                _hitPosition = right
                    ? new Vector3i((int)position.X, (int)position.Y, (int)position.Z)
                    : new Vector3i((int)position.X - 1, (int)position.Y, (int)position.Z);
                if (!World.GetInstance()!.GetBlockAt(_hitPosition).IsSolid()) continue;

                _foundBlock = true;
                _hitFace = right ? BlockFaces.West : BlockFaces.East;
            }

            if (yDistanceToComplete < zDistanceToComplete)
            {
                totalDistance += yDistanceToComplete;
                if (totalDistance > maxDistance) return;

                position += yDistanceToComplete * forwardVector;
                _hitPosition = up
                    ? new Vector3i((int)position.X, (int)position.Y, (int)position.Z)
                    : new Vector3i((int)position.X, (int)position.Y - 1, (int)position.Z);
                if (!World.GetInstance()!.GetBlockAt(_hitPosition).IsSolid()) continue;

                _foundBlock = true;
                _hitFace = up ? BlockFaces.Bottom : BlockFaces.Top;
            }

            totalDistance += zDistanceToComplete;
            if (totalDistance > maxDistance) return;

            position += zDistanceToComplete * forwardVector;
            _hitPosition = forward
                ? new Vector3i((int)position.X, (int)position.Y, (int)position.Z)
                : new Vector3i((int)position.X, (int)position.Y, (int)position.Z - 1);
            if (!World.GetInstance()!.GetBlockAt(_hitPosition).IsSolid()) continue;

            _foundBlock = true;
            _hitFace = forward ? BlockFaces.South : BlockFaces.North;
        }
    }

    private void PlaceBlock()
    {
        var blockToPlace = EngineDefaults.Blocks[(int)State.CurrentSelectedBlock];
        var blockBox = blockToPlace.BlockBounds;
        switch (_hitFace)
        {
            case BlockFaces.Bottom:
                blockBox.Translate(_hitPosition - Vector3i.UnitY);
                break;
            case BlockFaces.Top:
                blockBox.Translate(_hitPosition + Vector3i.UnitY);
                break;
            case BlockFaces.East:
                blockBox.Translate(_hitPosition + Vector3i.UnitX);
                break;
            case BlockFaces.West:
                blockBox.Translate(_hitPosition - Vector3i.UnitX);
                break;
            case BlockFaces.North:
                blockBox.Translate(_hitPosition + Vector3i.UnitZ);
                break;
            case BlockFaces.South:
                blockBox.Translate(_hitPosition - Vector3i.UnitZ);
                break;
        }

        if (blockToPlace.IsSolid() && EngineDefaults.IsIntersecting(GetBoundingBox(), blockBox)) return;

        World.GetInstance()?.SetBlockAt((Vector3i)blockBox.Min, State.CurrentSelectedBlock);
    }

    private void ProcessCurrentPlayerInput()
    {
        var input = State.PlayerInput;
        State.JumpInput = input.IsKeyHold(KeySet.Jump);
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
        if (input.IsKeyPressed(KeySet.LeftMouseButton) && _foundBlock)
        {
            if (State.Mode)
                PlaceBlock();
            else
                World.GetInstance()?.BreakBlock(_hitPosition);
        }

        if (!input.IsKeyPressed(KeySet.Reset)) return;

        var random = World.GetInstance()?.GetWorldRandom() ?? throw new InvalidOperationException();
        State.Position = new Vector3(random.NextFloat() * 256.0f, 67.0f, random.NextFloat() * 256.0f);
    }
}