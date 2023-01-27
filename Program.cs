using System;
using System.Linq;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace OGL
{
    class Program
    {
        private static IWindow _window;
        private static GL _gl;
        private static IKeyboard _primaryKeyboard;

        private const int Width = 2560;
        private const int Height = 1440;

        private static BufferObject<float> _vbo;
        private static BufferObject<uint> _ebo;
        private static VertexArrayObject<float, uint> _vao;
        private static Texture _texture;
        private static Shader _shader;

        //Setup the camera's location, directions, and movement speed
        private static Vector3 _cameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
        private static Vector3 _cameraFront = new Vector3(0.0f, 0.0f, -1.0f);
        private static Vector3 _cameraUp = Vector3.UnitY;
        private static Vector3 _cameraDirection = Vector3.Zero;
        private static float _cameraYaw = -90f;
        private static float _cameraPitch = 0f;
        private static float _cameraZoom = 45f;

        //Used to track change in mouse movement to allow for moving of the Camera
        private static Vector2 _lastMousePosition;

        private static readonly float[] Vertices =
        {
            //X    Y      Z     U   V
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,

            -0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  1.0f, 1.0f,

             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  0.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            -0.5f,  0.5f, -0.5f,  0.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 0.0f
        };

        private static readonly uint[] Indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private static Terrain _terrain;
        private static Shader _terrainShader;

        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(Width, Height);
            options.Title = "LearnOpenGL with Silk.NET";
            _window = Window.Create(options);

            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Closing += OnClose;

            _window.Run();
        }

        private static void OnLoad()
        {
            IInputContext input = _window.CreateInput();
            _primaryKeyboard = input.Keyboards.FirstOrDefault();
            if (_primaryKeyboard != null)
            {
                _primaryKeyboard.KeyDown += KeyDown;
            }
            for (int i = 0; i < input.Mice.Count; i++)
            {
                input.Mice[i].Cursor.CursorMode = CursorMode.Raw;
                input.Mice[i].MouseMove += OnMouseMove;
                input.Mice[i].Scroll += OnMouseWheel;
            }

            _gl = GL.GetApi(_window);

            _ebo = new BufferObject<uint>(_gl, Indices, BufferTargetARB.ElementArrayBuffer);
            _vbo = new BufferObject<float>(_gl, Vertices, BufferTargetARB.ArrayBuffer);
            _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);

            _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            _vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

            _shader = new Shader(_gl, "shader.vert", "shader.frag");
            _terrainShader = new Shader(_gl, "terrain.vert", "terrain.frag");
            _texture = new Texture(_gl, "silk.png");

            _terrain = new Terrain(_gl, 1024, 1024);
        }

        private static unsafe void OnUpdate(double deltaTime)
        {
            var moveSpeed = 14.5f * (float) deltaTime;

            if (_primaryKeyboard.IsKeyPressed(Key.W))
            {
                //Move forwards
                _cameraPosition += moveSpeed * _cameraFront;
            }
            if (_primaryKeyboard.IsKeyPressed(Key.S))
            {
                //Move backwards
                _cameraPosition -= moveSpeed * _cameraFront;
            }
            if (_primaryKeyboard.IsKeyPressed(Key.A))
            {
                //Move left
                _cameraPosition -= Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * moveSpeed;
            }
            if (_primaryKeyboard.IsKeyPressed(Key.D))
            {
                //Move right
                _cameraPosition += Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * moveSpeed;
            }
          
            // Console.WriteLine(_cameraPosition);
            // var tIndex = ((int)_cameraPosition.X + (int)_cameraPosition.Z * _terrain._width);
            // var tpos = tIndex <= _terrain._vertices.Count ? _terrain._vertices[tIndex].Position : _cameraPosition;
            // tpos.Y += 1.7f;
            //     _cameraPosition = new  Vector3(_cameraPosition.X, Vector3.Lerp(_cameraPosition, tpos, (float)(8.2f * deltaTime)).Y, _cameraPosition.Z);
         
        }

        private static unsafe void OnRender(double deltaTime)
        {
            _gl.Enable(EnableCap.DepthTest);
            _gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            _vao.Bind();
            _texture.Bind();
            _shader.Use();
            _shader.SetUniform("uTexture0", 0);

            //Use elapsed time to convert to radians to allow our cube to rotate over time
            var difference = (float) (_window.Time * 100);

            var model = Matrix4x4.Identity;
            var view = Matrix4x4.CreateLookAt(_cameraPosition, _cameraPosition + _cameraFront, _cameraUp);
            var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_cameraZoom), Width / Height, 0.1f, 1000.0f);

            _shader.SetUniform("uModel", model);
            _shader.SetUniform("uView", view);
            _shader.SetUniform("uProjection", projection);
           

            //We're drawing with just vertices and no indices, and it takes 36 vertices to have a six-sided textured cube
            _gl.DrawArrays(PrimitiveType.Triangles, 0, 36);

            _terrain.Bind();
            _terrainShader.Use();
            _terrainShader.SetUniform("uModel", model);
            _terrainShader.SetUniform("uView", view);
            _terrainShader.SetUniform("uProjection", projection);
            _terrainShader.SetUniform("uCameraPosition", _cameraPosition);
            _terrainShader.SetUniform("specular_coefficient", 0.2f);
            _terrainShader.SetUniform("shininess", 3.0f);
            _terrainShader.SetUniform("specular_color", new Vector3(0.992f, 0.984f, 0.827f));
            _terrain.Render();
        }

        private static unsafe void OnMouseMove(IMouse mouse, Vector2 position)
        {
            var lookSensitivity = 0.1f;
            if (_lastMousePosition == default) { _lastMousePosition = position; }
            else
            {
                var xOffset = (position.X - _lastMousePosition.X) * lookSensitivity;
                var yOffset = (position.Y - _lastMousePosition.Y) * lookSensitivity;
                _lastMousePosition = position;

                _cameraYaw += xOffset;
                _cameraPitch -= yOffset;

                //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
                _cameraPitch = Math.Clamp(_cameraPitch, -89.0f, 89.0f);

                _cameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(_cameraYaw)) * MathF.Cos(MathHelper.DegreesToRadians(_cameraPitch));
                _cameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(_cameraPitch));
                _cameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(_cameraYaw)) * MathF.Cos(MathHelper.DegreesToRadians(_cameraPitch));
                _cameraFront = Vector3.Normalize(_cameraDirection);
            }
        }

        private static unsafe void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
        {
            //We don't want to be able to zoom in too close or too far away so clamp to these values
            _cameraZoom = Math.Clamp(_cameraZoom - scrollWheel.Y, 1.0f, 45f);
        }

        private static void OnClose()
        {
            _vbo.Dispose();
            _ebo.Dispose();
            _vao.Dispose();
            _shader.Dispose();
            _texture.Dispose();
        }

        private static void KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            if (key == Key.Escape)
            {
                _window.Close();
            }
        }
    }
}
