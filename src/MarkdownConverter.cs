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
using Markdig.Syntax;
using Microsoft.VisualBasic.CompilerServices;

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
			FallbackOrigin = "https://twitter.com",
			RewriteCallback = (link, origin) => {
				if (link.Length == 0 || link.Length == 1)
					return $"{origin}/{link}";
				
				String rest = link[1..];
				return link[0] switch {
					'@' => $"{origin}/@{rest}",
					'#' => $"{origin}/hashtag/{rest}",
					'!' => $"{origin}/i/status/{rest}",
					_   => $"{origin}/{link}"
				};
			}
		},
		new FakeLinkProtocolOptions {
			Protocol = "youtube",
			FallbackOrigin = "https://www.youtube.com",
			RewriteCallback = (link, origin) => {
				if (link.Length == 0 || link.Length == 1)
					return origin;
				
				if (link[0] == '@')
					return $"{origin}/@{link[1..]}";
				
				var split = link.Split("=", 2);
				if (split.Length != 2)
					return $"{origin}/{link}";
				
				String rest = split[1];
				return split[0] switch {
					"v" => $"{origin}/watch?v={rest}",
					"playlist" => $"{origin}/@{rest}",
					"search" => $"{origin}/results?search_query={rest}",
					_   => $"{origin}/{link}"
				};
			}
		},
		new FakeLinkProtocolOptions {
			Protocol = "github",
			FallbackOrigin = "https://github.com",
			RewriteCallback = (link, origin) => $"{origin}/{link}"
		},
		new FakeLinkProtocolOptions {
			Protocol = "nhnd-forgejo",
			FallbackOrigin = "https://git.helpimnotdrowning.net",
			RewriteCallback = (link, origin) => $"{origin}/helpimnotdrowning/{link}"
		},
		new FakeLinkProtocolOptions {
			Protocol = "tangled",
			FallbackOrigin = "https://tangled.org",
			RewriteCallback = (link, origin) => $"{origin}/{link}"
		},
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
