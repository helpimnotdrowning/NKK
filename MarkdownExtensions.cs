using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using NUglify.Helpers;

namespace NKK;

public class LocalLinkFixerOptions {
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
	public required String Protocol { get; init; }
	public required String FallbackOrigin { get; init; }
	public required Func<String, String, String> RewriteCallback {
		get;
		init {
			field = (link, fallbackOrigin) => {
				// catch-all wrapper so that a bad handler doesn't prevent a page render
				try {
					return value(link, fallbackOrigin);
				} catch (Exception ex) {
					Console.WriteLine($"Fake link protocol '{this.Protocol}' threw an exception for link '{link}'!");
					Utils.WriteException(ex);

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

		public MarkdownPipelineBuilder UseFakeLinkProtocolExtension(FakeLinkProtocolOptions[] newOptions) {
			pipeline.Extensions.TryFind(out FakeLinkProtocolExtension? ext);
			
			if (pipeline.Extensions.Contains<LocalLinkFixerExtension>() && ext != null)
				throw new NotSupportedException("Applying the FakeLinkProtocolExtension for the first time must be done before the LocalLinkFixerExtension is added (subsequent calls are fine)");

			// add new options if not duplicate
			if (ext != null) {
				var originalProtos = ext.Options.Select( op => op.Protocol).ToList();

				newOptions.ForEach(newProto => {
					if (originalProtos.Contains(newProto.Protocol)) {
						Console.WriteLine($"Tried to add duplicate fake protocol {newProto.Protocol}, ignoring");
						return;
					}
					
					ext.Options.Add(newProto);
				});
			} else {
				pipeline.Extensions.Add( new FakeLinkProtocolExtension(newOptions) );				
			}
			
			return pipeline;
		}
	}
}