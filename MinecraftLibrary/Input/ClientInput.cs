using System.Runtime.InteropServices;

namespace MinecraftLibrary.Input;
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct ClientInput
{
    public float MouseX = 0.0F;
    public float MouseY = 0.0F;
    public byte KeySet1 = 0; // bit 0 = LeftMouseButtonPressed, bit 1 = RightMouseButtonPressed, bit 2 = JumpPressed, bit 3 = ResetPressed, bit 4 = SpawnZombiePressed, bit 5 = OnePressed, bit 6 = TwoPressed, bit 7 = ThreePressed
    public byte KeySet2 = 0; // bit 0 = FourPressed, bit 1 = FivePressed, bit 2 = ForwardPressed, bit 3 = BackwardPressed, bit 4 = LeftPressed, bit 5 = RightPressed
    public byte KeySet3 = 0;
    public byte KeySet4 = 0;

    public ClientInput()
    {
    }

    public void SetKey(KeySet key, bool pressed)
    {
        switch (key)
        {
            case KeySet.LeftMouseButton:
                KeySet1 = (byte)((KeySet1 & 0xFE) + Convert.ToByte(pressed));
                break;
            case KeySet.RightMouseButton:
                KeySet1 = (byte)((KeySet1 & 0xFD) + (Convert.ToByte(pressed) << 1));
                break;
            case KeySet.Jump:
                KeySet1 = (byte)((KeySet1 & 0xFB) + (Convert.ToByte(pressed) << 2));
                break;
            case KeySet.Reset:
                KeySet1 = (byte)((KeySet1 & 0xF7) + (Convert.ToByte(pressed) << 3));
                break;
            case KeySet.SpawnZombie:
                KeySet1 = (byte)((KeySet1 & 0xEF) + (Convert.ToByte(pressed) << 4));
                break;
            case KeySet.One:
                KeySet1 = (byte)((KeySet1 & 0xDF) + (Convert.ToByte(pressed) << 5));
                break;
            case KeySet.Two:
                KeySet1 = (byte)((KeySet1 & 0xBF) + (Convert.ToByte(pressed) << 6));
                break;
            case KeySet.Three:
                KeySet1 = (byte)((KeySet1 & 0x7F) + (Convert.ToByte(pressed) << 7));
                break;
            case KeySet.Four:
                KeySet2 = (byte)((KeySet2 & 0xFE) + Convert.ToByte(pressed));
                break;
            case KeySet.Five:
                KeySet2 = (byte)((KeySet2 & 0xFD) + (Convert.ToByte(pressed) << 1));
                break;
            case KeySet.Up:
                KeySet2 = (byte)((KeySet2 & 0xFB) + (Convert.ToByte(pressed) << 2));
                break;
            case KeySet.Down:
                KeySet2 = (byte)((KeySet2 & 0xF7) + (Convert.ToByte(pressed) << 3));
                break;
            case KeySet.Left:
                KeySet2 = (byte)((KeySet2 & 0xEF) + (Convert.ToByte(pressed) << 4));
                break;
            case KeySet.Right:
                KeySet2 = (byte)((KeySet2 & 0xDF) + (Convert.ToByte(pressed) << 5));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(key), key, null);
        }
    }
}