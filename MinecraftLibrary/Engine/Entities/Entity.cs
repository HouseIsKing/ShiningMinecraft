using MinecraftLibrary.Engine.States.Entities;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.Entities;

public abstract class Entity<TEntityState> where TEntityState : EntityState<TEntityState>
{
    public TEntityState State { get; }
    protected Entity(TEntityState state)
    {
        State = state;
    }
    
    public Box3 GetBoundingBox()
    {
        return new Box3(State.Position - State.Scale, State.Position + State.Scale);
    }
    
    public virtual void Tick()
    {
    }

    protected void CheckCollisionAndMove()
    {
        var boundingBox = GetBoundingBox();
        var movementBox = EngineDefaults.Expand(boundingBox, State.Velocity);
        movementBox.HalfSize += Vector3.One;
        var collidingBoxes = World.GetInstance()?.GetBlocksColliding(movementBox) ??
                             throw new InvalidOperationException();
        var movementVector = State.Velocity;
        foreach (var collidingBox in collidingBoxes)
            EngineDefaults.ClipCollisionY(boundingBox, collidingBox, ref movementVector.Y);
        boundingBox.Translate(Vector3.UnitY * movementVector.Y);
        foreach (var collidingBox in collidingBoxes)
            EngineDefaults.ClipCollisionX(boundingBox, collidingBox, ref movementVector.X);
        boundingBox.Translate(Vector3.UnitX * movementVector.X);
        foreach (var collidingBox in collidingBoxes)
            EngineDefaults.ClipCollisionZ(boundingBox, collidingBox, ref movementVector.Z);
        boundingBox.Translate(Vector3.UnitZ * movementVector.Z);
        State.Position += movementVector;
        if (Math.Abs(movementVector.Y - State.Velocity.Y) > 0.0f)
        {
            State.IsGrounded = State.Velocity.Y < 0;
            movementVector.Y = 0;
        }
        else
        {
            State.IsGrounded = false;
        }

        if (Math.Abs(movementVector.X - State.Velocity.X) > 0.0f) movementVector.X = 0;
        if (Math.Abs(movementVector.Z - State.Velocity.Z) > 0.0f) movementVector.Z = 0;
        State.Velocity = movementVector;
    }
}