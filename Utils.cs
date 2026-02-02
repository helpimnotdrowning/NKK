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