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

#pragma warning disable BL0006

using System.Text;
using System.Text.Json;

using FluentResults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using NUglify;
using NUglify.Html;

using static Microsoft.AspNetCore.WebUtilities.ReasonPhrases;

namespace NKK;

public static class Utils {
	private static readonly HtmlSettings HtmlSettings = new() {
		RemoveComments = false,
		RemoveOptionalTags = false,
		RemoveInvalidClosingTags = false,
		RemoveEmptyAttributes = false,
		RemoveScriptStyleTypeAttribute = false,
		ShortBooleanAttribute = false,
		IsFragmentOnly = true,
		MinifyJs = false,
		MinifyJsAttributes = false,
		MinifyCss = false,
		MinifyCssAttributes = false,
	};

	private static readonly HtmlSettings XmlSettings = new() {
		AttributesCaseSensitive = false,
		TagsCaseSensitive = true,
		CollapseWhitespaces = true,
		RemoveComments = false,
		RemoveOptionalTags = false,
		RemoveInvalidClosingTags = false,
		RemoveEmptyAttributes = false,
		RemoveAttributeQuotes = false,
		DecodeEntityCharacters = false,
		AttributeQuoteChar = '"',
		RemoveScriptStyleTypeAttribute = false,
		ShortBooleanAttribute = false,
		IsFragmentOnly = true,
		MinifyJs = false,
		MinifyJsAttributes = false,
		MinifyCss = false,
		MinifyCssAttributes = false,
	};

	public static String OptimizeHtml(String html) {
		return Uglify.Html(html, HtmlSettings).Code ?? String.Empty;
	}

	public static String OptimizeXml(String html) {
		return Uglify.Html(html, XmlSettings).Code ?? String.Empty;
	}
	
	public enum ReadPostReason {
		/// <summary>
		///		Used when the post file is missing
		/// </summary>
		NoPost,
		/// <summary>
		///		Used when the postfile payload cannot be deserialized
		/// </summary>
		PayloadDeserialize,
		/// <summary>
		///		Used when numeric portion of ID is invalid
		///		TODO: handle invalid TitleID
		/// </summary>
		InvalidNumericId,
		/// <summary>
		///		Used for unhandled exceptions in parsing
		/// </summary>
		Unknown,
	}
	
	public class ReadPostError(ReadPostReason NKK_Reason, String message, String fullId) : FluentResults.IError {
		public ReadPostReason NKK_Reason { get; } = NKK_Reason;
		public String Id = fullId;
		public String Message { get; } = message;
		public Dictionary<String, Object> Metadata { get; } = new();
		public List<IError> Reasons { get; } = [];
	}
	
	public struct PostData {
		public String JsonString { get; init; }
		public String MarkdownContent { get; init; }
	}
	
	/// <summary>
	///		Read post data (JSON and Markdown) for a given post directory
	/// </summary>
	/// <param name="postDirectory">
	///		Post directory, containing a <see cref="T.PostFile"/>.
	/// </param>
	/// <returns>
	///		Result containing either a complete <see cref="PostData"/>, or a
	///		<see cref="ReadPostError"/>
	/// </returns>
	public static Result<PostData> GetPostData<T>(DirectoryInfo postDirectory) where T : IPostPayload {
		String postNameForErr = $"{postDirectory.Parent?.Name}/{postDirectory.Name}";
		
		StreamReader reader;
		try {
			reader = new StreamReader(Path.Combine(postDirectory.FullName, T.PostFile), Encoding.UTF8);
		} catch (Exception e) {
			return Result.Fail(new ReadPostError(ReadPostReason.PayloadDeserialize, 
				$"Failed to read post file for '{postNameForErr}': {e.Message}", postDirectory.Name));
		}
		
		String[] rawContent = reader.ReadToEnd().Split("%---", 2);
		if (rawContent.Length != 2)
			return Result.Fail(new ReadPostError(ReadPostReason.PayloadDeserialize,
				$"Failed to split post '{postNameForErr}'", postDirectory.Name));
		
		return new PostData {
			JsonString = rawContent[0],
			MarkdownContent = rawContent[1]
		};
	}

	/// <summary>
	///		Read post data (JSON and Markdown) for a given post. Wrapper for GetPostData
	///		when you already have a complete (though not necessarily valid) payload.
	/// </summary>
	/// <param name="postPayload">
	///		Potentially valid <see cref="IPostPayload"/>
	/// </param>
	/// <returns>
	///		Result containing either a complete <see cref="PostData"/>, or a
	///		<see cref="ReadPostError"/>
	/// </returns>
	public static Result<PostData> GetPostData<T>(T postPayload) where T : IPostPayload {
		return GetPostData<T>(postPayload.PostDirectory);
	}

