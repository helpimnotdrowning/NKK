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
using ThrottleDebounce;

namespace NKK;

public class PostWatcherOptions {
	public DirectoryInfo? AllPostsRoot { get; set; }
}

public class PostWatcher(PostStore postStore, IOptions<PostWatcherOptions> options) : BackgroundService {
	private Action _debouncedUpdate;
	
	// TODO: seems to stop watching on exception? i think???
	protected override Task ExecuteAsync(CancellationToken stoppingToken) {
		this.UpdateAllStores();

		var watcher = new FileSystemWatcher(options.Value.AllPostsRoot!.FullName) {
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
			(Action)this.UpdateAllStores, TimeSpan.FromSeconds(5), leading: true, trailing: false
		).Invoke;
		
		watcher.Changed += this.UpdateStore_EventWrapper;
		watcher.Created += this.UpdateStore_EventWrapper;
		watcher.Deleted += this.UpdateStore_EventWrapper;
		watcher.Renamed += this.UpdateStore_EventWrapper;
		watcher.Error += (sender, e) => {
			Utils.WriteException(e.GetException());
		};
		
		return Task.CompletedTask;
	}

	private void UpdateStore_EventWrapper(Object sender, FileSystemEventArgs e) {
		if (e is RenamedEventArgs re) {
			Console.WriteLine($"{DateTime.Now}: Got FileSystemEvent: File '{re.OldName}' experienced {re.ChangeType} (to '{re.Name}')");			
		} else {
			Console.WriteLine($"{DateTime.Now}: Got FileSystemEvent: File '{e.Name}' experienced {e.ChangeType}");	
		}
		
		this._debouncedUpdate();
	}

	private void UpdateStore<T>() where T : class, IPostPayload, new() {
		// Now That's What I Call Type Safety!
		
		List<String> addedPosts = [];
		
		IEnumerable<T> posts = postStore.GetAll<T>().ToList();
		posts.ForEach(post => {
			var newPost = Utils.ReadPost<T>(post.PostDirectory);
			if (newPost.IsFailed) {
				postStore.Remove<T>(post.Id.FullId);
				return;
			}
			
			if (post.PostFileLastModified == newPost.Value.PostFileLastModified) return;
			
			// please do not change post IDs while live...
			postStore.AddOrUpdate<T>(post.Id.FullId, newPost.Value);
			addedPosts.Add(post.PostDirectory.FullName);
		});

		new DirectoryInfo(Path.Combine(options.Value.AllPostsRoot!.FullName, T.PathFragment)).EnumerateDirectories()
			.Where(d => !addedPosts.Contains(d.FullName))
			.ForEach(d => {
					var post = Utils.ReadPost<T>(d);
					if (post.IsFailed) return;
				
					postStore.AddOrUpdate<T>(post.Value.Id.FullId, post.Value);
				}
			);
	}
	
	private void UpdateAllStores() {
		this.UpdateStore<SayingPayload>();
		this.UpdateStore<ArtifactPayload>();
	}
}