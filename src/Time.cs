using System.Diagnostics;

namespace ShaderPlayer
{
	class Time
	{
		public float FrameDelta { get; protected set; } = 1f/60f;
		public float Total { get; protected set; } = 0f;

		public void Update()
		{
			Total = (float)stopwatch.Elapsed.TotalSeconds;
			FrameDelta = Total - lastTime;
			lastTime = Total;
		}

		private Stopwatch stopwatch = Stopwatch.StartNew();
		private float lastTime = 0f;
	}
}
