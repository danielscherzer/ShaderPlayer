using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;

namespace ShaderPlayer
{
	internal class PrimitiveShaderQuad : IDisposable
	{
		public PrimitiveShaderQuad(TextureView? inputTextureView, string shaderCode, Framebuffer output)
		{
			Output = output ?? throw new ArgumentNullException(nameof(output));

			graphicsDevice = IoC.Resolve<GraphicsDevice>();
			var resourceFactory = graphicsDevice.ResourceFactory;

			uniformsBuffer = resourceFactory.CreateBuffer(new BufferDescription(PredefinedUniforms.SizeInBytes, BufferUsage.UniformBuffer));

			List<ResourceLayoutElementDescription> layouts = new() 
			{ 
				new ResourceLayoutElementDescription(PredefinedUniforms.ResourceName, ResourceKind.UniformBuffer, ShaderStages.Fragment) 
			};
			List<BindableResource> resources = new() { uniformsBuffer };
			if(inputTextureView != null)
			{
				layouts.Add(new ResourceLayoutElementDescription("inputTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment));
				resources.Add(inputTextureView);
				layouts.Add(new ResourceLayoutElementDescription("inputTextureSampler", ResourceKind.Sampler, ShaderStages.Fragment));
				resources.Add(graphicsDevice.LinearSampler);
			}

			resourceLayout = resourceFactory.CreateResourceLayout(new ResourceLayoutDescription(layouts.ToArray()));

			resourceSet = resourceFactory.CreateResourceSet(new ResourceSetDescription(resourceLayout, resources.ToArray()));
			pipeline = CreatePipeline(resourceFactory, shaderCode);
		}

		public void Dispose()
		{
			uniformsBuffer.Dispose();
			resourceLayout.Dispose();
			resourceSet.Dispose();
			pipeline.Dispose();
		}

		internal void Draw(CommandList commandList)
		{
			commandList.SetFramebuffer(Output);
			commandList.SetFullViewports();
			commandList.SetPipeline(pipeline);
			commandList.SetGraphicsResourceSet(0, resourceSet);
			commandList.Draw(4);
		}

		internal void Update(PredefinedUniforms uniforms)
		{
			graphicsDevice.UpdateBuffer(uniformsBuffer, 0, uniforms);
		}

		private readonly GraphicsDevice graphicsDevice;
		private Framebuffer Output { get; }
		private readonly Pipeline pipeline;
		private readonly ResourceLayout resourceLayout;
		private readonly ResourceSet resourceSet;
		private readonly DeviceBuffer uniformsBuffer;

		private static Shader[] LoadShader(ResourceFactory resourceFactory, string fragmentShaderSourceCode)
		{
			const string vertexCode = @"#version 330
				out vec2 uv;

				const vec2 vertices[4] = vec2[4](
					vec2( -1.0, 1.0),
					vec2( 1.0,  1.0),
					vec2(-1.0, -1.0),
					vec2( 1.0, -1.0));

				void main()
				{
					vec2 pos = vertices[gl_VertexID];
					gl_Position = vec4(pos, 0, 1);
					uv = 0.5 + 0.5 * pos;
				}";

			var vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(vertexCode), "main");
			var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(fragmentShaderSourceCode), "main");
			//return resourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
			var vertexShader = resourceFactory.CreateShader(vertexShaderDesc);
			var fragmentShader = resourceFactory.CreateShader(fragmentShaderDesc);
			return new Shader[] { vertexShader, fragmentShader };
		}

		private Pipeline CreatePipeline(ResourceFactory resourceFactory, string fragmentShaderSourceCode)
		{
			var shaders = LoadShader(resourceFactory, fragmentShaderSourceCode);
			var pipelineDesc = new GraphicsPipelineDescription()
			{
				BlendState = BlendStateDescription.SingleDisabled,
				DepthStencilState = DepthStencilStateDescription.Disabled,
				RasterizerState = RasterizerStateDescription.CullNone,
				PrimitiveTopology = PrimitiveTopology.TriangleStrip,
				ResourceLayouts = new ResourceLayout[] { resourceLayout },
				ShaderSet = new ShaderSetDescription()
				{
					VertexLayouts = Array.Empty<VertexLayoutDescription>(),
					Shaders = shaders
				},
				Outputs = Output.OutputDescription
			};
			return resourceFactory.CreateGraphicsPipeline(pipelineDesc);
		}
	}
}
