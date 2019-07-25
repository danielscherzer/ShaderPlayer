using System;
using Veldrid;

namespace ShaderPlayer
{
	class RenderSurface : IDisposable
	{
		public RenderSurface(ResourceFactory resourceFactory, uint resolutionX, uint resolutionY)
		{
			texture = resourceFactory.CreateTexture(TextureDescription.Texture2D(resolutionX, resolutionY, 1, 1,
				PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled));
			View = resourceFactory.CreateTextureView(texture);
			FrameBuffer = resourceFactory.CreateFramebuffer(new FramebufferDescription(null, texture));
		}

		public TextureView View { get; }

		public Framebuffer FrameBuffer { get; }

		public void Dispose()
		{
			FrameBuffer.Dispose();
			View.Dispose();
			texture.Dispose();
		}

		private readonly Texture texture;
	}
}
