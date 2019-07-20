using Jot;
using Jot.Storage;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.StartupUtilities;

namespace ShaderPlayer
{
	partial class Program
	{
		[DllImport("User32.dll")]
		public static extern bool SetProcessDPIAware();

		static void Main(string[] args)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				SetProcessDPIAware(); // no DPI scaling -> no blurry fonts
			}
			var tracker = new Tracker(new JsonFileStore("./"));
			tracker.Configure<Veldrid.Sdl2.Sdl2Window>().Id(w => nameof(ShaderPlayer))
				.Property(w => w.X, 200)
				.Property(w => w.Y, 60)
				.Property(w => w.Width, 1024)
				.Property(w => w.Height, 1024)
				.WhenPersistingProperty((wnd, property) => property.Cancel = WindowState.Normal != wnd.WindowState)
				.PersistOn(nameof(Veldrid.Sdl2.Sdl2Window.Closing));

			Veldrid.Sdl2.Sdl2Window window = VeldridStartup.CreateWindow(new WindowCreateInfo(200, 60, 1024, 1024, WindowState.Normal, "CG Exercise"));
			tracker.Track(window);

			var options = new GraphicsDeviceOptions() { PreferStandardClipSpaceYDirection = true, SyncToVerticalBlank = true };
			var graphicsDevice = VeldridStartup.CreateDefaultOpenGLGraphicsDevice(options, window, GraphicsBackend.OpenGL);
			//var graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options, GraphicsBackend.OpenGL);
			//var graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options);
			//var graphicsDevice = VeldridStartup.CreateVulkanGraphicsDevice(options, window);

			var myGui = new MyGui(window, graphicsDevice);

			window.Resized += () => graphicsDevice.ResizeMainWindow((uint)window.Width, (uint)window.Height);

			string fragmentShaderSourceCode = "#version 330\n" + PredefinedUniforms.ShaderString + "\n" +
				@"out vec4 fragColor;
				//in vec2 uv;
				void main()
				{
					vec2 uv = gl_FragCoord.xy / iResolution;
					fragColor = vec4(uv, abs(sin(iGlobalTime)), 1.0);
				}";

			var shaderQuad = new PrimitiveShaderQuad(graphicsDevice, fragmentShaderSourceCode);

			IDisposable fileChangeSubscription = null;

			window.DragDrop += (dropEvent) =>
			{
				fileChangeSubscription?.Dispose();
				fileChangeSubscription = TrackedFile.Load(dropEvent.File).Subscribe(
					fileName =>
					{
						myGui.ShowErrorInfo(string.Empty);
						try
						{
							shaderQuad.Load(ShaderFileTools.ShaderFileToSourceCode(fileName, graphicsDevice.ResourceFactory));
						}
						catch (VeldridException vex)
						{
							myGui.ShowErrorInfo(vex.Message);
						}
						window.Title = fileName;
					});
			};

			var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
			var stopwatch = Stopwatch.StartNew();
			var lastTime = 0f;
			while (window.Exists)
			{
				var time = (float)stopwatch.Elapsed.TotalSeconds;
				var deltaTime = time - lastTime;
				lastTime = time;

				var viewport = myGui.Viewport;
				PredefinedUniforms uniforms = new PredefinedUniforms { /*mouse = new Vector4(), */Time = time, Resolution = new Vector2(viewport.Width, viewport.Height) };
				shaderQuad.Update(uniforms);

				commandList.Begin();
				commandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
				commandList.SetViewport(0, viewport);

				shaderQuad.Draw(commandList);

				commandList.SetFullViewport(0);
				myGui.Render(commandList);

				commandList.End();
				graphicsDevice.SubmitCommands(commandList);
				graphicsDevice.SwapBuffers();
				InputSnapshot snapshot = window.PumpEvents();
				myGui.Update(deltaTime, snapshot); // Feed the input events to the ui
			}

			fileChangeSubscription?.Dispose();
			graphicsDevice.WaitForIdle();
			myGui.Dispose();
			shaderQuad.Dispose();
			commandList.Dispose();
			graphicsDevice.Dispose();
		}
	}
}
