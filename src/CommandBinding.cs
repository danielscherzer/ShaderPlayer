using System;
using Veldrid;

namespace ShaderPlayer
{
	public class CommandBinding
	{
		private readonly Action action;

		public CommandBinding(Action action, Key? key)
		{
			this.action = action ?? throw new ArgumentNullException(nameof(action));
			Key = key;
		}

		public Key? Key { get; }

		public void Execute() => action();
	}
}
