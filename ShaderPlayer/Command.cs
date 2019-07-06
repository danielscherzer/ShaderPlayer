using System;
using Veldrid;

namespace ShaderPlayer
{
	class Command
	{
		private readonly Action action;

		public Command(string caption, Action action, Key? key)
		{
			Caption = caption;
			this.action = action ?? throw new ArgumentNullException(nameof(action));
			Key = key;
		}

		public string Caption { get; }

		public Key? Key { get; }

		public void Execute() => action();
	}
}
