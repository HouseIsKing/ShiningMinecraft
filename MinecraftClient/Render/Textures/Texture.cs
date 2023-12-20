using OpenTK.Graphics.OpenGL4;

namespace MinecraftClient.Render.Textures;

public class Texture(long handle, uint textureId, uint textureIndex)
{
    private static readonly Dictionary<string, Texture> TexturesCache = new();
    private static readonly SortedDictionary<uint, Texture> TextureIndexToTexture = new();
    private static uint _textureIndex;
    private static readonly uint Ubo;

    static Texture()
    {
        GL.CreateBuffers(1, out Ubo);
    }
    public long Handle { get; } = handle;
    public uint TextureId { get; } = textureId;

    public uint TextureIndex { get; } = textureIndex;

    public static Texture LoadTexture(string path)
    {
        if (TexturesCache.TryGetValue(path, out var texture)) return texture;
        var image = new FileStream(path, FileMode.Open, FileAccess.Read);
        var header = new byte[128];
        image.ReadExactly(header, 0, 128);
        if (BitConverter.ToUInt32(header, 0) != 0x20534444) throw new Exception("Invalid texture file");
        var height = BitConverter.ToInt32(header, 12);
        var width = BitConverter.ToInt32(header, 16);
        var mipMapCount = BitConverter.ToInt32(header, 28);
        var format = BitConverter.ToInt32(header, 84);
        var blockSize = (CompressionFormat)format switch
        {
            CompressionFormat.Dxt1 => (8, 33777),
            CompressionFormat.Dxt5 => (16, 33779),
            _ => throw new Exception("Invalid texture file")
        };
        GL.CreateTextures(TextureTarget.Texture2D, 1, out uint textureId);
        GL.TextureStorage2D(textureId, mipMapCount, (SizedInternalFormat)blockSize.Item2, width, height);
        GL.TextureParameter(textureId, TextureParameterName.TextureMaxAnisotropy, 8.0f);
        GL.TextureParameter(textureId, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TextureParameter(textureId, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TextureParameter(textureId, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TextureParameter(textureId, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        var w = width;
        var h = height;
        var size = (w + 3) / 4 * ((h + 3) / 4) * blockSize.Item1;
        var data = new byte[size];
        for (var i = 0; i < mipMapCount; i++)
        {
            if (w == 0 || h == 0)
            {
                mipMapCount--;
                continue;
            }
            image.ReadExactly(data, 0, size);
            GL.CompressedTextureSubImage2D(textureId, i, 0, 0, w, h, (PixelFormat)blockSize.Item2, size, data);
            w /= 2;
            h /= 2;
            size = (w + 3) / 4 * ((h + 3) / 4) * blockSize.Item1;
        }
        GL.TextureParameter(textureId, TextureParameterName.TextureMaxLevel, mipMapCount - 1);
        texture = new Texture(GL.Arb.GetTextureHandle(textureId), textureId, _textureIndex++);
        TexturesCache.Add(path, texture);
        TextureIndexToTexture.Add(texture.TextureIndex, texture);
        texture.Resident();
        return texture;
    }

    public void Resident()
    {
        GL.Arb.MakeTextureHandleResident(Handle);
    }
    
    public void NonResident()
    {
        GL.Arb.MakeTextureHandleNonResident(Handle);
    }
    
    ~Texture()
    {
        NonResident();
        GL.DeleteTexture(TextureId);
    }

    public static void SetupTextures()
    {
        var textureHandles = new long[TextureIndexToTexture.Count];
        foreach (var texture in TextureIndexToTexture.Values) textureHandles[texture.TextureIndex] = texture.Handle;
        GL.NamedBufferStorage(Ubo, sizeof(long) * TextureIndexToTexture.Count, textureHandles, BufferStorageFlags.None);
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 3, Ubo);
    }
    
    public static void Terminate()
    {
        GL.DeleteBuffer(Ubo);
    }
}