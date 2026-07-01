using Markdig;
using Markdig.Syntax;

namespace NKK;

public class MarkdownConverter {
	private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
	private readonly String _source;
	private const String ErrNotParsed = "Markdown document not parsed. Set md or use Parse(String;)V";

	public MarkdownDocument Document {
		get => field ?? throw new NullReferenceException(ErrNotParsed);
		set => field = value ?? throw new NullReferenceException(ErrNotParsed);
	} = null;

	public MarkdownConverter(String markdownString) {
		this._source = markdownString;
		this.Document = Markdown.Parse(markdownString, this._pipeline);
	}

	public String ToHtml() {
		return this.Document.ToHtml(this._pipeline);
	}
}
