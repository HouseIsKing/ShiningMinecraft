using MinecraftLibrary.Engine.States.Entities;
using OpenTK.Mathematics;

namespace MinecraftLibrary.Engine.Entities;

public abstract class LivingEntity<TLivingEntityState> : Entity<TLivingEntityState> where TLivingEntityState : LivingEntityState<TLivingEntityState>
{
    protected LivingEntity(TLivingEntityState state) : base(state)
    {
    }

    protected void CalculateVelocityAndMove()
    {
        Vector3 velocity = State.Velocity;
        float speedModifier = 0.1f;
        if (State is { JumpInput: true, IsGrounded: true }) velocity.Y = 0.5f;
        if (!State.IsGrounded) speedModifier *= 0.2f;
        velocity.Y -= 0.08f;
        Vector3 rotation = State.Rotation;
        if (State.HorizontalInput * State.HorizontalInput + State.VerticalInput * State.VerticalInput > 0)
        {
            velocity.X += speedModifier * (State.VerticalInput * MathF.Cos(MathHelper.DegreesToRadians(rotation.Y)) - State.HorizontalInput * MathF.Sin(MathHelper.DegreesToRadians(rotation.Y)));
            velocity.Z += speedModifier * (State.VerticalInput * MathF.Sin(MathHelper.DegreesToRadians(rotation.Y)) + State.HorizontalInput * MathF.Cos(MathHelper.DegreesToRadians(rotation.Y)));
        }
        State.Velocity = velocity;
        CheckCollisionAndMove();
        velocity = State.Velocity;
        velocity.X *= 0.91f;
        velocity.Z *= 0.91f;
        velocity.Y *= 0.98f;
        if (State.IsGrounded)
        {
            velocity.X *= 0.7f;
            velocity.Z *= 0.7f;
        }
        State.Velocity = velocity;
    }

    public override void Tick()
    {
        base.Tick();
        CalculateVelocityAndMove();
    }
}