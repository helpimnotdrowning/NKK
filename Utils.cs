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
		NoPost,
		PayloadDeserialize,
		InvalidNumericId
	}
	
	public class ReadPostError(ReadPostReason NKK_Reason, String message) : FluentResults.IError {
		public ReadPostReason NKK_Reason { get; }
		public String Message { get; }
		public Dictionary<String, Object> Metadata { get; }
		public List<IError> Reasons { get; }
	}
	
	public static Result<T> ReadPost<T>(DirectoryInfo postDirectory) where T : class, IPostPayload, new() {
		FileInfo? postFile = postDirectory.EnumerateFiles().SingleOrDefault(f => f != null && f.Name == T.PostFile, null);
		if (postFile == null)
			return Result.Fail(new ReadPostError(ReadPostReason.NoPost, $"Directory {postDirectory.FullName} has no {T.PathFragment} file"));
		
		using StreamReader reader = new StreamReader(postFile.FullName, Encoding.UTF8);
		String[] rawContent = reader.ReadToEnd().Split("%---", 2);
		String[] ids = postDirectory.Name.Split('-',2);

		dynamic? payloadJson;
		
		try {
			payloadJson = JsonSerializer.Deserialize(rawContent[0], T.JsonTarget, new JsonSerializerOptions());
			if (payloadJson == null)
				return Result.Fail(new ReadPostError(ReadPostReason.PayloadDeserialize, $"Payload for {postDirectory.FullName} was null"));
		} catch (JsonException e) {
			return Result.Fail(new ReadPostError(ReadPostReason.PayloadDeserialize, $"Failed to parse payload for {postDirectory.FullName}: {e.Message}"));
		}

		if (!Int32.TryParse(ids[0], out int numericId))
			return Result.Fail(new ReadPostError(ReadPostReason.InvalidNumericId, $"Numeric ID for post '{postDirectory.FullName}' could not be parsed"));
		
		T payload = new T {
			PostDirectory = postDirectory,
			PostFileLastModified = postFile.LastWriteTimeUtc,
			Id = new PostId {
				FullId = postDirectory.Name,
				NumericId = numericId,
				TitleId = ids[1],
			}
		};
		
		payload.LoadJson(payloadJson);
		
		return payload;
	}
	
	public static bool IsAbsoluteUrl(String url) {
		return Uri.TryCreate(url, UriKind.Absolute, out _);
	}

	public static void WriteException(Exception? ex) {
		while (ex != null) {
			Console.WriteLine($"ERROR: watcher failed! ${ex.GetType()}: ${ex.Message}");
			Console.WriteLine(ex.StackTrace);
			Console.WriteLine();
			ex = ex.InnerException;
		}
	}

	extension<T>(IEnumerable<T> enumerable) {
		public T RandomElement() {
			int index = (new Random()).Next(0, enumerable.Count());
			return enumerable.ElementAt(index);
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
			
			return builder.ToString() ?? String.Empty;
		}
	}
}