using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.SemanticKernel;
using RAG_using_Azure_AI_Search.Models;

#pragma warning disable SKEXP0010 


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Load AppSettings from configuration
var appSettings = new AppSettings();
builder.Configuration.GetSection("AppSettings").Bind(appSettings);


builder.Services.AddSingleton<Kernel>(s =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    //Chat Completion Service
    kernelBuilder.AddAzureOpenAIChatCompletion(appSettings.AzureOpenAIChatCompletion.Model, appSettings.AzureOpenAIChatCompletion.Endpoint, appSettings.AzureOpenAIChatCompletion.ApiKey);

    //Text Embedding Service
    kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(appSettings.AzureOpenAITextEmbedding.Model, appSettings.AzureOpenAITextEmbedding.Endpoint, appSettings.AzureOpenAITextEmbedding.ApiKey);


    kernelBuilder.Services.AddSingleton<SearchIndexClient>(sp =>
        new SearchIndexClient(new Uri(appSettings.AzureSearch.Endpoint), new AzureKeyCredential(appSettings.AzureSearch.ApiKey))
    );

    //Vector Store
    kernelBuilder.Services.AddAzureAISearchVectorStore(); ;

    /*
        kernelBuilder.Services.AddSingleton<AzureAISearchCollection<string, Speaker>>(sp =>
            new AzureAISearchCollection<string, Speaker>(
                new SearchIndexClient(new Uri(appSettings.AzureSearch.Endpoint), new AzureKeyCredential(appSettings.AzureSearch.ApiKey)),
                appSettings.AzureSearch.Index
            )
        );
    */

    return kernelBuilder.Build();
});

builder.Services.AddSingleton<AppSettings>(appSettings);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
