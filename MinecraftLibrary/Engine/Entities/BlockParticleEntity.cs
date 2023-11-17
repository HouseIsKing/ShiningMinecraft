using MinecraftLibrary.Engine.States.Entities;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.Entities;

public sealed class BlockParticleEntity(BlockParticleEntityState state) : Entity<BlockParticleEntityState>(state)
{
    public override void Tick()
    {
        base.Tick();
        State.LifeTime++;
        if (State.LifeTime >= State.MaxLifeTime) World.GetInstance()?.DespawnBlockParticleEntity(State.EntityId);
        base.Tick();
        var velocity = State.Velocity;
        velocity.Y -= 0.06f;
        State.Velocity = velocity;
        CheckCollisionAndMove();
        velocity = State.Velocity;
        velocity.X *= 0.98f;
        velocity.Z *= 0.98f;
        velocity.Y *= 0.98f;
        if (State.IsGrounded)
        {
            velocity.X *= 0.7f;
            velocity.Z *= 0.7f;
        }

        State.Velocity = velocity;
    }
}