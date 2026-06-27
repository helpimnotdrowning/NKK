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

namespace NKK;

public class PostWatcherOptions {
	public DirectoryInfo? AllPostsRoot { get; set; }
}

public class PostWatcher(PostStore postStore, IOptions<PostWatcherOptions> options) : BackgroundService {
	protected override Task ExecuteAsync(CancellationToken stoppingToken) {
		List<String> addedSayings = [];
		List<String> addedArtifacts = [];
		
		IEnumerable<SayingPayload> sayings = postStore.GetAllSayings().ToList();
		sayings.ForEach((post) => {
			var newPost = Utils.ReadPost<SayingPayload>(post.PostDirectory);
			if (newPost == null) {
				postStore.RemoveSaying(post.Id.FullId);
				return;
			}
			
			if (post.PostFileLastModified == newPost.PostFileLastModified) return;
			
			// please do not change post IDs while live...
			postStore.AddOrUpdateSaying(post.Id.FullId, newPost);
			addedSayings.Add(post.PostDirectory.FullName);
		});

		new DirectoryInfo(Path.Combine(options.Value.AllPostsRoot.FullName, SayingPayload.PathFragment)).EnumerateDirectories()
			.Where(d => !addedSayings.Contains(d.FullName))
			.ForEach(d => {
				var post = Utils.ReadPost<SayingPayload>(d);
				if (post == null) return;
				
				postStore.AddOrUpdateSaying(post.Id.FullId, post);
			}
		);
		
		/* *** *** *** */
		
		IEnumerable<ArtifactPayload> artifacts = postStore.GetAllArtifacts().ToList();
		artifacts.ForEach((post) => {
			var newPost = Utils.ReadPost<ArtifactPayload>(post.PostDirectory);
			if (newPost == null) {
				postStore.RemoveArtifact(post.Id.FullId);
				return;
			}
			
			if (post.PostFileLastModified == newPost.PostFileLastModified) return;
			
			// please do not change post IDs while live...
			postStore.AddOrUpdateArtifact(post.Id.FullId, newPost);
			addedArtifacts.Add(post.PostDirectory.FullName);
		});

		new DirectoryInfo(Path.Combine(options.Value.AllPostsRoot.FullName, ArtifactPayload.PathFragment)).EnumerateDirectories()
			.Where(d => !addedArtifacts.Contains(d.FullName))
			.ForEach(d => {
					var post = Utils.ReadPost<ArtifactPayload>(d);
					if (post == null) return;
				
					postStore.AddOrUpdateArtifact(post.Id.FullId, post);
				}
			);

		return Task.CompletedTask;
	}
}