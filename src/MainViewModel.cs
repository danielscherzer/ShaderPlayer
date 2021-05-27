using System.ComponentModel;

namespace ShaderPlayer
{
	class MainViewModel : INotifyPropertyChanged
	{
		public MainViewModel()
		{
			//PropertyChanged.SetNotify()
		}

		public string ErrorMessage { get; set; }
		public bool Fullscreen { get; set; } = false;
		public bool ShowDashboardWindow { get; set; } = false;
		public bool ShowStatsWindow { get; set; } = true;

		public event PropertyChangedEventHandler PropertyChanged;

		//TODO: public void ToggleFullscreen() { Fullscreen = !Fullscreen; }
		public void ToggleDashboardWindow() { ShowDashboardWindow = !ShowDashboardWindow; }
		public void ToggleStatsWindow() { ShowStatsWindow = !ShowStatsWindow; }
	}
}
