using System;
using System.Text;
using Veldrid;

namespace ShaderPlayer
{
	internal class PrimitiveShaderQuad : IDisposable
	{
		public PrimitiveShaderQuad(GraphicsDevice graphicsDevice, string fragmentShaderSourceCode, OutputDescription outputDescription)
		{
			this.graphicsDevice = graphicsDevice;
			this.outputDescription = outputDescription;
			uniformsBuffer = graphicsDevice.ResourceFactory.CreateBuffer(new BufferDescription(PredefinedUniforms.SizeInBytes, BufferUsage.UniformBuffer));

			resourceLayout = graphicsDevice.ResourceFactory.CreateResourceLayout(
				new ResourceLayoutDescription(PredefinedUniforms.ResourceLayoutElementDescription));

			resourceSet = graphicsDevice.ResourceFactory.CreateResourceSet(new ResourceSetDescription(resourceLayout, uniformsBuffer));
			Load(fragmentShaderSourceCode);
		}

		private GraphicsDevice graphicsDevice;
		private readonly OutputDescription outputDescription;
		private DeviceBuffer uniformsBuffer;
		private readonly ResourceLayout resourceLayout;
		private Pipeline pipeline, disposePipeline;
		private readonly ResourceSet resourceSet;

		private static Shader[] LoadShader(ResourceFactory resourceFactory, string fragmentShaderSourceCode)
		{
			const string vertexCode = @"
				#version 130

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

		public void Dispose()
		{
			uniformsBuffer.Dispose();
			resourceLayout.Dispose();
			resourceSet.Dispose();
			disposePipeline?.Dispose();
			pipeline.Dispose();
		}

		public void Load(string fragmentShaderSourceCode)
		{
			var shaders = LoadShader(graphicsDevice.ResourceFactory, fragmentShaderSourceCode);
			var pipelineDesc = new GraphicsPipelineDescription()
			{
				BlendState = BlendStateDescription.SingleDisabled,
				DepthStencilState = DepthStencilStateDescription.Disabled,
				RasterizerState = RasterizerStateDescription.CullNone,
				PrimitiveTopology = PrimitiveTopology.TriangleStrip,
				ResourceLayouts = new ResourceLayout[] { resourceLayout },
				ShaderSet = new ShaderSetDescription()
				{
					VertexLayouts = new VertexLayoutDescription[] { },
					Shaders = shaders
				},
				Outputs = outputDescription
			};
			var oldPipeline = pipeline;
			pipeline = graphicsDevice.ResourceFactory.CreateGraphicsPipeline(pipelineDesc);
			disposePipeline = oldPipeline;
		}

		internal void Update(PredefinedUniforms uniforms)
		{
			graphicsDevice.UpdateBuffer(uniformsBuffer, 0, uniforms);
		}

		internal void Draw(CommandList commandList)
		{
			commandList.SetPipeline(pipeline);
			commandList.SetGraphicsResourceSet(0, resourceSet);
			commandList.Draw(4);
			disposePipeline?.Dispose(); // delayed disposing because during creation the old pipeline could still be shown
			disposePipeline = null;
		}
	}
}
