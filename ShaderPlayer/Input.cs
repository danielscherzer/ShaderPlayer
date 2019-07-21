using System.Collections.Generic;
using System.Linq;
using Veldrid;
using Veldrid.Sdl2;

namespace ShaderPlayer
{
	class Input
	{
		public InputSnapshot Snapshot { get; protected set; }

		public void Update(Sdl2Window window)
		{
			Snapshot = window.PumpEvents();
			inputTracker.UpdateFrameInput(Snapshot, window);
			foreach (var command in Commands.Where(command => command.Key.HasValue))
			{
				if (inputTracker.GetKeyDown(command.Key.Value)) { command.Execute(); }
			}
		}

		public List<Command> Commands { get; } = new List<Command>();

		private InputTracker inputTracker = new InputTracker();
	}
}
