using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace ShaderPlayer
{
	internal class MyGui
	{
		public MyGui(MainViewModel mainViewModel)
		{
			MainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

			window = IoC.Resolve<Sdl2Window>();
			Viewport = new Viewport(0f, 0f, window.Width, window.Height, 0f, 1f);

			commandCaption.Add(("Toggle Dashboard", new CommandBinding(mainViewModel.ToggleDashboardWindow, Key.D)));
			commandCaption.Add(("Toggle Stats", new CommandBinding(mainViewModel.ToggleStatsWindow, Key.S)));
			commandCaption.Add(("Toggle Fullscreen", new CommandBinding(() =>
				window.WindowState = WindowState.BorderlessFullScreen == window.WindowState ? WindowState.Normal : WindowState.BorderlessFullScreen, Key.F11)));
			commandCaption.Add(("Close", new CommandBinding(window.Close, Key.Escape)));
		}

		public IEnumerable<CommandBinding> CommandBindings => commandCaption.Select( cb => cb.commandBinding);

		public Viewport Viewport { get; private set; }

		private readonly Sdl2Window window;
		private readonly List<(string caption, CommandBinding commandBinding)> commandCaption = new List<(string caption, CommandBinding)>();

		public MainViewModel MainViewModel { get; }

		private bool showDemoWindow = false;

		private void MenuItemFromCommand(string caption, CommandBinding commandBinding)
		{
			if (ImGui.MenuItem(caption, commandBinding.Key.ToString()))
			{
				commandBinding.Execute();
			}
		}

		public void Submit()
		{
			Viewport = new Viewport(0f, 0f, window.Width, window.Height, 0f, 1f);
			if (WindowState.BorderlessFullScreen == window.WindowState) return;
			if (ImGui.IsMouseClicked(1))
			{
				ImGui.OpenPopup("pop-up");
			}
			if(ImGui.BeginPopup("pop-up"))
			{
				if (ImGui.BeginMenu("Window"))
				{
					foreach (var (caption, commandBinding) in commandCaption)
					{
						MenuItemFromCommand(caption, commandBinding);
					}

					ImGui.MenuItem("IMGUI Demo", "", ref showDemoWindow);
					ImGui.EndMenu();
				}
				ImGui.EndPopup();
			}

			if(!string.IsNullOrEmpty(MainViewModel.ErrorMessage))
			{
				ImGui.Begin("Shader Log");
				ImGui.TextWrapped(MainViewModel.ErrorMessage);
				ImGui.End();
			}

			var showDashboardWindow = MainViewModel.ShowDashboardWindow;
			if (showDashboardWindow && ImGui.Begin("Dashboard", ref showDashboardWindow))
			{
				ImGuiIOPtr io = ImGui.GetIO();
				ImGui.SliderFloat("Font scale", ref io.FontGlobalScale, 0.2f, 2f, "%.1f");
				//ImGui.Text("Hello, world!");
				//ImGui.ColorEdit4("color", ref color);
				//if (ImGui.Button("Button")) _counter++;
				//ImGui.SameLine(0, -1);
				//ImGui.Text($"counter = {_counter}");
				//ImGui.DragInt("Draggable Int", ref _dragInt);
				ImGui.End();
			}
			MainViewModel.ShowDashboardWindow = showDashboardWindow;

			var showStatsWindow = MainViewModel.ShowStatsWindow;
			if (showStatsWindow) ShowStatsWindow(ref showStatsWindow);
			MainViewModel.ShowStatsWindow = showStatsWindow;

			if (showDemoWindow) ImGui.ShowDemoWindow(ref showDemoWindow);
		}

		void ShowStatsWindow(ref bool open)
		{
			var io = ImGui.GetIO();
			ImGui.SetNextWindowPos(io.DisplaySize - new Vector2(10f, 10f), 0, new Vector2(1f, 1f));
			ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background
			if (ImGui.Begin(" ", ref open, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize))
			{
				ImGui.Text($"View port: {Viewport.Width}x{Viewport.Height}");
				ImGui.Text($"Mouse: {ImGui.GetMousePos()}");
				float framerate = ImGui.GetIO().Framerate;
				ImGui.Text($"{1000.0f / framerate:0.##} ms");
				ImGui.Text($"{framerate:0} FPS");
			}
			ImGui.End();
		}
	}
}
