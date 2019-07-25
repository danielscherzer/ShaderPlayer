using System;
using System.Numerics;
using Veldrid;

namespace ShaderPlayer
{
	class ShaderViewModel : IDisposable
	{
		public ShaderViewModel(GraphicsDevice graphicsDevice)
		{
			this.graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
			sourceCode = @"void main() {
					vec2 uv = gl_FragCoord.xy / iResolution;
					vec4 color = texture(texInput, uv);
					gl_FragColor = vec4(uv, abs(sin(iGlobalTime)), 1.0);
				}";
			sourceCode = GlslTools.MakeConformal(sourceCode);
			Resize(graphicsDevice.SwapchainFramebuffer.Width, graphicsDevice.SwapchainFramebuffer.Height);
		}

		public void Resize(uint width, uint height)
		{
			Dispose();
			//var shaderQuad = new PrimitiveShaderQuad(graphicsDevice, null, graphicsDevice.SwapchainFramebuffer);
			renderSurface = new RenderSurface(graphicsDevice.ResourceFactory, width, height);
			shaderQuad = new PrimitiveShaderQuad(graphicsDevice, null, sourceCode, renderSurface.FrameBuffer);
			const string copyShaderSourceCode = @"void main() {
					vec2 uv = gl_FragCoord.xy / iResolution;
					vec4 color = texture(texInput, uv);
					gl_FragColor = color;
				}";
			shaderQuadCopy = new PrimitiveShaderQuad(graphicsDevice, renderSurface.View, GlslTools.MakeConformal(copyShaderSourceCode), graphicsDevice.SwapchainFramebuffer);
		}

		public void Draw(CommandList commandList)
		{
			shaderQuad.Draw(commandList);
			shaderQuadCopy.Draw(commandList);
		}

		public void Dispose()
		{
			shaderQuadCopy?.Dispose();
			shaderQuad?.Dispose();
			renderSurface?.Dispose();
		}

		public string Load(string shaderFileName)
		{
			try
			{
				sourceCode = ShaderFileTools.ShaderFileToSourceCode(shaderFileName, graphicsDevice.ResourceFactory);
				//shaderQuad.Load(graphicsDevice.ResourceFactory, sourceCode);
				Resize(graphicsDevice.SwapchainFramebuffer.Width, graphicsDevice.SwapchainFramebuffer.Height);
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
			PredefinedUniforms uniforms = new PredefinedUniforms { Mouse = new Vector4(mousePos, 0f, 0f), Time = time, Resolution = new Vector2(renderSurface.FrameBuffer.Width, renderSurface.FrameBuffer.Height) };
			shaderQuad.Update(graphicsDevice, uniforms);
			uniforms.Resolution = new Vector2(graphicsDevice.SwapchainFramebuffer.Width, graphicsDevice.SwapchainFramebuffer.Height);
			shaderQuadCopy.Update(graphicsDevice, uniforms);
		}

		private RenderSurface renderSurface;
		private PrimitiveShaderQuad shaderQuad;
		private PrimitiveShaderQuad shaderQuadCopy;
		private string sourceCode;
		private readonly GraphicsDevice graphicsDevice;
	}
}
