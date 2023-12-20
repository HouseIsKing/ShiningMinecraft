using MinecraftLibrary.Engine;
using OpenTK.Mathematics;

namespace MinecraftClient.Render.Entities.Player;

internal sealed class Camera
{
    private Vector3 _front;
    private Vector3 _up;
    private Vector3 _right;
    private float _fov;
    private float _aspectRatio;
    private Matrix4 _viewMatrix;
    private readonly byte[,,] _viewMatrixBytes = new byte[4, 4, 4];
    private Matrix4 _projectionMatrix;
    private readonly byte[,,] _projectionMatrixBytes = new byte[4, 4, 4];
    private bool _isDirtyProjectionMatrix;
    private readonly float _zNear;
    private readonly float _zFar;
    private readonly Frustum _frustum = new();
    private EngineDefaults.UInt32ToSingle _converter;

    internal float Pitch;
    internal float Yaw;
    internal Vector3 Position;
    
    private static readonly Camera Instance = new(new Vector3(0, 16.1f, 0), 1280.0f / 720.0f);

    internal static Camera GetInstance()
    {
        return Instance;
    }

    private Camera(Vector3 position, float aspectRatio)
    {
        Position = position;
        _aspectRatio = aspectRatio;
        _fov = 70;
        _zNear = 0.05f;
        _zFar = 1000;
        _isDirtyProjectionMatrix = true;
        Yaw = 0;
        Pitch = 0;
        UpdateVectors();
    }
    
    private void UpdateVectors()
    {
        _front.X = (float)(Math.Cos(MathHelper.DegreesToRadians(Yaw)) * Math.Cos(MathHelper.DegreesToRadians(Pitch)));
        _front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Pitch));
        _front.Z = (float)(Math.Sin(MathHelper.DegreesToRadians(Yaw)) * Math.Cos(MathHelper.DegreesToRadians(Pitch)));
        _front = Vector3.Normalize(_front);
        _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        _up = Vector3.Normalize(Vector3.Cross(_right, _front));
    }

    internal Frustum GetFrustum()
    {
        _frustum.UpdateFrustum(GetViewMatrix(), GetProjectionMatrix());
        return _frustum;
    }

    internal void SetFov(float fov)
    {
        _fov = fov;
        _isDirtyProjectionMatrix = true;
    }

    public Vector3 GetFrontVector()
    {
        return _front;
    }

    internal Matrix4 GetViewMatrix()
    {
        RecalculateViewMatrix();
        return _viewMatrix;
    }
    
    internal byte[,,] GetViewMatrixBytes()
    {
        return _viewMatrixBytes;
    }
    
    internal byte[,,] GetProjectionMatrixBytes()
    {
        return _projectionMatrixBytes;
    }

    internal Matrix4 GetProjectionMatrix()
    {
        if (_isDirtyProjectionMatrix) RecalculateProjectionMatrix();
        return _projectionMatrix;
    }

    internal void SetAspectRatio(float newAspectRatio)
    {
        _aspectRatio = newAspectRatio;
        _isDirtyProjectionMatrix = true;
    }

    private void RecalculateProjectionMatrix()
    {
        _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_fov), _aspectRatio, _zNear, _zFar);
        for (var i = 0; i < 4; i++)
        for (var j = 0; j < 4; j++)
        {
            _converter.Single = _projectionMatrix[i, j];
            _projectionMatrixBytes[i, j, 0] = _converter.Byte0;
            _projectionMatrixBytes[i, j, 1] = _converter.Byte1;
            _projectionMatrixBytes[i, j, 2] = _converter.Byte2;
            _projectionMatrixBytes[i, j, 3] = _converter.Byte3;
        }
        _isDirtyProjectionMatrix = false;
    }

    private void RecalculateViewMatrix()
    {
        UpdateVectors();
        _viewMatrix = Matrix4.LookAt(Position, Position + _front, _up);
        for (var i = 0; i < 4; i++)
        for (var j = 0; j < 4; j++)
        {
            _converter.Single = _viewMatrix[i, j];
            _viewMatrixBytes[i, j, 0] = _converter.Byte0;
            _viewMatrixBytes[i, j, 1] = _converter.Byte1;
            _viewMatrixBytes[i, j, 2] = _converter.Byte2;
            _viewMatrixBytes[i, j, 3] = _converter.Byte3;
        }
    }
}