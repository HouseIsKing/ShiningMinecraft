namespace MinecraftLibrary.Engine.States;

public enum StateChange : ushort
{
    WorldTime,
    WorldEntityEnter,
    WorldEntityLeave,
    WorldChunk,
    WorldLight,
    WorldEntity,
    EntityPosition,
    EntityRotation,
    EntityScale,
    EntityVelocity,
    EntityIsGrounded,
    LivingEntityJumpInput,
    LivingEntityHorizontalInput,
    LivingEntityVerticalInput,
    PlayerPitch,
    PlayerMode,
    PlayerCurrentSelectedBlock,
    PlayerInput
}