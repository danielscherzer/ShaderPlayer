using System;
using System.IO;
using System.Reactive.Linq;

namespace ShaderPlayer
{
	internal class TrackedFile
	{
		internal static IDisposable Load(string fileName, Action<string> onSourceAvailable)
		{
			var seqFileChanged = CreateFileChangeSequence(fileName);
			return seqFileChanged.Throttle(TimeSpan.FromSeconds(0.1f)).Delay(TimeSpan.FromSeconds(0.1f)).Select((newFileName) => File.ReadAllText(newFileName))
				.Subscribe((shaderSource) => onSourceAvailable(shaderSource));
		}

		private static IObservable<string> CreateFileChangeSequence(string fileName)
		{
			var fullPath = Path.GetFullPath(fileName);
			return Observable.Return(fileName).Concat(
				Observable.Using(
				() => new FileSystemWatcher(Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath))
				{
					NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime | NotifyFilters.FileName,
					EnableRaisingEvents = true,
				},
				watcher =>
				{
					var fileChanged = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => watcher.Changed += h, h => watcher.Changed -= h).Select(x => fullPath);
					var fileCreated = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => watcher.Created += h, h => watcher.Created -= h).Select(x => fullPath);
					var fileRenamed = Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(h => watcher.Renamed += h, h => watcher.Renamed -= h).Select(x => fullPath);
					return fileChanged.Merge(fileCreated).Merge(fileRenamed);
				})
				);
		}
	}
}
