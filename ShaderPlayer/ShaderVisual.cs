using System;
using System.Numerics;
using Veldrid;

namespace ShaderPlayer
{
	class ShaderVisual : IDisposable
	{
		public ShaderVisual(uint width, uint height, string sourceCode)
		{
			graphicsDevice = IoC.Resolve<GraphicsDevice>();
			renderSurface = new RenderSurface(graphicsDevice.ResourceFactory, width, height);
			shaderQuad = new PrimitiveShaderQuad(null, sourceCode, renderSurface.FrameBuffer);
			const string copyShaderSourceCode = @"in vec2 uv;
					void main() {
					vec4 color = texture(texInput, uv);
					gl_FragColor = color;
				}";
			shaderQuadCopy = new PrimitiveShaderQuad(renderSurface.View, GlslTools.MakeConformal(copyShaderSourceCode), graphicsDevice.SwapchainFramebuffer);
		}

		public void Draw(CommandList commandList)
		{
			shaderQuad.Draw(commandList);
			shaderQuadCopy.Draw(commandList);
		}

		public void Dispose()
		{
			shaderQuadCopy.Dispose();
			shaderQuad.Dispose();
			renderSurface.Dispose();
		}

		internal void Update(Vector2 mousePos, float time)
		{
			PredefinedUniforms uniforms = new PredefinedUniforms { Mouse = new Vector4(mousePos, 0f, 0f), Time = time, Resolution = new Vector2(renderSurface.FrameBuffer.Width, renderSurface.FrameBuffer.Height) };
			shaderQuad.Update(uniforms);
			uniforms.Resolution = new Vector2(graphicsDevice.SwapchainFramebuffer.Width, graphicsDevice.SwapchainFramebuffer.Height);
			shaderQuadCopy.Update(uniforms);
		}

		private readonly RenderSurface renderSurface;
		private readonly PrimitiveShaderQuad shaderQuad;
		private readonly PrimitiveShaderQuad shaderQuadCopy;
		private readonly GraphicsDevice graphicsDevice;
	}
}
