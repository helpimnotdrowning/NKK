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

using System.Text.RegularExpressions;
using FluentResults;

using static NKK.Utils;

namespace NKK;

public class PostId : IComparable {
	public required String FullId;
	public required int NumericId;
	public required String TitleId;
	
	/// <summary>
	///		Parse FullId into a PostId
	/// </summary>
	/// <param name="fullId">
	///		FullId, like [NumericId]-[TitleId]
	///	</param>
	/// <returns>
	///		Result, containing a ReadPostError or PostId
	/// </returns>
	public static Result<PostId> From(String fullId) {
		String[] ids = fullId.Split('-', 2);
		if (ids.Length != 2)
			return Result.Fail(new ReadPostError(ReadPostReason.InvalidNumericId,
				$"ID for post '{fullId}' was malformed"));
		
		if (!Int32.TryParse(ids[0], out int numericId))
			return Result.Fail(new ReadPostError(ReadPostReason.InvalidNumericId,
				$"ID for post '{fullId}' could not be parsed"));
		
		return new PostId {
			FullId = fullId,
			NumericId = numericId,
			TitleId = ids[1],
		};
	}
	
	public Int32 CompareTo(Object? obj) {
		if (obj == null || obj.GetType() != typeof(PostId)) return 1;
		
		return this.NumericId.CompareTo( ((PostId)obj).NumericId );
	}
}

public interface IPostPayload  {
	/// <summary>
	///		Name of directory under ALL_POSTS_ROOT where all posts of this type live
	/// </summary>
	public static abstract String PathFragment { get; }
	/// <summary>
	///		Name of post base Markdown file (ie post.md) under post directory
	/// </summary>
	public static abstract String PostFile { get; }
	/// <summary>
	///		Type that maps to all properties of the postfile's JSON payload
	/// </summary>
	public static abstract Type JsonTarget { get; }
	
	/// <summary>
	///		Post directory (contains <see cref="PostFile"/> file)
	/// </summary>
	public DirectoryInfo PostDirectory { get; set; }
	/// <summary>
	///		Post ID bundle (parsed from directory name)
	/// </summary>
	public PostId Id { get; set; }
	/// <summary>
	///		Post title (from JSON)
	/// </summary>
	public String Title { get; set; }
	/// <summary>
	///		Post creation date (from JSON)
	/// </summary>
	public DateTime Created { get; set; }
	/// <summary>
	///		PostFile last modified (from filesystem, used for update tracking, NOT
	///		display)
	/// </summary>
	public DateTime PostFileLastModified { get; set; }
	
	/// <summary>
	///		Loads information from deserialized JSON
	/// </summary>
	/// <param name="json">
	///		Object of type <see cref="JsonTarget"/>
	/// </param>
	public void LoadJson(dynamic json);
	
	public String GetMarkdown();
}

public class SayingPayloadJson {
	public required String Title { get; set; }
	public required String Description { get; set; }
	public required DateTime Created { get; set; }
}

public sealed class SayingPayload : IPostPayload {
	public static String PathFragment => "Sayings";
	public static String PostFile => "post.md";
	public static Type JsonTarget => typeof(SayingPayloadJson);
	
	public DirectoryInfo PostDirectory { get; set; }
	public PostId Id { get; set; }
	public String Title { get; set; }
	public DateTime Created { get; set; }
	public DateTime PostFileLastModified { get; set; }
	
	public String Description { get; set; }
	
	public void LoadJson(dynamic json) {
		this.Title = json.Title;
		this.Description = json.Description;
		this.Created = json.Created;
	}
	
	public String GetMarkdown() {
		return Utils.GetPostData<SayingPayload>(this.PostDirectory).Value.MarkdownContent;
	}
}

public class ArtifactSocialMediaLinks {
	public IList<String>? Twitter {
		get;
		// first twttr post has an id of 20 (two characters, and the length only grows
		// see: https://x.com/jack/status/20
		set => field = CheckPostIds(value, "Twitter", "^[0-9]{2,}$");
	}
	
	public IList<String>? Pixiv {
		get;
		// first pixiv post has an id of 20 (two characters, and the length only grows
		// see: https://dic.pixiv.net/en/a/pixiv%27s%20Oldest%20Drawing
		set => field = CheckPostIds(value, "Pixiv", "^[0-9]{2,}$");
	}
	
	public IList<String>? YouTube {
		get;
		// will probably be 11 characters for the rest of time
		// thanks, tom: https://www.youtube.com/watch?v=gocwRvLhDf8
		set => field = CheckPostIds(value, "YouTube", "^[0-9a-z_-]{11}$");
	}
	
	public IList<String>? Bluesky {
		get;
		// bsky needs both your handle/did AND post id to properly link, so this isn't
		// really an atomic post id like you would think...
		// matches <handle>/post/<post id>, where <handle> is either a did:plc, a
		// did:web, or a domain name
		set => field = CheckPostIds(value, "Bluesky", @"(?:(?:did:plc:[a-z0-9]{24})|(?:did:web:)?(?:[a-zA-Z0-9\-\.]+))\/post\/[a-z0-9]{13}");
	}
	
	private static IList<String>? CheckPostIds(IList<String>? idList, String siteName, String regex) {
		String? badId = null;
		
		if (idList == null)
			return idList;
		
		if (idList.Any(x => {
			badId = x;
			return String.IsNullOrWhiteSpace(x) || !Regex.IsMatch(x, regex);
		})) {
			throw new ArgumentException($"Invalid {siteName} post ID \"{badId}\"");
		}
		
		return idList;
	}
}

public class ArtifactPayloadJson {
	public required String Title { get; set; }
	public required DateTime Created { get; set; }
	public String[] Tags { get; set; }
	public ArtifactSocialMediaLinks Links { get; set; }
}

public sealed class ArtifactPayload : IPostPayload {
	private static readonly String[] ImageExtensions = [ ".gif", ".jpg", ".jpeg", ".png", ".webp" ];
	
	public static String PathFragment => "Museum";
	public static String PostFile => "artifact.md";
	public static Type JsonTarget => typeof(ArtifactPayloadJson);

	public DirectoryInfo PostDirectory {
		get;
		set {
			field = value;
			this.Images = value.EnumerateFiles()
				.Where(f => ImageExtensions.Contains(f.Extension))
				.OrderBy(f => f.Name)
				.ToList();
		}
	}

	public PostId Id { get; set; }
	public String Title { get; set; }
	public DateTime Created { get; set; }
	public DateTime PostFileLastModified { get; set; }
	
	public IList<String> Tags { get; set; }
	public ArtifactSocialMediaLinks Links { get; set; }
	public IList<FileInfo> Images { get; set; }

	public void LoadJson(dynamic json) {
		this.Title = json.Title;
		this.Created = json.Created;
		this.Tags = json.Tags;
		this.Links = json.Links;
	}
	
	public String GetMarkdown() {
		return Utils.GetPostData<ArtifactPayload>(this.PostDirectory).Value.MarkdownContent;
	}
}