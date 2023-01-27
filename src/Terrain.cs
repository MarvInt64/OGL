using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OGL.Core;
using SharpNoise.Modules;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OGL
{
    public class Terrain
    {
        
        private readonly GL _gl;
        public readonly int _width;
        public readonly int _height;
        //private List<float> _vertices; 
        //private List<float> _normals; 
        public List<Vertex> _vertices;
        private List<uint> _indices;
        private BufferObject<float> _vbo;
        private readonly BufferObject<uint> _ebo;
        private readonly uint _vaoHandle;
        private float _scale = 1.0f;

        private Texture _texture;
        
        public Terrain(GL gl, int width, int height)
        {
            unsafe
            {
                _gl = gl;
            
                _width = width;
                _height = height;

                CreateVertices();
                CreateIndices();

                _texture = new Texture(_gl, "Resources\\silk.png");
                
                //CalculateNormals();

               
                
                var img = Image.Load<Rgb24>(@"Resources\Luna\normalmap.png");
                
                img.ProcessPixelRows(accessor =>
                {
                    // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                    Rgba32 transparent = Color.Transparent;
                
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        Span<Rgb24> pixelRow = accessor.GetRowSpan(y);
                
                        // pixelRow.Length has the same value as accessor.Width,
                        // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                        for (int x = 0; x < pixelRow.Length; x++)
                        {
                            // Get a reference to the pixel at position x
                            ref Rgb24 pixel = ref pixelRow[x];
                            //var value = (((pixel.R + pixel.G + pixel.B) / 3) / 255.0f) * 1.0f;
                        
                            _vertices[x + y * accessor.Width].Normal = Vector3Extensions.FromRgb(pixel.R, pixel.B, pixel.G);
                        
                   
                        }
                    }
                });
                
                ImageHelper.SavePng("Resources\\test.png", _vertices.SelectMany(x =>
                {
                    // convert normals to RGB values and convert each RGB value to 
                    // a byte array
                    var rgb = x.Normal.ToRgb();
                    return new[] {rgb.r, rgb.b, rgb.g };
                }).ToArray(), _width, height);

                _vaoHandle  = _gl.GenVertexArray();
                _vbo = new BufferObject<float>(_gl, _vertices.SelectMany(x => x.ToArray()).ToArray(), BufferTargetARB.ArrayBuffer);
                _ebo = new BufferObject<uint>(gl, _indices.ToArray(), BufferTargetARB.ElementArrayBuffer);
            
                _gl.BindVertexArray(_vaoHandle);
                _vbo.Bind();
                _ebo.Bind();
                
                _gl.VertexAttribPointer(0, 3,  VertexAttribPointerType.Float, false, 8 * (uint) sizeof(float), (void*) (0 * sizeof(float)));
                _gl.VertexAttribPointer(1, 3,  VertexAttribPointerType.Float, false, 8 * (uint) sizeof(float), (void*) (3 * sizeof(float)));
                _gl.VertexAttribPointer(3, 2,  VertexAttribPointerType.Float, false, 8 * (uint) sizeof(float), (void*) (2 * sizeof(float)));
                _gl.EnableVertexAttribArray(0);
                _gl.EnableVertexAttribArray(1);
            }
        }

        private void CalculateNormals()
        {
            var _vertexCount = new List<int>(new int[_vertices.Count]);

            for (int i = 0; i < _indices.Count - _width; i += 3)
            {
                var index1 = _indices[i + 0];
                var index2 = _indices[i + 1];
                var index3 = _indices[i + 2];

                Vector3 v1 = _vertices[(int)index1].Position;
                Vector3 v2 = _vertices[(int)index2].Position;
                Vector3 v3 = _vertices[(int)index3].Position;

                //Vector3 normal = Vector3.Normalize(Vector3.Cross(v3 - v1, v2 - v1));
                Vector3 normal;

                if (Vector3.Dot(Vector3.Cross(v2 - v1, v3 - v1), Vector3.UnitY) < 0) // Check for clockwise winding
                {
                    normal = Vector3.Normalize(Vector3.Cross(v3 - v1, v2 - v1)); // Reverse order of vertices
                }
                else
                {
                    normal = Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));
                }

                _vertices[(int)index1].Normal += normal;
                _vertices[(int)index2].Normal += normal;
                _vertices[(int)index3].Normal += normal;

                _vertexCount[(int)index1]++;
                _vertexCount[(int)index2]++;
                _vertexCount[(int)index3]++;
            }

            for (int i = 0; i < _vertices.Count; i++)
            {
                _vertices[i].Normal = Vector3.Normalize(_vertices[i].Normal / _vertexCount[i]);
            }
        }

        private void CreateIndices()
        {
            // Creates an array of indices
            _indices = new List<uint>();

            AddIndicesAlternatingWinding();
            
        }

        private void AddIndicesAlternatingWinding()
        {
            // for (int y = 0; y < _height - 1; y++)
            // {
            //     if (y % 2 == 0) // even rows
            //     {
            //         for (int x = 0; x < _width; x++)
            //         {
            //             _indices.Add((uint)(x + y * _width));
            //             _indices.Add((uint)(x + (y + 1) * _width));
            //         }
            //
            //         _indices.Add((uint)(_width - 1 + y * _width));
            //     }
            //     else // odd rows
            //     {
            //         for (int x = _width - 1; x >= 0; x--)
            //         {
            //             _indices.Add((uint)(x + (y + 1) * _width));
            //             _indices.Add((uint)(x + y * _width));
            //         }
            //
            //         _indices.Add((uint)(y * _width));
            //     }
            // }


            for (int y = 0; y < _height - 1; y++) {
                if (y > 0) {
                    // Degenerate begin: repeat first vertex
                    _indices.Add((uint)(y * _height));
                }
 
                for (int x = 0; x < _width; x++) {
                    // One part of the strip
                    _indices.Add((uint)(y * _height + x));
                    _indices.Add((uint)((y + 1) * _height + x));
                }
 
                if (y < _height - 2) {
                    // Degenerate end: repeat last vertex
                    _indices.Add((uint)((y + 1) * _height + (_width - 1)));
                }
            }
        }

        private void CreateVertices()
        {
            
            
            _vertices = new List<Vertex>();

            Perlin perlin = new Perlin();
            perlin.Frequency = 2.0f;
            perlin.OctaveCount = 5;
            // sharpnoise can generate planetary surface!
            for (int y = 0; y < _height; ++y)
            {
                for (int x = 0; x < _width; ++x)
                {
                    float sampleX = (float)x/_width * 40.1f + 30;
                    float sampleY = (float)y/_height * 40.1f + 30;
                    float noiseValue = (float)perlin.GetValue(sampleX, 0, sampleY);
                    Vertex v = new Vertex
                    {
                        Position = new Vector3(x * _scale ,0, y * _scale),
                        Normal = new Vector3(0.0f, 0.0f, 0.0f)
                    };

                    //_vertices[x * 3 + y * _width * 3 + 0] = x;
                    //_vertices[x * 3 + y * _width * 3 + 1] = y;
                    //_vertices[x * 3 + y * _width * 3 + 2] = r;
                    _vertices.Add(v);
                }
            }
            
            
            var img = Image.Load<Rgb24>(@"Resources\Luna\heightmap.png");
            
            img.ProcessPixelRows(accessor =>
            {
                // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                Rgba32 transparent = Color.Transparent;

                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgb24> pixelRow = accessor.GetRowSpan(y);

                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        // Get a reference to the pixel at position x
                        ref Rgb24 pixel = ref pixelRow[x];
                        var value = (((pixel.R + pixel.G + pixel.B) / 3) / 255.0f) * 30.0f;
                        
                        _vertices[x + y * accessor.Height].Position += new Vector3(0.0f,  value, 0.0f);
                        
                   
                    }
                }
            });
            
        }

        public unsafe void Bind()
        {
            _gl.BindVertexArray(_vaoHandle);
        }

        public unsafe void Render()
        {
            //_gl.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Line);
            _gl.PolygonMode(MaterialFace.Front,PolygonMode.Fill);
            _gl.DrawElements(PrimitiveType.TriangleStrip, (uint)_indices.ToArray().Length, DrawElementsType.UnsignedInt, null);
        }
    }
}
