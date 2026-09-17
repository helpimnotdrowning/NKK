/*
	This file is part of NKK.

	NKK is free software: you can redistribute it and/or modify it under the
	terms of the GNU Affero General Public License as published by the Free
	Software Foundation, either version 3 of the License, or (at your option)
	any later version.

	NKK is distributed in the hope that it will be useful, but WITHOUT ANY
	WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
	FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for
	more details.

	You should have received a copy of the GNU Affero General Public License
	along with NKK. If not, see <http://www.gnu.org/licenses/>.
*/

using Microsoft.Extensions.Options;
using NUglify.Helpers;
using Serilog;
using Serilog.Events;
using ThrottleDebounce;
using ILogger = Serilog.ILogger;

namespace NKK;

public class PostWatcherOptions {
	public required DirectoryInfo AllPostsRoot { get; set; }
}

public class PostWatcher(PostStore postStore, IOptions<PostWatcherOptions> options) : BackgroundService {
	private static ILogger _logger = Log.ForContext<PostWatcher>();
	// lambda that will be called by UpdateStore_EventWrapper
	// this should never be called by anyone else!!
	private Action _debouncedUpdate;
	private FileSystemWatcher _watcher;
	
	// TODO: seems to stop watching on exception? i think???
	protected override Task ExecuteAsync(CancellationToken stoppingToken) {
		this.UpdateAllStores();
		
		this._watcher = new FileSystemWatcher(options.Value.AllPostsRoot.FullName) {
			NotifyFilter = NotifyFilters.Attributes
				| NotifyFilters.CreationTime
				| NotifyFilters.DirectoryName
				| NotifyFilters.FileName
				| NotifyFilters.LastWrite
				| NotifyFilters.Security
				| NotifyFilters.Size,
			IncludeSubdirectories = true,
			EnableRaisingEvents = true,
		};
		
		this._debouncedUpdate = Debouncer.Debounce(
			(Action)this.UpdateAllStores, TimeSpan.FromSeconds(1), leading: true, trailing: false
		).Invoke;
		
		this._watcher.Changed += this.UpdateStore_EventWrapper;
		this._watcher.Created += this.UpdateStore_EventWrapper;
		this._watcher.Deleted += this.UpdateStore_EventWrapper;
		this._watcher.Renamed += this.UpdateStore_EventWrapper;
		this._watcher.Error += (sender, e) => {
			_logger.Error(e.GetException(), "PostWatcher got an error!");
		};

		this._watcher.Disposed += (_, _) => {
			_logger.Error("oops threw the watcher away :/");
		};
		
		return Task.CompletedTask;
	}

	/// <summary>
	///		Small wrapper for debounced UpdateStore call for debug logging and maybe other
	///		things
	/// </summary>
	/// <param name="sender">
	///		see <see cref="FileSystemEventHandler"/>
	/// </param>
	/// <param name="e">
	///		see <see cref="FileSystemEventHandler"/>
	/// </param>
	private void UpdateStore_EventWrapper(Object sender, FileSystemEventArgs e) {
		if (_logger.IsEnabled(LogEventLevel.Debug))
			if (e is RenamedEventArgs re)
				_logger.Debug("Got FileSystemEvent: File '{OldName}' experienced {ChangeType} (to '{Name}')",
					re.OldName,
					re.ChangeType,
					re.Name );
			else
				_logger.Debug("Got FileSystemEvent: File {Name} experienced {ChangeType}",
					e.Name,
					e.ChangeType);
		
		this._debouncedUpdate();
	}

	/// <summary>
	///		Scan and update store for corresponding T
	/// </summary>
	/// <typeparam name="T">
	///		Implementor of <see cref="IPostPayload"/>
	/// </typeparam>
	private void UpdateStore<T>() where T : class, IPostPayload, new() {
		// Now That's What I Call Type Safety!
		
		List<String> checkedPosts = [];

		var posts = postStore.GetAll<T>();
		// check on current posts
		posts.ForEach(post => {
			if (Utils.ReadPost<T>(post.PostDirectory).HasResult(out var newPost, out var errs)) {
				// post has been modified?
				if (post.PostFileLastModified != newPost.PostFileLastModified) {
					if (_logger.IsEnabled(LogEventLevel.Debug))
						_logger.Debug("Updated {0}: {Title} ({Id})",
							typeof(T),
							newPost.Title,
							newPost.Id
						);
					postStore.AddOrUpdate<T>(post.Id, newPost);
				}
				else {
					if (_logger.IsEnabled(LogEventLevel.Debug))
						_logger.Debug("Skipping {0}: {Title} ({Id})",
							typeof(T),
							newPost.Title,
							newPost.Id
						);
				}

				// please do not change post IDs while live...
				checkedPosts.Add(post.PostDirectory.FullName);
				return;
			}

			var err = errs.Cast<Utils.ReadPostError>().Last();
			_logger.Error("Failed to read updated {0} ({Id}), removing from store: {NKK_Reason} ({Message})",
				typeof(T),
				post.Id,
				err.NKK_Reason,
				err.Message);
			postStore.Remove<T>(post.Id);
			checkedPosts.Add(post.PostDirectory.FullName);
		});
		
		// look for new posts
		new DirectoryInfo(Path.Combine(options.Value.AllPostsRoot!.FullName, T.PathFragment)).EnumerateDirectories()
			.Where(d => !checkedPosts.Contains(d.FullName)) // note: this is fine, remember we are per T
			.Select(Utils.ReadPost<T>)
			.ForEach(res => {
				if (res.HasResult(out var post, out var err)) {
					if (_logger.IsEnabled(LogEventLevel.Debug))
						_logger.Debug("Added {0}: {Title} ({Id})",
							typeof(T),
							post.Title,
							post.Id
						);
					postStore.AddOrUpdate(post.Id, post);
				} else {
					var errC = err.Cast<Utils.ReadPostError>().Last();
					_logger.Error("Failed to load {0}({Id}) : {NKK_Reason} ({Message})",
						typeof(T),
						errC.Id,
						errC.NKK_Reason,
						errC.Message);
				}
			});
	}
	
	private void UpdateAllStores() {
		this.UpdateStore<SayingPayload>();
		this.UpdateStore<ArtifactPayload>();
	}
}