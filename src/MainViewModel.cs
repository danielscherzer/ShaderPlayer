namespace ShaderPlayer
{
	class MainViewModel
	{
		public string ErrorMessage { get; set; } = "";
		public bool Fullscreen { get; set; } = false;
		public bool ShowDashboardWindow { get; set; } = false;
		public bool ShowStatsWindow { get; set; } = true;

		//TODO: public void ToggleFullscreen() { Fullscreen = !Fullscreen; }
		public void ToggleDashboardWindow() { ShowDashboardWindow = !ShowDashboardWindow; }
		public void ToggleStatsWindow() { ShowStatsWindow = !ShowStatsWindow; }
	}
}
