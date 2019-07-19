using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace ShaderPlayer
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct Uniforms
	{
		public Vector4 mouse;
		public Vector4 camPos;
		public Vector4 camRot;
		public Vector2 resolution;
		public float time;
		private readonly float padding;

		public static string ShaderString => "uniform uniforms { vec4 iMouse; float iCamPosX; float iCamPosY; float iCamPosZ; float paddingCamPos; float iCamRotX; float iCamRotY; float iCamRotZ; float paddingCamRot; vec2 iResolution; float iGlobalTime; float padding; };";

	public static readonly uint SizeInBytes = (uint)Marshal.SizeOf<Uniforms>();

		public static ResourceLayoutDescription CalculateLayout()
		{
			return new ResourceLayoutDescription(
				new ResourceLayoutElementDescription("uniforms", ResourceKind.UniformBuffer, ShaderStages.Fragment)
			);
		}
	}
}
