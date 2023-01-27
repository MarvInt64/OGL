using System.Numerics;

namespace OGL.Core
{
    public static class Vector3Extensions
    {
        public static (byte r, byte g, byte b) ToRgb(this Vector3 v)
        {
            // normals can have a range from -1 to +1
            // but RGB values are positive [0-255]
            // Example: nX -1.0f
            // -1.0f + 1 = 0 * 0.5f * 255 = 0
            //
            // for nX = 1.0f
            // 1.0f + 1 = 2 * 0.5f = 1 * 255 = 255
            
            // byte r = (byte)((v.X + 1) * 0.5f * 255);
            // byte g = (byte)((v.Y + 1) * 0.5f * 255);
            // byte b = (byte)((v.Z + 1) * 0.5f * 255);
            
            byte r = (byte)((v.X + 1) * 0.5f * 255);
            byte g = (byte)((v.Y + 1) * 0.5f * 255);
            byte b = (byte)((v.Z + 1) * 0.5f * 255);

            return (r, g, b);
        }
        
        public static Vector3 FromRgb(byte r, byte g , byte b)
        {
            return new Vector3(r / 255.0f * 2 - 1.0f, g / 255.0f * 2 - 1.0f, b / 255.0f * 2 - 1.0f);
        }
    }
}