using System.Numerics;
using System.Runtime.InteropServices;

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

		public const string ResourceName = nameof(PredefinedUniforms);
		public static readonly string ShaderString = "uniform " + ResourceName
			+ " { vec4 iMouse;"
			+ " float iCamPosX; float iCamPosY; float iCamPosZ; float paddingCamPos;"
			+ " float iCamRotX; float iCamRotY; float iCamRotZ; float paddingCamRot;"
			+ " vec2 iResolution;"
			+ " float iGlobalTime; float padding; };\n"
			+ "uniform sampler2D texInput;\n"
			//+ "uniform texture2D texInput;\n"
			//+ "uniform sampler samplerInput;\n"
			;

		public static readonly uint SizeInBytes = (uint)Marshal.SizeOf<PredefinedUniforms>();
	}
}
