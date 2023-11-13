using MinecraftLibrary.Engine.States.Entities;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.Entities;

public class Entity<TEntityState> where TEntityState : EntityState<TEntityState>
{
    public TEntityState State { get; }
    public Entity(TEntityState state)
    {
        State = state;
    }
    
    public Box3 GetBoundingBox()
    {
        return new Box3(State.Position - State.Scale, State.Position + State.Scale);
    }

    protected void CheckCollisionAndMove()
    {
        var originalY = State.Velocity.Y;
        var boundingBox = GetBoundingBox();
        var movementBox = EngineDefaults.Expand(boundingBox, State.Velocity);
        movementBox.HalfSize += Vector3.One;
        World.GetInstance()
    }
}