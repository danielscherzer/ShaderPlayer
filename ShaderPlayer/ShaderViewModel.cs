using System;
using System.Numerics;
using Veldrid;

namespace ShaderPlayer
{
	class ShaderViewModel : IDisposable
	{
		public ShaderViewModel()
		{
			var framebuffer = IoC.Resolve<GraphicsDevice>().SwapchainFramebuffer;
			sourceCode = @"void main() {
					vec2 uv = gl_FragCoord.xy / iResolution;
					gl_FragColor = vec4(uv, abs(sin(iGlobalTime)), 1.0);
				}";
			sourceCode = GlslTools.MakeConformal(sourceCode);
			shaderVisual = new ShaderVisual(framebuffer.Width, framebuffer.Height, sourceCode);
		}

		public void Resize(uint width, uint height)
		{
			shaderVisual.Dispose();
			shaderVisual = new ShaderVisual(width, height, sourceCode);
		}

		public void Draw(CommandList commandList)
		{
			shaderVisual.Draw(commandList);
		}

		public void Dispose()
		{
			shaderVisual.Dispose();
		}

		public string Load(string shaderFileName)
		{
			try
			{
				var graphicsDevice = IoC.Resolve<GraphicsDevice>();
				sourceCode = ShaderFileTools.ShaderFileToSourceCode(shaderFileName, graphicsDevice.ResourceFactory);
				var newShaderVisual = new ShaderVisual(graphicsDevice.SwapchainFramebuffer.Width, graphicsDevice.SwapchainFramebuffer.Height, sourceCode);
				shaderVisual.Dispose();
				shaderVisual = newShaderVisual;
			}
			catch (ShaderIncludeException siex)
			{
				return $"{siex.Message} with\n{siex.InnerException.Message}";
			}
			catch (VeldridException vex)
			{
				return vex.Message;
			}
			return string.Empty;
		}

		internal void Update(Vector2 mousePos, float time)
		{
			shaderVisual.Update(mousePos, time);
		}

		private string sourceCode;
		private ShaderVisual shaderVisual;
	}
}
