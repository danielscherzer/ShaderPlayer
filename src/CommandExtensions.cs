using System.Collections.Generic;
using System.Linq;

namespace ShaderPlayer
{
	public static class CommandExtensions
	{
		public static void Execute(this IEnumerable<CommandBinding> commands, InputTracker inputTracker)
		{
			foreach (var command in commands.Where(command => command.Key.HasValue))
			{
				if (inputTracker.IsNewKeyDown(command.Key.Value))
				{
					command.Execute();
				}
			}
		}
	}
}
