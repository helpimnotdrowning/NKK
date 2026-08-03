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

using System.IO.Compression;
using System.Text;

using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

using NKK;
using NKK.Components;

String allPostsRoot = Environment.GetEnvironmentVariable("ALL_POSTS_ROOT") ??
	throw new ArgumentException("env:ALL_POSTS_ROOT is unset!");

var builder = WebApplication.CreateBuilder(new WebApplicationOptions() {
	Args = args,
	WebRootPath = "public"
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents();
builder.Services.AddResponseCompression(options => {
	options.EnableForHttps = true;
	options.Providers.Add<BrotliCompressionProvider>();
	options.Providers.Add<GzipCompressionProvider>();
	options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
} );
builder.Services.Configure<BrotliCompressionProviderOptions>(options => {
	options.Level = CompressionLevel.SmallestSize;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options => {
	options.Level = CompressionLevel.SmallestSize;
});

builder.Services.AddSingleton<PostStore>();
builder.Services.Configure<PostWatcherOptions>(opts => {
	opts.AllPostsRoot = new DirectoryInfo(Environment.GetEnvironmentVariable("ALL_POSTS_ROOT") ??
		throw new ArgumentException("env:ALL_POSTS_ROOT is unset!"));
});
builder.Services.AddHostedService<PostWatcher>();
builder.Services.AddScoped<HeadAccumulator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
	app.UseDeveloperExceptionPage();
} else {
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();
app.UseResponseCompression();
app.Use(async (context, next) => {
	// context.Response.Body is a direct line to the client, so
	// swap it out for our own in-memory stream for now
	Stream responseStream = context.Response.Body;
	using var memoryStream = new MemoryStream();
	context.Response.Body = memoryStream;
	
	// let downstream render the response & write to our stream
	await next(context);
	
	if (context.Response.ContentType?.StartsWith("text/html") != true) {
		// oops my bad gangalang
		// ok now put it back
		memoryStream.Position = 0;
		await memoryStream.CopyToAsync(responseStream);
		context.Response.Body = responseStream;
		
		return;
	}
	
	memoryStream.Position = 0;
	String html = await new StreamReader(memoryStream).ReadToEndAsync();
	String minified = Utils.OptimizeHtml(html);
	
	context.Response.ContentLength = Encoding.UTF8.GetByteCount(minified);
	await responseStream.WriteAsync(Encoding.UTF8.GetBytes(minified));
	context.Response.Body = responseStream;
});

var validFileTypes = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase) {
	{ ".png", "image/png" },
	{ ".jpg", "image/jpeg" },
	{ ".jpeg", "image/jpeg" },
	{ ".md", "text/markdown" },
};

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions {
	FileProvider = new PhysicalFileProvider(Path.Combine(allPostsRoot, SayingPayload.PathFragment)),
	RequestPath = "/sayings",
	ServeUnknownFileTypes = false,
	ContentTypeProvider = new FileExtensionContentTypeProvider(validFileTypes),
});
app.UseStaticFiles(new StaticFileOptions {
	FileProvider = new PhysicalFileProvider(Path.Combine(allPostsRoot, ArtifactPayload.PathFragment)),
	RequestPath = "/museum",
	ServeUnknownFileTypes = false,
	ContentTypeProvider = new FileExtensionContentTypeProvider(validFileTypes),
});

app.MapRazorComponents<App>();

app.Run();