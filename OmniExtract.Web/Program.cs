using OmniExtract.App.Services;
using OmniExtract.Core.Config;
using OmniExtract.Web.Components;
using OmniExtract.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<ProcessingSettings>(builder.Configuration.GetSection("Processing"));
builder.Services.Configure<PathSettings>(builder.Configuration.GetSection("Paths"));

builder.Services.AddSingleton<GptClient>();
builder.Services.AddSingleton<TokenCounter>();
builder.Services.AddSingleton<LibreOfficeBridge>();
builder.Services.AddSingleton<DocumentProcessor>();
builder.Services.AddSingleton<ArchiveHandler>();
builder.Services.AddSingleton<ExtractionService>();
builder.Services.AddSingleton<OutputWriter>();

builder.Services.AddSingleton<ResultsRepository>();
builder.Services.AddSingleton<ExtractionOrchestrator>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
