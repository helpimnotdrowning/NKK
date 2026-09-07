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

using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using NUglify.Helpers;
using Serilog;
using ILogger = Serilog.ILogger;

namespace NKK;

public class LocalLinkFixerOptions {
	/// <summary>
	/// New base to use for non-absolute URLs.
	/// </summary>
	public required String Base { get; init; }
}

public class LocalLinkFixerExtension : IMarkdownExtension {
	private readonly LocalLinkFixerOptions _options;
	
	public LocalLinkFixerExtension(LocalLinkFixerOptions options) {
		this._options = options;
	}
	
	public void Setup(MarkdownPipelineBuilder pipeline) {
		pipeline.DocumentProcessed += document => document.Descendants<LinkInline>()
			.ForEach(link => {
				if (link.Url != null && !Utils.IsAbsoluteUrl(link.Url))
					link.Url = Path.Combine(this._options.Base, link.Url);
			});
	}
	
	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
}

public class FakeLinkProtocolOptions {
	// yes, not for this class!
	private ILogger _logger = Log.Logger.ForContext<FakeLinkProtocolExtension>();
	
	/// <summary>
	///		URL "protocol" that will be used to detect compatible urls, which must
	///		then start with <c>protocol:</c>
	/// </summary>
	public required String Protocol { get; init; }
	/// <summary>
	///		Base URL origin, will be passed to <see cref="RewriteCallback"/> and used in case
	///		of an unhandled exception
	/// </summary>
	public required String FallbackOrigin { get; init; }
	/// <summary>
	///		Function that emits a string that will replace the source URL. In the
	///		case of an unhandled exception, the string <c>$"{FallbackOrigin}/{arg1}"</c> will
	///		be used instead.
	///	<param name="RewriteCallback arg1">
	///		URL from the Markdown source, with the leading <c>protocol:</c> stripped
	/// </param>
	///	<param name="RewriteCallback arg2">
	///		<see cref="FallbackOrigin"/>
	/// </param>
	///	<returns>
	///		Replacement URL (unchecked, can be any string)
	/// </returns>
	/// </summary>
	public required Func<String, String, String> RewriteCallback {
		get;
		init {
			field = (link, fallbackOrigin) => {
				// catch-all wrapper so that a bad handler doesn't prevent a page render
				try {
					return value(link, fallbackOrigin);
				} catch (Exception ex) {
					this._logger.Error(ex, "Fake link protocol {Protocol} threw an exception for link {link}!",
						this.Protocol,
						link);
					
					return $"{fallbackOrigin}/{link}";
				}
			};
		}
	}
}

public class FakeLinkProtocolExtension : IMarkdownExtension {
	public readonly List<FakeLinkProtocolOptions> Options;
	
	public FakeLinkProtocolExtension(IList<FakeLinkProtocolOptions> options) {
		this.Options = options.ToList();
	}
	
	public void Setup(MarkdownPipelineBuilder pipeline) {
		pipeline.DocumentProcessed += document => document.Descendants<LinkInline>()
			.ForEach(link => {
				if (link.Url == null || link.Url.StartsWith("https:") || link.Url.StartsWith("http:"))
					return;
				
				this.Options.ForEach(proto => {
					if (link.Url.StartsWith($"{proto.Protocol}:", StringComparison.InvariantCultureIgnoreCase))
						link.Url = proto.RewriteCallback(link.Url.Substring(proto.Protocol.Length + 1), proto.FallbackOrigin);
				});
			});
	}
	
	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
}

public static class MarkdownPipelineExtensions {
	extension(MarkdownPipelineBuilder pipeline) {
		public MarkdownPipelineBuilder UseLocalLinkFixer(LocalLinkFixerOptions options) {
			pipeline.Extensions.ReplaceOrAdd<LocalLinkFixerExtension>( new LocalLinkFixerExtension(options) );
			return pipeline;
		}
		
		/// <summary>
		///		Add a <see cref="FakeLinkProtocolExtension"/> step to the pipeline.
		/// </summary>
		/// <param name="options">
		///		List of <see cref="FakeLinkProtocolOptions"/>' that will be successively
		///		applied to all <see cref="LinkInline"/>s in the Markdown document.
		///		Duplicates, two <see cref="FakeLinkProtocolOptions"/> with the same
		///		<see cref="FakeLinkProtocolOptions.Protocol"/>, will be logged and
		///		ignored.
		/// </param>
		/// <exception cref="NotSupportedException">
		///		Upon first invocation of this method, it *must* be performed before
		///		<see cref="LocalLinkFixerExtension"/> is added to the pipeline, or it will
		///		consume and modify the URLs before the 
		///		<see cref="FakeLinkProtocolExtension"/> can. *After* doing so, this method
		///		can be called an unlimited number of times.
		/// </exception>
		/// <returns>
		///		<see cref="MarkdownPipelineBuilder"/> for further modification or
		///		building.
		/// </returns>
		public MarkdownPipelineBuilder UseFakeLinkProtocolExtension(FakeLinkProtocolOptions[] options) {
			pipeline.Extensions.TryFind(out FakeLinkProtocolExtension? ext);
			
			if (pipeline.Extensions.Contains<LocalLinkFixerExtension>() && ext != null)
				throw new NotSupportedException("Applying the FakeLinkProtocolExtension for the first time must be done before the LocalLinkFixerExtension is added (subsequent calls are fine)");
			
			// add new options if not duplicate
			if (ext != null) {
				var originalProtos = ext.Options.Select(op => op.Protocol).ToList();
				
				options.ForEach(newProto => {
					if (originalProtos.Contains(newProto.Protocol)) {
						Console.WriteLine($"Tried to add duplicate fake protocol {newProto.Protocol}, ignoring");
						return;
					}
					
					ext.Options.Add(newProto);
				});
			} else {
				pipeline.Extensions.Add( new FakeLinkProtocolExtension(options) );				
			}
			
			return pipeline;
		}
	}
}