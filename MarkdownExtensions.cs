using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using NUglify.Helpers;

namespace NKK;

public class LocalLinkFixerOptions {
	public String Base { get; set; }
	
	public LocalLinkFixerOptions() {
		this.Base = "/";
	}
}

public class LocalLinkFixerExtension : IMarkdownExtension {
	private LocalLinkFixerOptions Options { get; }
	
	public LocalLinkFixerExtension() : this(new LocalLinkFixerOptions()) {}

	public LocalLinkFixerExtension(LocalLinkFixerOptions? options) {
		this.Options = options ?? new LocalLinkFixerOptions();
	}
	
	public void Setup(MarkdownPipelineBuilder pipeline) {
		pipeline.DocumentProcessed += document => document.Descendants<LinkInline>()
			.ForEach(link => {
				if (link.Url != null && !Utils.IsAbsoluteUrl(link.Url))
					link.Url = Path.Combine(this.Options.Base, link.Url);
			});
	}
	
	public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
}

public static class MarkdownPipelineExtensions {
	extension(MarkdownPipelineBuilder pipeline) {
		public MarkdownPipelineBuilder UseLocalLinkFixer(LocalLinkFixerOptions? options) {
			pipeline.Extensions.ReplaceOrAdd<LocalLinkFixerExtension>( new LocalLinkFixerExtension(options) );
			return pipeline;
		}
	}
}