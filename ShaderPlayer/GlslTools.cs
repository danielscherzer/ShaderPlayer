using System;
using System.Text.RegularExpressions;

namespace ShaderPlayer
{
	public static class GlslTools
	{
		/// <summary>
		/// Searches for #include statements in the shader code and replaces them by the code in the include resource.
		/// </summary>
		/// <param name="shaderCode">The shader code.</param>
		/// <param name="GetIncludeCode">Functor that will be called with the include path as parameter and returns the include shader code.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">GetIncludeCode</exception>
		public static string ExpandIncludes(string shaderCode, Func<string, string> GetIncludeCode)
		{
			if (GetIncludeCode == null) throw new ArgumentNullException(nameof(GetIncludeCode));

			var lines = shaderCode.Split(new[] { '\n' }, StringSplitOptions.None); //if UNIX style line endings still working so do not use Envirnoment.NewLine
			var pattern = @"#include\s+""([^""]+)"""; //match everything inside " except " so we get shortest ".+" match 
			int lineNr = 1;
			foreach (var line in lines)
			{
				// Search for include pattern (e.g. #include raycast.glsl) (nested not supported)
				foreach (Match match in Regex.Matches(line, pattern, RegexOptions.Singleline))
				{
					var sFullMatch = match.Value;
					var includeName = match.Groups[1].ToString(); // get the include
					var includeCode = GetIncludeCode(includeName);
					var lineNumberCorrection = $"{Environment.NewLine}#line {lineNr}{Environment.NewLine}";
					shaderCode = shaderCode.Replace(sFullMatch, includeCode + lineNumberCorrection); // replace #include with actual include code
				}
				++lineNr;
			}
			return shaderCode;
		}

		private static string UniformPattern => @"uniform\s+([^\s]+)\s+([^\s]+)\s*;"; //matches uniform<spaces>type<spaces>name<spaces>; 

		public static string MakeConformal(string shaderSourceCode)
		{
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

			newCode = Regex.Replace(newCode, UniformPattern, m => string.Empty);
			newCode = Regex.Replace(newCode, @"varying\s", m => "in ");
			return header + newCode;
		}
	}
}
