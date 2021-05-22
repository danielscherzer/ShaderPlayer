using ImGuiNET;
using System;
using Veldrid;
using Veldrid.Sdl2;

namespace ShaderPlayer
{
	class GuiVisual : IDisposable
	{
		public GuiVisual()
		{
			var window = IoC.Resolve<Sdl2Window>();
			graphicsDevice = IoC.Resolve<GraphicsDevice>();
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
		}

		private readonly ImFontPtr font;
		private readonly GraphicsDevice graphicsDevice;
		private ImGuiRenderer imGuiRenderer;

		internal void BeginDraw(float deltaTime, InputSnapshot inputSnapshot)
		{
			imGuiRenderer.Update(deltaTime, inputSnapshot);
			ImGui.PushFont(font);
		}

		internal void EndDraw(CommandList commandList)
		{
			ImGui.PopFont();
			imGuiRenderer.Render(graphicsDevice, commandList);
		}

		public void Dispose()
		{
			imGuiRenderer.Dispose();
		}
	}
}
