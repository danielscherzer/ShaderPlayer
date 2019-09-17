using Jot;
using Jot.Storage;
using Veldrid;
using Veldrid.Sdl2;

namespace ShaderPlayer
{
	static class Persist
	{
		internal static Tracker CreateTracker(Sdl2Window window, MainViewModel mainViewModel)
		{
			var tracker = new Tracker(new JsonFileStore("./"));
			tracker.Configure<Sdl2Window>().Id(w => "Window")
				.Property(w => w.X, 200)
				.Property(w => w.Y, 60)
				.Property(w => w.Width, 1024)
				.Property(w => w.Height, 1024)
				.WhenPersistingProperty((wnd, property) => property.Cancel = WindowState.Normal != wnd.WindowState)
				.PersistOn(nameof(Sdl2Window.Closing));
			tracker.Track(window);

			tracker.Configure<MainViewModel>().Id(vm => nameof(MainViewModel))
				.Property(vm => vm.ShowDashboardWindow, false)
				.Property(vm => vm.ShowStatsWindow, true)
				.PersistOn(nameof(Sdl2Window.Closing), window);
			tracker.Track(mainViewModel);
			return tracker;
		}
	}
}
