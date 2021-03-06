﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Data;
using MusicPlayer.Models;

namespace MusicPlayer.Managers
{
	internal class TempFileManager : ManagerBase<TempFileManager>
	{
		SlideQueue<string> Queue = new SlideQueue<string>(5);

		public TempFileManager()
		{
			Queue.Removed = (file) => { File.Delete(Path.Combine(Path.GetTempPath(), file)); };
			var files = Directory.EnumerateFiles(Path.GetTempPath())
				.Where(x => x.EndsWith("mp3", StringComparison.CurrentCultureIgnoreCase) ||
							x.EndsWith("mp4", StringComparison.CurrentCultureIgnoreCase))
				.OrderBy(File.GetCreationTime).Where(x =>
				{
					var info = new FileInfo(x);
					if (info.Length == 0)
						File.Delete(x);
					return (info.Length > 0);
				}).Select(Path.GetFileName).ToList();
			files.ForEach(Queue.Add);
		}

		public void Cleanup()
		{
			Directory.GetFiles(Path.GetTempPath(), "*.tmp").ToList().ForEach(File.Delete);
		}

		public Tuple<bool, string> GetTempFile(string trackId)
		{
			var track = Database.Main.GetObject<Track, TempTrack>(trackId);
			var newPath = track.FileName;
			if (Queue.Contains(newPath))
				return new Tuple<bool, string>(true, Path.Combine(Path.GetTempPath(), newPath));
			return new Tuple<bool, string>(false, null);
		}

		public void Add(string trackId, string filePath)
		{
			var track = Database.Main.GetObject<Track, TempTrack>(trackId);
			var newPath = track.FileName;
			var info = new FileInfo(filePath);
			if (info.Length == 0)
				return;
			File.Copy(filePath, Path.Combine(Path.GetTempPath(), newPath), true);
			Queue.Add(newPath);
		}
	}
}