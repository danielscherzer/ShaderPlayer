using System;
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

			Veldrid.Sdl2.Sdl2Window window = VeldridStartup.CreateWindow(new WindowCreateInfo(200, 60, 1024, 1024, WindowState.Normal, nameof(ShaderPlayer)));
			IoC.RegisterInstance(window);
			var mainViewModel = new MainViewModel();

			var tracker = Persist.CreateTracker(window, mainViewModel);

			var options = new GraphicsDeviceOptions() { PreferStandardClipSpaceYDirection = true, SyncToVerticalBlank = true };
			var graphicsDevice = VeldridStartup.CreateDefaultOpenGLGraphicsDevice(options, window, GraphicsBackend.OpenGL);
			//var graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options, GraphicsBackend.OpenGL);
			//var graphicsDevice = VeldridStartup.CreateGraphicsDevice(window, options);
			//var graphicsDevice = VeldridStartup.CreateVulkanGraphicsDevice(options, window);

			window.Resized += () => graphicsDevice.ResizeMainWindow((uint)window.Width, (uint)window.Height);
			IoC.RegisterInstance(graphicsDevice);

			var inputTracker = new InputTracker();
			var myGui = new MyGui(mainViewModel);

			var viewModel = new ShaderViewModel();
			window.Resized += () => viewModel.Resize((uint)window.Width, (uint)window.Height);

			var taskService = new TaskService();
			IoC.RegisterInstance(taskService);

			IDisposable? fileChangeSubscription = null;

			void LoadShader(string shaderFileName)
			{
				fileChangeSubscription?.Dispose();
				fileChangeSubscription = TrackedFileObservable.DelayedLoad(shaderFileName).Subscribe(
					fileName =>
					{
						taskService.AddTask(() => mainViewModel.ErrorMessage = viewModel.Load(fileName));
						//myGui.ShowErrorInfo(viewModel.Load(fileName));
						window.Title = fileName;
					});
			}
			window.DragDrop += (dropEvent) => LoadShader(dropEvent.File);
			//LoadShader(@"D:\Daten\git\SHADER\2D\PatternCircle.glsl");

			var guiVisual = new GuiVisual();
			var commandList = graphicsDevice.ResourceFactory.CreateCommandList();
			//IoC.RegisterInstance(commandList);
			var time = new Time();
			while (window.Exists)
			{
				time.Update();
				var inputSnapShot = window.PumpEvents();
				inputTracker.UpdateFrameInput(inputSnapShot, window);
				myGui.CommandBindings.Execute(inputTracker);

				var viewport = myGui.Viewport;
				var mousePos = inputSnapShot.MousePosition;
				mousePos.Y = viewport.Height - mousePos.Y - 1;

				commandList.Begin();
					viewModel.Draw(commandList, mousePos, time.Total);
					guiVisual.BeginDraw(time.FrameDelta, inputSnapShot);
						myGui.Submit();
					guiVisual.EndDraw(commandList);
				commandList.End();
				graphicsDevice.SubmitCommands(commandList);
				graphicsDevice.SwapBuffers();
				taskService.ProcessNextTask();
			}

			guiVisual.Dispose();
			fileChangeSubscription?.Dispose();
			graphicsDevice.WaitForIdle();
			viewModel.Dispose();
			commandList.Dispose();
			graphicsDevice.Dispose();
		}
	}
}
