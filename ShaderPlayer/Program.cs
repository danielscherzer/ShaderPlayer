using Jot;
using Jot.Storage;
using System;
using System.Collections.Concurrent;
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

			Veldrid.Sdl2.Sdl2Window window = VeldridStartup.CreateWindow(new WindowCreateInfo(200, 60, 1024, 1024, WindowState.Normal, "CG Exercise"));
			var tracker = new Tracker(new JsonFileStore("./"));
			tracker.Configure<Veldrid.Sdl2.Sdl2Window>().Id(w => nameof(ShaderPlayer))
				.Property(w => w.X, 200)
				.Property(w => w.Y, 60)
				.Property(w => w.Width, 1024)
				.Property(w => w.Height, 1024)
				.WhenPersistingProperty((wnd, property) => property.Cancel = WindowState.Normal != wnd.WindowState)
				.PersistOn(nameof(Veldrid.Sdl2.Sdl2Window.Closing));
			tracker.Track(window);

			var options = new GraphicsDeviceOptions() { PreferStandardClipSpaceYDirection = true, SyncToVerticalBlank = true };
			var graphicsDevice = VeldridStartup.CreateDefaultOpenGLGraphicsDevice(options, window, GraphicsBackend.OpenGL);
			//var graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options, GraphicsBackend.OpenGL);
			//var graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options);
			//var graphicsDevice = VeldridStartup.CreateVulkanGraphicsDevice(options, window);

			var input = new Input();
			var myGui = new MyGui(window, graphicsDevice, input);

			var viewModel = new ShaderViewModel(graphicsDevice);

			window.Resized += () => graphicsDevice.ResizeMainWindow((uint)window.Width, (uint)window.Height);
			window.Resized += () => viewModel.Resize((uint)window.Width, (uint)window.Height);

			var tasks = new ConcurrentQueue<Action>();
			IDisposable fileChangeSubscription = null;

			void LoadShader(string shaderFileName)
			{
				fileChangeSubscription?.Dispose();
				fileChangeSubscription = TrackedFile.Load(shaderFileName).Subscribe(
					fileName =>
					{
						tasks.Enqueue(() => myGui.ShowErrorInfo(viewModel.Load(fileName)));
						//myGui.ShowErrorInfo(viewModel.Load(fileName));
						window.Title = fileName;
					});
			}
			window.DragDrop += (dropEvent) => LoadShader(dropEvent.File);
			LoadShader(@"D:\Daten\git\SHADER\2D\PatternCircle.glsl");

			var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
			var time = new Time();
			while (window.Exists)
			{
				time.Update();
				input.Update(window);
				myGui.Update(time.FrameDelta);

				var viewport = myGui.Viewport;
				var mousePos = input.Snapshot.MousePosition;
				mousePos.Y = viewport.Height - mousePos.Y - 1;
				viewModel.Update(mousePos, time.Total);

				commandList.Begin();
					viewModel.Draw(commandList);
					myGui.Render(commandList);
				commandList.End();
				graphicsDevice.SubmitCommands(commandList);
				graphicsDevice.SwapBuffers();
				if(tasks.TryDequeue(out var task))
				{
					task();
				}
			}

			fileChangeSubscription?.Dispose();
			graphicsDevice.WaitForIdle();
			myGui.Dispose();
			commandList.Dispose();
			graphicsDevice.Dispose();
		}
	}
}
