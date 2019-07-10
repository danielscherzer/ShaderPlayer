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
			try
			{
				SetProcessDPIAware(); // no DPI scaling with blurry fonts
			}
			catch { };
			var window = VeldridStartup.CreateWindow(new WindowCreateInfo(200, 60, 1024, 1024, WindowState.Normal, "CG Exercise"));
			var options = new GraphicsDeviceOptions() { PreferStandardClipSpaceYDirection = true, SyncToVerticalBlank = true };
			var graphicsDevice = VeldridStartup.CreateDefaultOpenGLGraphicsDevice(options, window, GraphicsBackend.OpenGL);
			//var graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options);
			//var graphicsDevice = VeldridStartup.CreateVulkanGraphicsDevice(options, window);

			var myGui = new MyGui(window, graphicsDevice);

			window.Resized += () => graphicsDevice.ResizeMainWindow((uint)window.Width, (uint)window.Height);

			const string fragmentShaderSourceCode = @"
				#version 330

				uniform uniforms
				{
					vec2 iResolution;
					float iGlobalTime;
				};
				out vec4 fragColor;

				//in vec2 uv;
				void main()
				{
					vec2 uv = gl_FragCoord.xy / iResolution;
					fragColor = vec4(uv, abs(sin(iGlobalTime)), 1.0);
				}";

			var shaderQuad = new PrimitiveShaderQuad(graphicsDevice, fragmentShaderSourceCode);

			IDisposable fileChangeSubscription = null;

			var commandList = graphicsDevice.ResourceFactory.CreateCommandList();

			window.DragDrop += (dropEvent) =>
			{
				try
				{
					fileChangeSubscription?.Dispose();
					fileChangeSubscription = TrackedFile.Load(dropEvent.File,
						fileName => shaderQuad.Load(ShaderFileTools.ShaderFileToSourceCode(fileName, graphicsDevice.ResourceFactory)));
				}
				catch
				{

				}
			};

			var stopwatch = Stopwatch.StartNew();
			var lastTime = 0f;
			while (window.Exists)
			{
				var time = (float)stopwatch.Elapsed.TotalSeconds;
				var deltaTime = time - lastTime;
				lastTime = time;

				var viewport = myGui.Viewport;
				Uniforms uniforms = new Uniforms { time = time, resolution = new Vector2(viewport.Width, viewport.Height) };
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
