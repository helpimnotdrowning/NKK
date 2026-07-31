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

namespace NKK;

public static class Utils {
	private static readonly HtmlSettings HtmlSettings = new HtmlSettings() {
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
	
	public static String OptimizeHtml(String html) {
		return Uglify.Html(html, HtmlSettings).Code ?? String.Empty;
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
	
	public class ReadPostError(ReadPostReason NKK_Reason, String message) : FluentResults.IError {
		public ReadPostReason NKK_Reason { get; } = NKK_Reason;
		public String Message { get; } = message;
		public Dictionary<String, Object> Metadata { get; } = new Dictionary<String, Object>();
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
	///		
	/// </param>
	/// <returns></returns>
	public static Result<PostData> GetPostData<T>(DirectoryInfo postDirectory) where T : IPostPayload {
		String postNameForErr = $"{postDirectory.Parent?.Name}/{postDirectory.Name}";
		
		StreamReader reader;
		try {
			reader = new StreamReader(Path.Combine(postDirectory.FullName, T.PostFile), Encoding.UTF8);
		} catch (Exception e) {
			return Result.Fail(new ReadPostError(ReadPostReason.PayloadDeserialize, 
				$"Failed to read post file for '{postNameForErr}': {e.Message}"));
		}
		
		String[] rawContent = reader.ReadToEnd().Split("%---", 2);
		if (rawContent.Length != 2)
			return Result.Fail(new ReadPostError(ReadPostReason.PayloadDeserialize,
				$"Failed to split post '{postNameForErr}'"));
		
		return new PostData {
			JsonString = rawContent[0],
			MarkdownContent = rawContent[1]
		};
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
					$"Post '{postNameForErr}' has no {T.PostFile} file"));
			FileInfo postFile = candidates.First();
			
			// try to read the post file
			var postDataResult = GetPostData<T>(postDirectory);
			if (postDataResult.IsFailed)
				return Result.Fail(postDataResult.Errors);
			
			// try to deserialize the payload
			dynamic? payloadJson;
			try {
				payloadJson = JsonSerializer.Deserialize(postDataResult.Value.JsonString, T.JsonTarget);
				if (payloadJson == null)
					return Result.Fail(new ReadPostError(ReadPostReason.PayloadDeserialize,
						$"Payload for post '{postNameForErr} was null"));
			} catch (Exception e) {
				return Result.Fail(new ReadPostError(ReadPostReason.PayloadDeserialize,
					$"Failed to parse payload for post '{postNameForErr}': {e.Message}"));
			}
			
			var maybeId = PostId.From(postDirectory.Name);
			if (maybeId.IsFailed)
				return Result.Fail(maybeId.Errors);
			
			T payload = new T {
				PostDirectory = postDirectory,
				PostFileLastModified = postFile.LastWriteTimeUtc,
				Id = maybeId.Value,
			};
			
			payload.LoadJson(payloadJson);
			
			return payload;
		} catch (Exception e) {
			return Result.Fail(new ReadPostError(ReadPostReason.Unknown, 
				$"Failed to read post '{postNameForErr}' due to an unknown exception: {e.Message}"));
		}
	}
	
	public static bool IsAbsoluteUrl(String url) {
		return Uri.TryCreate(url, UriKind.Absolute, out _);
	}

	public static void WriteException(Exception ex) {
		Exception? exc = ex;
		while (exc != null) {
			Console.WriteLine($"ERROR: watcher failed! ${exc.GetType()}: ${exc.Message}");
			Console.WriteLine(exc.StackTrace);
			Console.WriteLine();
			exc = exc.InnerException;
		}
	}
	
	/// <summary>
	///		Shortcut to call the error page
	/// </summary>
	/// <param name="httpContextAccessor">
	///		<c>@inject IHttpContextAccessor HttpContextAccessor</c>
	/// </param>
	/// <param name="navigationManager">
	///		<c>@inject NavigationManager NavigationManager</c>
	/// </param>
	/// <param name="OnInitializedAsync">
	///		<c>base.OnInitializedAsync</c>
	/// </param>
	/// <param name="statusCode">
	///		Integer where 400 &lt;= statusCode &lt;= 599
	/// </param>
	/// <returns>
	///		Task, not sure what it actually represents but just it
	/// </returns>
	public static Task FailPage(IHttpContextAccessor httpContextAccessor, NavigationManager navigationManager, Func<Task> OnInitializedAsync,
		int statusCode) {
		httpContextAccessor.HttpContext!.Response.StatusCode = statusCode;
		navigationManager.NotFound();
		return OnInitializedAsync();
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
}