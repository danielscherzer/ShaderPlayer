using ImGuiNET;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace ShaderPlayer
{
	internal class MyGui : IDisposable
	{
		public MyGui(Sdl2Window window, GraphicsDevice graphicsDevice, Input input)
		{
			this.window = window;
			this.graphicsDevice = graphicsDevice;
			this.input = input;
			imGuiRenderer = new ImGuiRenderer(graphicsDevice, graphicsDevice.MainSwapchain.Framebuffer.OutputDescription, window.Width, window.Height);
			var style = ImGui.GetStyle();
			style.FrameBorderSize = 1f;
			style.WindowBorderSize = 3f;

			var io = ImGui.GetIO();
			io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
			io.ConfigWindowsResizeFromEdges = true;
			var fonts = io.Fonts;
			//fonts.ClearFonts();
			//var config = new ImFontConfig
			//{
			//	OversampleH = 2,
			//	OversampleV = 2,
			//};
			//IntPtr unmanagedAddr = Marshal.AllocHGlobal(Marshal.SizeOf(config));
			//Marshal.StructureToPtr(config, unmanagedAddr, true);

			font = fonts.AddFontFromFileTTF("Content/DroidSans.ttf", 32);
			//font.ConfigDataCount = 1;
			//var i = font.ConfigDataCount;
			//font = fonts.AddFontFromFileTTF(@"D:\arial.ttf", 26, new ImFontConfigPtr(unmanagedAddr));
			//io.FontDefault = font;
			imGuiRenderer.RecreateFontDeviceTexture();

			window.Resized += () =>
			{
				imGuiRenderer.WindowResized(window.Width, window.Height);
			};
			Viewport = new Viewport(0f, 0f, window.Width, window.Height, 0f, 1f);

			input.Commands.Add(new Command("Toggle Dashboard", () => showDashboardWindow = !showDashboardWindow, Key.D));
			input.Commands.Add(new Command("Toggle Stats", () => showStatsWindow = !showStatsWindow, Key.S));
			input.Commands.Add(new Command("Toggle Fullscreen", () =>
				window.WindowState = WindowState.BorderlessFullScreen == window.WindowState ? WindowState.Normal : WindowState.BorderlessFullScreen, Key.F11));
			input.Commands.Add(new Command("Close", window.Close, Key.Escape));
		}

		public Viewport Viewport { get; private set; }

		internal void ShowErrorInfo(string message)
		{
			errorMessage = message;
		}

		private readonly ImFontPtr font;
		private readonly Sdl2Window window;
		private readonly GraphicsDevice graphicsDevice;
		private readonly Input input;
		private ImGuiRenderer imGuiRenderer;

		private bool showDashboardWindow = false;
		private bool showDemoWindow = false;
		private bool showStatsWindow = true;
		private string errorMessage;

		private void MenuItemFromCommand(Command command)
		{
			if (ImGui.MenuItem(command.Caption, command.Key.ToString()))
			{
				command.Execute();
			}
		}

		private void SubmitUI()
		{
			Viewport = new Viewport(0f, 0f, window.Width, window.Height, 0f, 1f);
			if (WindowState.BorderlessFullScreen == window.WindowState) return;
			ImGui.PushFont(font);
			{
				if(ImGui.BeginMainMenuBar())
				{
					if (ImGui.BeginMenu("Window"))
					{
						foreach(var command in input.Commands)
						{
							MenuItemFromCommand(command);
						}

						ImGui.MenuItem("IMGUI Demo", "", ref showDemoWindow);
						ImGui.EndMenu();
					}
					var clientStart = ImGui.GetWindowHeight();
					Viewport = new Viewport(0f, clientStart, window.Width, window.Height - clientStart, 0f, 1f);
					ImGui.EndMainMenuBar();
				}
				if(!string.IsNullOrEmpty(errorMessage))
				{
					ImGui.Begin("Shader Log");
					ImGui.TextWrapped(errorMessage);
					ImGui.End();
				}
				if(showDashboardWindow && ImGui.Begin("Dashboard", ref showDashboardWindow))
				{
					ImGuiIOPtr io = ImGui.GetIO();
					ImGui.SliderFloat("Font scale", ref io.FontGlobalScale, 0.2f, 2f);
					//ImGui.Text("Hello, world!");
					//ImGui.ColorEdit4("color", ref color);
					//if (ImGui.Button("Button")) _counter++;
					//ImGui.SameLine(0, -1);
					//ImGui.Text($"counter = {_counter}");
					//ImGui.DragInt("Draggable Int", ref _dragInt);
					ImGui.End();
				}
				if(showStatsWindow) ShowStatsWindow(ref showStatsWindow);
				if(showDemoWindow) ImGui.ShowDemoWindow(ref showDemoWindow);
			}
			ImGui.PopFont();
		}

		void ShowStatsWindow(ref bool open)
		{
			var io = ImGui.GetIO();
			ImGui.SetNextWindowPos(io.DisplaySize - new Vector2(10f, 10f), 0, new Vector2(1f, 1f));
			ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background
			if (ImGui.Begin(" ", ref open, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize))
			{
				ImGui.Text($"Viewport: {Viewport.Width}x{Viewport.Height}");
				ImGui.Text($"Mouse: {ImGui.GetMousePos()}");
				float framerate = ImGui.GetIO().Framerate;
				ImGui.Text($"{1000.0f / framerate:0.##} ms");
				ImGui.Text($"{framerate:0} FPS");
			}
			ImGui.End();
		}

		internal void Update(float deltaTime)
		{
			imGuiRenderer.Update(deltaTime, input.Snapshot);
			SubmitUI();
		}

		internal void Render(CommandList commandList)
		{
			imGuiRenderer.Render(graphicsDevice, commandList);
		}

		public void Dispose()
		{
			imGuiRenderer.Dispose();
		}
	}
}
