using System.Numerics;

namespace OGL
{
    public class Camera
    {
        public Vector3 Position { get; set; } = new Vector3(50.0f, 55.0f, 70.0f);
        public Vector3 Front { get; set; }= new Vector3(0.0f, 0.0f, -1.0f);
        public Vector3 Up { get; set; }= Vector3.UnitY;
        public Vector3 Direction { get; set; } = Vector3.Zero;
        public float Yaw { get; set; }= -90f;
        public float Pitch { get; set; }= 0f;
        public float Zoom { get; set; }= 45f;
    }
}