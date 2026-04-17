using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace SimpleOpenGL
{
    class Game : GameWindow
    {
        // Vertex data: position (x, y) + color (r, g, b) for each vertex
        private readonly float[] _vertices =
        {
            // Position       // Color
             
            0.5f, -0.5f,    0.0f, 0.0f, 1.0f,   // Right  - Blue

            -0.5f, -0.5f,    0.0f, 1.0f, 0.0f,   // Left   - Green
             
              0.0f,  0.5f,    1.0f, 0.0f, 0.0f,   // Top    - Red
        };

        private int _vao;   // Vertex Array Object
        private int _vbo;   // Vertex Buffer Object
        private int _shaderProgram;

        // Vertex Shader: positions each vertex
        private const string VertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec2 aPosition;
            layout(location = 1) in vec3 aColor;
            out vec3 vColor;
            void main()
            {
                gl_Position = vec4(aPosition, 0.0, 1.0);
                vColor = aColor;
            }
        ";

        // Fragment Shader: colors each pixel
        private const string FragmentShaderSource = @"
            #version 330 core
            in vec3 vColor;
            out vec4 FragColor;
            void main()
            {
                FragColor = vec4(vColor, 1.0);
            }
        ";

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings) { }

        // Called once when the window opens
        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f); // Dark background

            // --- Build and compile shaders ---
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, VertexShaderSource);
            GL.CompileShader(vertexShader);
            CheckShaderCompile(vertexShader, "Vertex");

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, FragmentShaderSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompile(fragmentShader, "Fragment");

            //int geometryShader = GL.CreateShader(ShaderType.GeometryShader);
            //GL.ShaderSource(geometryShader, FragmentShaderSource);
            //GL.CompileShader(geometryShader);
            //CheckShaderCompile(geometryShader, "geometryShader");

            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            //GL.AttachShader(_shaderProgram, geometryShader);
            GL.LinkProgram(_shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            //            GL.DeleteShader(geometryShader);
            // --- Upload vertex data to the GPU ---
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float),
                          _vertices, BufferUsageHint.StaticDraw);

            int stride = 5 * sizeof(float); // 2 floats position + 3 floats color

            // Position attribute (location = 0)
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            // Color attribute (location = 1)
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
        }

        // Called every frame — update game logic here
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
                Close();
        }

        // Called every frame — render here
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            //GL.DrawArrays(PrimitiveType.Triangles, 2, 7);

            //GL.DrawArrays(PrimitiveType.Triangles, 3, 8);

            //GL.DrawArrays(PrimitiveType.Triangles, 4, 9);


            SwapBuffers(); // Show the rendered frame
        }

        // Called when the window is resized
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }

        // Cleanup GPU resources when closing
        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteProgram(_shaderProgram);
        }

        private static void CheckShaderCompile(int shader, string type)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string log = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"[ERROR] {type} Shader compilation failed:\n{log}");
            }
        }


       
        static void Main()
        {
            var nativeSettings = new NativeWindowSettings
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Simple OpenGL Triangle",
                Profile = ContextProfile.Any,
                APIVersion = new Version(3, 3),
            };

            using var game = new Game(GameWindowSettings.Default, nativeSettings);
            game.Run();
        }
    }
}
