using System.IO;
using System.Text;
using Veldrid;

namespace ShaderPlayer
{
	static class ShaderFileTools
	{

		public static string ShaderFileToSourceCode(string fileName, ResourceFactory resourceFactory)
		{
			var dir = Path.GetDirectoryName(fileName);
			var shaderCode = File.ReadAllText(fileName);
			string GetIncludeCode(string includeName)
			{
				var includeFileName = Path.Combine(dir, includeName);
				var includeCode = File.ReadAllText(includeFileName);
				try
				{
					var conformalIncludeCode = GlslTools.MakeConformal(includeCode);
					var fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(conformalIncludeCode), "main");
					using (resourceFactory.CreateShader(fragmentShaderDesc))
					{
						return includeCode;
					}
				}
				catch(VeldridException vex)
				{
					throw new ShaderIncludeException($"Error compiling include file '{includeName}'", vex);
				}
			}
			var expandedShaderCode = GlslTools.ExpandIncludes(shaderCode, GetIncludeCode);
			return GlslTools.MakeConformal(expandedShaderCode);
		}
	}
}
