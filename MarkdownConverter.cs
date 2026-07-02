using Markdig;
using Markdig.Syntax;

namespace NKK;

public class MarkdownOptions {
	public String LocalLinkBase { get; set; } = "/";
}

public class MarkdownConverter {
	private readonly MarkdownPipeline _pipeline;
	private readonly MarkdownOptions _options;
	private readonly String _source;
	
	private const String ErrNotParsed = "Markdown document not parsed. Set md or use Parse(String;)V";

	private static readonly FakeLinkProtocolOptions[] fakeLinkProtocolOptions = [
		new FakeLinkProtocolOptions {
			Protocol = "twitter",
			RewriteCallback = link => {
				const String origin = "https://twitter.com"; 
				String rest = link[1..];

				return link[0] switch {
					'@' => $"{origin}/@{rest}",
					'#' => $"{origin}/hashtag/{rest}",
					'!' => $"{origin}/i/status/{rest}",
					_   => $"{origin}/{link}"
				};
			}
		}
	];

	private MarkdownDocument Document {
		get => field ?? throw new NullReferenceException(ErrNotParsed);
		set => field = value ?? throw new NullReferenceException(ErrNotParsed);
	} = null;
	
	public MarkdownConverter(String markdownString, MarkdownOptions options) {
		this._source = markdownString;
		this._options = options;
		this._pipeline = new MarkdownPipelineBuilder()
			.UseAdvancedExtensions()
			.UseFakeLinkProtocolExtension(fakeLinkProtocolOptions)
			.UseLocalLinkFixer(new LocalLinkFixerOptions {
				Base = this._options.LocalLinkBase
			})
			.Build();
		this.Document = Markdown.Parse(markdownString, this._pipeline);
	}

	public String ToHtml() {
		return this.Document.ToHtml(this._pipeline);
	}
}
