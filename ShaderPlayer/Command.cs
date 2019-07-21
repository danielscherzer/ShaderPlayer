using System;
using Veldrid;

namespace ShaderPlayer
{
	class Command
	{
		private readonly Action action;

		public Command(string caption, Action action, Key? key)
		{
			this.action = action ?? throw new ArgumentNullException(nameof(action));
			Caption = caption;
			Key = key;
		}

		public string Caption { get; }

		public Key? Key { get; }

		public void Execute() => action();
	}
}
