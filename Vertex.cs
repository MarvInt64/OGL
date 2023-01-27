// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Numerics;

namespace OGL
{
    public class Vertex
    {
        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        
        public Vector2 Uv { get; set; }

        public float[] PositionArray()
        {
            return new[] { Position.X, Position.Y, Position.Z };
        }
        
        public Span<float> NormalArray()
        {
            return new[] { Normal.X, Normal.Y, Normal.Z };
        }

        public float[] ToArray()
        {
            return new[] { Position.X, Position.Y, Position.Z, Normal.X, Normal.Y, Normal.Z, Uv.X, Uv.Y };
        }
        
        public float[] NormalToArray()
        {
            return new[] { Position.X, Position.Y, Position.Z, Normal.X, Normal.Y, Normal.Z, Uv.X, Uv.Y };
        }
    }
}
