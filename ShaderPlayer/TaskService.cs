using System;
using System.Collections.Concurrent;

namespace ShaderPlayer
{
	class TaskService
	{
		public void AddTask(Action action) => tasks.Enqueue(action ?? throw new ArgumentNullException(nameof(action)));

		public void ProcessNextTask()
		{
			if (tasks.TryDequeue(out var task))
			{
				task();
			}
		}

		private ConcurrentQueue<Action> tasks = new ConcurrentQueue<Action>();
	}
}
