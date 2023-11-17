using MinecraftLibrary.Network;

namespace MinecraftLibrary.Input;

public struct PlayerInput
{
    private float MouseX = 0.0f;
    private float MouseY = 0.0f;
    private byte KeySet1Pressed = 0;
    private byte KeySet2Pressed = 0;
    private byte KeySet1Hold = 0;
    private byte KeySet2Hold = 0;

    public PlayerInput()
    {
    }

    public bool IsKeyPressed(KeySet key)
    {
        switch (key)
        {
            case KeySet.LeftMouseButton:
                return Convert.ToBoolean(KeySet1Pressed & 0x1);
            case KeySet.RightMouseButton:
                return Convert.ToBoolean((KeySet1Pressed & 0x2) >> 1);
            case KeySet.Jump:
                return Convert.ToBoolean((KeySet1Pressed & 0x4) >> 2);
            case KeySet.Reset:
                return Convert.ToBoolean((KeySet1Pressed & 0x8) >> 3);
            case KeySet.SpawnZombie:
                return Convert.ToBoolean((KeySet1Pressed & 0x10) >> 4);
            case KeySet.One:
                return Convert.ToBoolean((KeySet1Pressed & 0x20) >> 5);
            case KeySet.Two:
                return Convert.ToBoolean((KeySet1Pressed & 0x40) >> 6);
            case KeySet.Three:
                return Convert.ToBoolean((KeySet1Pressed & 0x80) >> 7);
            case KeySet.Four:
                return Convert.ToBoolean(KeySet2Pressed & 0x1);
            case KeySet.Five:
                return Convert.ToBoolean((KeySet2Pressed & 0x2) >> 1);
            case KeySet.Up:
                return Convert.ToBoolean((KeySet2Pressed & 0x4) >> 2);
            case KeySet.Down:
                return Convert.ToBoolean((KeySet2Pressed & 0x8) >> 3);
            case KeySet.Left:
                return Convert.ToBoolean((KeySet2Pressed & 0x10) >> 4);
            case KeySet.Right:
                return Convert.ToBoolean((KeySet2Pressed & 0x20) >> 5);
        }

        return false;
    }

    public bool IsKeyHold(KeySet key)
    {
        switch (key)
        {
            case KeySet.LeftMouseButton:
                return Convert.ToBoolean(KeySet1Hold & 0x1);
            case KeySet.RightMouseButton:
                return Convert.ToBoolean((KeySet1Hold & 0x2) >> 1);
            case KeySet.Jump:
                return Convert.ToBoolean((KeySet1Hold & 0x4) >> 2);
            case KeySet.Reset:
                return Convert.ToBoolean((KeySet1Hold & 0x8) >> 3);
            case KeySet.SpawnZombie:
                return Convert.ToBoolean((KeySet1Hold & 0x10) >> 4);
            case KeySet.One:
                return Convert.ToBoolean((KeySet1Hold & 0x20) >> 5);
            case KeySet.Two:
                return Convert.ToBoolean((KeySet1Hold & 0x40) >> 6);
            case KeySet.Three:
                return Convert.ToBoolean((KeySet1Hold & 0x80) >> 7);
            case KeySet.Four:
                return Convert.ToBoolean(KeySet2Hold & 0x1);
            case KeySet.Five:
                return Convert.ToBoolean((KeySet2Hold & 0x2) >> 1);
            case KeySet.Up:
                return Convert.ToBoolean((KeySet2Hold & 0x4) >> 2);
            case KeySet.Down:
                return Convert.ToBoolean((KeySet2Hold & 0x8) >> 3);
            case KeySet.Left:
                return Convert.ToBoolean((KeySet2Hold & 0x10) >> 4);
            case KeySet.Right:
                return Convert.ToBoolean((KeySet2Hold & 0x20) >> 5);
        }
        return false;
    }
    
    public float GetMouseX()
    {
        return MouseX;
    }
    
    public float GetMouseY()
    {
        return MouseY;
    }

    public void ApplyClientInput(ClientInput input)
    {
        MouseX = input.MouseX;
        MouseY = input.MouseY;
        var prevKeySet1Hold = KeySet1Hold;
        var prevKeySet2Hold = KeySet2Hold;
        KeySet1Hold = input.KeySet1;
        KeySet2Hold = input.KeySet2;
        KeySet1Pressed = (byte)(KeySet1Hold & ~prevKeySet1Hold);
        KeySet2Pressed = (byte)(KeySet2Hold & ~prevKeySet2Hold);
    }
    
    public void Serialize(Packet packet)
    {
        packet.Write(MouseX);
        packet.Write(MouseY);
        packet.Write(KeySet1Pressed);
        packet.Write(KeySet2Pressed);
    }

    public void Deserialize(Packet packet)
    {
        packet.Read(out MouseX);
        packet.Read(out MouseY);
        packet.Read(out KeySet1Pressed);
        packet.Read(out KeySet2Pressed);
    }
}