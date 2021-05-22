using System;
using System.Runtime.Serialization;

namespace ShaderPlayer
{
	[Serializable]
	internal class ShaderIncludeException : Exception
	{
		public ShaderIncludeException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ShaderIncludeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}