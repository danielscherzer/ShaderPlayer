using System;
using System.Numerics;
using Veldrid;

namespace ShaderPlayer
{
	class ShaderViewModel : IDisposable
	{
		public ShaderViewModel()
		{
			var sourceCode = @"void main() {
					vec2 uv = gl_FragCoord.xy / iResolution;
					gl_FragColor = vec4(uv, abs(sin(iGlobalTime)), 1.0);
				}";
			sourceCode = GlslTools.MakeConformal(sourceCode);

			var framebuffer = IoC.Resolve<GraphicsDevice>().SwapchainFramebuffer;
			//ResolutionX = 
			shaderVisual = new ShaderVisual(framebuffer.Width, framebuffer.Height, sourceCode);
		}

		//public uint ResolutionX { get; private set; }

		public void Resize(uint width, uint height)
		{
			var sourceCode = shaderVisual.SourceCode;
			shaderVisual.Dispose();
			//TODO: check for real size configuration
			shaderVisual = new ShaderVisual(width, height, sourceCode);
		}

		public void Draw(CommandList commandList, Vector2 mousePos, float time)
		{
			shaderVisual.Update(mousePos, time);
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
				var sourceCode = ShaderFileTools.ShaderFileToSourceCode(shaderFileName, graphicsDevice.ResourceFactory);
				var newShaderVisual = new ShaderVisual(shaderVisual.Width, shaderVisual.Height, sourceCode);
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

		private ShaderVisual shaderVisual;
	}
}