	/// <summary>
	///		Try to read a post at <paramref name="postDirectory"/>
	/// </summary>
	/// <param name="postDirectory">
	///		A directory with a well-formed name (NumericId-TitleId) that contains a file
	///		named <see cref="T.PostFile"/>, properly formatted with a JSON payload block,
	///		separator <c>%---</c>, and Markdown content.
	/// </param>
	/// <typeparam name="T">
	///		Concrete implementor of <see cref="IPostPayload"/>. <see cref="T.JsonTarget">
	///		T's JsonTarget</see> must represent every object in the payload.
	/// </typeparam>
	/// <returns>
	///		A <see cref="Result"/>, where all possible errors are
	///		<see cref="ReadPostError"/>s, broadly described by the
	///		<see cref="ReadPostError.NKK_Reason"/> and possibly (but not always) further
	///		described by the <see cref="ReadPostError.Message"/> (Messages are subject to
	///		change).
	/// </returns>
	public static Result<T> ReadPost<T>(DirectoryInfo postDirectory) where T : class, IPostPayload, new() {
		String postNameForErr = $"{postDirectory.Parent?.Name}/{postDirectory.Name}";
		
		try {
			// try to find the post file
			var candidates = postDirectory.EnumerateFiles(T.PostFile).ToList();
			if (candidates.Count == 0)
				return Result.Fail(new ReadPostError(ReadPostReason.NoPost,
					$"Post '{postNameForErr}' has no {T.PostFile} file", postDirectory.Name));
			FileInfo postFile = candidates.First();
			
			// try to read the post file
			if (!GetPostData<T>(postDirectory).HasResult(out var postData, out var postDataErr))
				return Result.Fail(postDataErr);
			
			if (!PostId.From(postDirectory.Name).HasResult(out var id, out var idErr))
				return Result.Fail(idErr);
			
			// try to deserialize the payload
			dynamic? payloadJson;
			try {
				payloadJson = JsonSerializer.Deserialize(postData.JsonString, T.JsonTarget, new JsonSerializerOptions {
					AllowTrailingCommas = true,
					AllowOutOfOrderMetadataProperties = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
				});
				if (payloadJson == null)
					return Result.Fail(new ReadPostError(ReadPostReason.PayloadDeserialize,
						$"Payload for post '{postNameForErr} was null", id.FullId));
			} catch (Exception e) {
				return Result.Fail(new ReadPostError(ReadPostReason.PayloadDeserialize,
					$"Failed to parse payload for post '{postNameForErr}': {e.Message}", id.FullId));
			}
			
			T payload = new T {
				PostDirectory = postDirectory,
				PostFileLastModified = postFile.LastWriteTimeUtc,
				Id = id,
			};
			
			payload.LoadJson(payloadJson);
			
			return payload;
		} catch (Exception e) {
			return Result.Fail(new ReadPostError(ReadPostReason.Unknown, 
				$"Failed to read post '{postNameForErr}' due to an unknown exception: {e.Message}", postDirectory.Name));
		}
	}
	
	public static String RESET = "\e[0m";
	public static String BRIGHT_RED = "\e[91m";
	public static String BLACK_ON_RED = "\e[37m\e[41m";
			
	public static String FormatStatusCode(int code) {
		var col = code switch {
			>= 500 => BLACK_ON_RED,
			>= 400 => BRIGHT_RED,
			_ => "",
		};

		return $"{col}{code} ({GetReasonPhrase(code)}){RESET}";
	}

	public static String FormatSize(long size) {
		double dSize = (double)size;
		return size switch {
			>= 1024L * 1024 * 1024 * 1024 => $"{dSize / (1024L * 1024 * 1024 * 1024):F2} TiB",
			>= 1024  * 1024 * 1024 => $"{dSize / (1024 * 1024 * 1024):F2} GiB",
			>= 1024  * 1024 => $"{dSize / (1024 * 1024):F2} MiB",
			>= 1024  => $"{dSize / 1024:F2} KiB",
			_ => $"{size} B",
		};
	}
	
	public static bool IsAbsoluteUrl(String url) {
		return Uri.TryCreate(url, UriKind.Absolute, out _);
	}

	extension<T>(IEnumerable<T> enumerable) {
		public T RandomElement() {
			var list = enumerable.ToList();
			int index = (new Random()).Next(0, list.Count);
			return list.ElementAt(index);
		}
	}

	extension(RenderFragment fragment) {
		public String RenderString() {
			StringBuilder builder = new StringBuilder();
			RenderTreeBuilder renderer = new RenderTreeBuilder();
			fragment(renderer);
			
			renderer.GetFrames().Array.ToList()
				.ForEach(f => {
					// this seems like the only types that have actual content?
					// idk tho
					switch (f.FrameType) {
						case RenderTreeFrameType.Markup:
							builder.Append(f.MarkupContent);
							break;
						case RenderTreeFrameType.Text:
							builder.Append(f.TextContent);
							break;
						default:
							break;
					}
				});
			
			return builder.ToString();
		}
	}

	extension<TResult>(Result<TResult> res) {
		public bool HasResult(out TResult val, out IReadOnlyList<IError> err) {
			if (res.IsFailed) {
				val = default!;
				err = res.Errors;
				return false;
			}

			val = res.Value;
			err = [];
			return true;
		}
	}
}