using System.Numerics;

namespace OGL
{
    public class Camera
    {
        public Vector3 CameraPosition { get; set; } = new Vector3(20.0f, 15.0f, 20.0f);
        public Vector3 CameraFront { get; set; }= new Vector3(0.0f, 0.0f, -1.0f);
        public Vector3 CameraUp { get; set; }= Vector3.UnitY;
        public Vector3 CameraDirection { get; set; } = Vector3.Zero;
        public float CameraYaw { get; set; }= -90f;
        public float CameraPitch { get; set; }= 0f;
        public float CameraZoom { get; set; }= 45f;
    }
}