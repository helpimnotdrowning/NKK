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

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using ILogger = Serilog.ILogger;

namespace NKK;

public class ExceptionCatchingMiddleware(RequestDelegate next) {
	private readonly ILogger _logger = Log.Logger.ForContext<ExceptionCatchingMiddleware>();
	public static String Name_ItemTriggered = "nhnd_ExceptionMiddlewareTriggered";
	
	public async Task InvokeAsync(HttpContext context) {
		if (context.Features.Get<IStatusCodeReExecuteFeature>() != null) {
			await next(context);
			return;
		}

		try {
			await next(context);
		} catch (Exception e) {
			var oMethod = context.Request.Method;
			var oPath = context.Request.Path;
			var oQuery = context.Request.QueryString;
			var oEndpoint = context.GetEndpoint();
			var oRouteValues = context.Features.Get<IRouteValuesFeature>()?.RouteValues;
			var oRequestServices = context.RequestServices;
			
			if (e is StatusCodeException se)
				context.Response.StatusCode = se.StatusCode;
			else
				context.Response.StatusCode = 500;
			
			context.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature {
				Error = e,
				Path = oPath,
				Endpoint = oEndpoint,
				RouteValues = oRouteValues,
			});
			context.Features.Set<IStatusCodeReExecuteFeature>(new StatusCodeReExecuteFeature {
				OriginalPath = oPath,
				OriginalQueryString = oQuery.Value,
				Endpoint = oEndpoint,
				RouteValues = oRouteValues,
			});

			context.SetEndpoint(null);
			context.Features.Set<IRouteValuesFeature>(null);

			context.Request.Method = HttpMethod.Get.ToString();
			context.Request.Path = new PathString("/Error"); // TODO configurable
			context.Request.QueryString = QueryString.Empty;
			context.Items[Name_ItemTriggered] = true;

			var scopeFactory = oRequestServices.GetRequiredService<IServiceScopeFactory>();
			await using var scope = scopeFactory.CreateAsyncScope();
			context.RequestServices = scope.ServiceProvider;

			try {
				await next(context);
			} catch (Exception ex) {
				this._logger.Error(ex, "Rendering the Error page threw an exception!");
			} finally {
				context.Request.Method = oMethod;
				context.Request.Path = oPath;
				context.Request.QueryString = oQuery;
				context.Features.Set<IStatusCodeReExecuteFeature>(null);
				context.RequestServices = oRequestServices;
			}
		}
	}
}

/// <summary>
///		Exception class to trigger an error page anywhere 
/// </summary>
public class StatusCodeException : Exception {
	public readonly int StatusCode;
	
	/// <param name="statusCode">
	///		Error status
	/// </param>
	/// <exception cref="ArgumentOutOfRangeException">
	///		Thrown if <see cref="statusCode"/> does not indicate an error (not <c>400 &lt;= statusCode &lt; 600</c>);
	/// </exception>
	public StatusCodeException(int statusCode) {
		if (statusCode < 400 || statusCode >= 600)
			throw new ArgumentOutOfRangeException(nameof(statusCode));
		this.StatusCode = statusCode;
	}
	
}
