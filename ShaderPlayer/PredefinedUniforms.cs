using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace ShaderPlayer
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct PredefinedUniforms
	{
		public Vector4 Mouse { get; set; }
		public Vector4 CamPos { get; set; }
		public Vector4 CamRot { get; set; }
		public Vector2 Resolution { get; set; }
		public float Time { get; set; }
		private readonly float padding;

		public static readonly string ShaderString = "uniform " + nameof(PredefinedUniforms) 
			+ " { vec4 iMouse;"
			+ " float iCamPosX; float iCamPosY; float iCamPosZ; float paddingCamPos;"
			+ " float iCamRotX; float iCamRotY; float iCamRotZ; float paddingCamRot;"
			+ " vec2 iResolution;"
			+ " float iGlobalTime; float padding; };";

		public static readonly uint SizeInBytes = (uint)Marshal.SizeOf<PredefinedUniforms>();

		public static readonly ResourceLayoutElementDescription ResourceLayoutElementDescription 
			= new ResourceLayoutElementDescription(nameof(PredefinedUniforms), ResourceKind.UniformBuffer, ShaderStages.Fragment);
	}
}
