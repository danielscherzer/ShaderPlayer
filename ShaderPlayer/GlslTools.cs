using GLSLhelper;
using System.Text.RegularExpressions;

namespace ShaderPlayer
{
	public static class GlslTools
	{
		private static readonly Regex regexVersion = new Regex(@"^#version\s+\d+");

		public static string MakeConformal(string shaderSourceCode)
		{
			bool containsVersion = regexVersion.IsMatch(shaderSourceCode);
			if (containsVersion) return shaderSourceCode;

			string header = string.Empty;
			string RemoveVersion(Match match)
			{
				header = match.Value;
				return string.Empty;
			}
			var newCode = Regex.Replace(shaderSourceCode, @"^#version\s+\d+", RemoveVersion);

			if (string.IsNullOrEmpty(header))
			{
				header = "#version 330 compatibility";
			}
			header += '\n';
			header += PredefinedUniforms.ShaderString;
			header += '\n';

			bool foundGl_FragColor = false;
			string ReplaceFragColor(Match match)
			{
				foundGl_FragColor = true;
				return "fragColor";
			}
			newCode = Regex.Replace(newCode, "gl_FragColor", ReplaceFragColor);
			if (foundGl_FragColor)
			{
				header += "out vec4 fragColor;\n";
			}
			header += "#line 1\n";

			newCode = RegexPatterns.Uniform.Replace(newCode, m => string.Empty);
			newCode = Regex.Replace(newCode, @"varying\s", m => "in ");
			return header + newCode;
		}
	}
}
