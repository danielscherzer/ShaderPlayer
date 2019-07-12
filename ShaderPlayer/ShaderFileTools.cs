using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Veldrid;

namespace ShaderPlayer
{
	static class ShaderFileTools
	{

		public static string ShaderFileToSourceCode(string fileName, ResourceFactory resourceFactory)
		{
			var dir = Path.GetDirectoryName(fileName);
			var shaderCode = PreprocessShaderCode(File.ReadAllText(fileName));
			string GetIncludeCode(string includeName)
			{
				var includeFileName = Path.Combine(dir, includeName);
				var includeCode = PreprocessShaderCode(File.ReadAllText(includeFileName));
				var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(includeCode), "main");
				using (resourceFactory.CreateShader(fragmentShaderDesc))
				{
					return includeCode;
				}
			}
			var expandedShaderCode = GlslTools.ExpandIncludes(shaderCode, GetIncludeCode);
			return expandedShaderCode;
		}

		static string PreprocessShaderCode(string shaderSourceCode)
		{
			var shaderString = Uniforms.ShaderString;
			string ReplaceOnce(Match match)
			{
				var result = shaderString;
				shaderString = string.Empty;
				return result;
			}
			var newCode = GlslTools.ReplaceUniforms(shaderSourceCode, ReplaceOnce);
			return newCode;
		}
	}
}
