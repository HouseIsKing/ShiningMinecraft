using System.Numerics;
using OpenTK.Mathematics;
using Vector3 = System.Numerics.Vector3;

namespace MinecraftClient.Entities.Player;

public class Camera
{
    private Vector3 Front;
    private Vector3 Up;
    private Vector3 Right;
    private float Fov;
    private float AspectRatio;
    private Matrix4x4 ViewMatrix;
    private Matrix4x4 ProjectionMatrix;
    private bool IsDirtyProjectionMatrix;
    private float PrevYaw;
    private float PrevPitch;
    private float ZNear;
    private float ZFar;

    public float Pitch;
    public float Yaw;
    public Vector3 Position;
    
    private static readonly Camera Instance = new Camera(new Vector3(0, 0, 0), 1280.0f/720.0f);
    
    public static Camera GetInstance()
    {
        return Instance;
    }

    private Camera(Vector3 position, float aspectRatio)
    {
        Position = position;
        AspectRatio = aspectRatio;
        Fov = 70;
        ZNear = 0.05f;
        ZFar = 1000;
        IsDirtyProjectionMatrix = true;
        PrevYaw = Yaw = 0;
        PrevPitch = Pitch = 0;
        UpdateVectors();
    }
    
    private void UpdateVectors()
    {
        Front.X = (float)(Math.Cos(MathHelper.DegreesToRadians(Yaw)) * Math.Cos(MathHelper.DegreesToRadians(Pitch)));
        Front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Pitch));
        Front.Z = (float)(Math.Sin(MathHelper.DegreesToRadians(Yaw)) * Math.Cos(MathHelper.DegreesToRadians(Pitch)));
        Front = Vector3.Normalize(Front);
        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }

    public Frustum GetFrustum()
    {
        return new Frustum(ViewMatrix, ProjectionMatrix);
    }

    public void SetFov(float fov)
    {
        Fov = fov;
        IsDirtyProjectionMatrix = true;
    }

    public Vector3 GetFrontVector()
    {
        return Front;
    }
    
    public Matrix4x4 GetViewMatrix()
    {
        RecalculateViewMatrix();
        return ViewMatrix;
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        if (IsDirtyProjectionMatrix)
        {
            RecalculateProjectionMatrix();
        }

        return ProjectionMatrix;
    }

    public void SetAspectRatio(float newAspectRatio)
    {
        AspectRatio = newAspectRatio;
        IsDirtyProjectionMatrix = true;
    }

    private void RecalculateProjectionMatrix()
    {
        ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), AspectRatio, ZNear, ZFar);
        IsDirtyProjectionMatrix = false;
    }

    private void RecalculateViewMatrix()
    {
        PrevYaw = Yaw;
        PrevPitch = Pitch;
        UpdateVectors();
        ViewMatrix = Matrix4x4.CreateLookAt(Position, Position + Front, Up);
    }
}