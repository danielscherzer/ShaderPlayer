using System.Collections.Generic;
using System.Linq;

namespace ShaderPlayer
{
	public static class CommandExtensions
	{
		public static void Execute(this IEnumerable<CommandBinding> commands, InputTracker inputTracker)
		{
			foreach (var command in commands)
			{
				if (command.Key.HasValue && inputTracker.IsNewKeyDown(command.Key.Value))
				{
					command.Execute();
				}
			}
		}
	}
}
