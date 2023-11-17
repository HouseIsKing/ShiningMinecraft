using System.Numerics;
using OpenTK.Mathematics;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace MinecraftClient.Entities.Player;

public readonly struct Frustum
{
    private readonly Vector4[] _planes = new Vector4[6];

    public Frustum(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
    {
        var v = viewMatrix;
        var p = projectionMatrix;
        Matrix4x4 clipMatrix = new Matrix4x4
        {
            [0, 0] = v[0, 0] * p[0, 0] + v[0, 1] * p[1, 0] + v[0, 2] * p[2, 0] + v[0, 3] * p[3, 0],
            [1, 0] = v[0, 0] * p[0, 1] + v[0, 1] * p[1, 1] + v[0, 2] * p[2, 1] + v[0, 3] * p[3, 1],
            [2, 0] = v[0, 0] * p[0, 2] + v[0, 1] * p[1, 2] + v[0, 2] * p[2, 2] + v[0, 3] * p[3, 2],
            [3, 0] = v[0, 0] * p[0, 3] + v[0, 1] * p[1, 3] + v[0, 2] * p[2, 3] + v[0, 3] * p[3, 3],
            [0, 1] = v[1, 0] * p[0, 0] + v[1, 1] * p[1, 0] + v[1, 2] * p[2, 0] + v[1, 3] * p[3, 0],
            [1, 1] = v[1, 0] * p[0, 1] + v[1, 1] * p[1, 1] + v[1, 2] * p[2, 1] + v[1, 3] * p[3, 1],
            [2, 1] = v[1, 0] * p[0, 2] + v[1, 1] * p[1, 2] + v[1, 2] * p[2, 2] + v[1, 3] * p[3, 2],
            [3, 1] = v[1, 0] * p[0, 3] + v[1, 1] * p[1, 3] + v[1, 2] * p[2, 3] + v[1, 3] * p[3, 3],
            [0, 2] = v[2, 0] * p[0, 0] + v[2, 1] * p[1, 0] + v[2, 2] * p[2, 0] + v[2, 3] * p[3, 0],
            [1, 2] = v[2, 0] * p[0, 1] + v[2, 1] * p[1, 1] + v[2, 2] * p[2, 1] + v[2, 3] * p[3, 1],
            [2, 2] = v[2, 0] * p[0, 2] + v[2, 1] * p[1, 2] + v[2, 2] * p[2, 2] + v[2, 3] * p[3, 2],
            [3, 2] = v[2, 0] * p[0, 3] + v[2, 1] * p[1, 3] + v[2, 2] * p[2, 3] + v[2, 3] * p[3, 3],
            [0, 3] = v[3, 0] * p[0, 0] + v[3, 1] * p[1, 0] + v[3, 2] * p[2, 0] + v[3, 3] * p[3, 0],
            [1, 3] = v[3, 0] * p[0, 1] + v[3, 1] * p[1, 1] + v[3, 2] * p[2, 1] + v[3, 3] * p[3, 1],
            [2, 3] = v[3, 0] * p[0, 2] + v[3, 1] * p[1, 2] + v[3, 2] * p[2, 2] + v[3, 3] * p[3, 2],
            [3, 3] = v[3, 0] * p[0, 3] + v[3, 1] * p[1, 3] + v[3, 2] * p[2, 3] + v[3, 3] * p[3, 3]
        };
        var row0 = new Vector4(clipMatrix[0, 0], clipMatrix[0, 1], clipMatrix[0, 2], clipMatrix[0, 3]);
        var row1 = new Vector4(clipMatrix[1, 0], clipMatrix[1, 1], clipMatrix[1, 2], clipMatrix[1, 3]);
        var row2 = new Vector4(clipMatrix[2, 0], clipMatrix[2, 1], clipMatrix[2, 2], clipMatrix[2, 3]);
        var row3 = new Vector4(clipMatrix[3, 0], clipMatrix[3, 1], clipMatrix[3, 2], clipMatrix[3, 3]);
        _planes[(int)Plane.PlaneRight] = row3 - row0;
        _planes[(int)Plane.PlaneLeft] = row3 + row0;
        _planes[(int)Plane.PlaneBottom] = row3 + row1;
        _planes[(int)Plane.PlaneTop] = row3 - row1;
        _planes[(int)Plane.PlaneBack] = row3 - row2;
        _planes[(int)Plane.PlaneFront] = row3 + row2;
        for (byte i = 0; i < 6; i++)
        {
            _planes[i] = Vector4.Normalize(_planes[i]);
        }
    }

    private bool CubeInFrustum(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
        for (byte i = 0; i < 6; i++)
        {
            if (_planes[i].X * minX + _planes[i].Y * minY + _planes[i].Z * minZ + _planes[i].W > 0)
            {
                continue;
            }
            if (_planes[i].X * maxX + _planes[i].Y * minY + _planes[i].Z * minZ + _planes[i].W > 0)
            {
                continue;
            }
            if (_planes[i].X * minX + _planes[i].Y * maxY + _planes[i].Z * minZ + _planes[i].W > 0)
            {
                continue;
            }
            if (_planes[i].X * maxX + _planes[i].Y * maxY + _planes[i].Z * minZ + _planes[i].W > 0)
            {
                continue;
            }
            if (_planes[i].X * minX + _planes[i].Y * minY + _planes[i].Z * maxZ + _planes[i].W > 0)
            {
                continue;
            }
            if (_planes[i].X * maxX + _planes[i].Y * minY + _planes[i].Z * maxZ + _planes[i].W > 0)
            {
                continue;
            }
            if (_planes[i].X * minX + _planes[i].Y * maxY + _planes[i].Z * maxZ + _planes[i].W > 0)
            {
                continue;
            }
            if (_planes[i].X * maxX + _planes[i].Y * maxY + _planes[i].Z * maxZ + _planes[i].W > 0)
            {
                continue;
            }
            // If we get here, it isn't in the frustum
            return false;
        }

        return true;
    }

    public bool CubeInFrustum(Box3 box)
    {
        return CubeInFrustum(box.Min.X, box.Min.Y, box.Min.Z, box.Max.X, box.Max.Y, box.Max.Z);
    }
}