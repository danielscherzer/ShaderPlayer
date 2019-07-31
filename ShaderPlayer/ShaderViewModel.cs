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
			var sourceCode = @"void main() {
					vec2 uv = gl_FragCoord.xy / iResolution;
					gl_FragColor = vec4(uv, abs(sin(iGlobalTime)), 1.0);
				}";
			sourceCode = GlslTools.MakeConformal(sourceCode);
			shaderVisual = new ShaderVisual(framebuffer.Width, framebuffer.Height, sourceCode);
		}

		public void Resize(uint width, uint height)
		{
			var sourceCode = shaderVisual.SourceCode;
			shaderVisual.Dispose();
			//TODO: check for real size configuration
			shaderVisual = new ShaderVisual(width, height, sourceCode);
		}

		public void Draw(CommandList commandList)
		{
			shaderVisual.Draw(commandList);

			if(newShaderVisual != null)
			{
				IoC.Resolve<TaskService>().AddTask(() =>
				{
					shaderVisual.Dispose();
					shaderVisual = newShaderVisual;
					newShaderVisual = null;
				});
			}
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
				newShaderVisual = new ShaderVisual(shaderVisual.Width, shaderVisual.Height, sourceCode);
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

		private ShaderVisual shaderVisual;
		private ShaderVisual newShaderVisual;
	}
}
