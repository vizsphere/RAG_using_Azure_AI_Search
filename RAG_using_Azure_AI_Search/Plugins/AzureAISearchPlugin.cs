using _7_ElasticSearch_VectorStore_SemanticKernel.Models;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using RAG_using_Azure_AI_Search.Models;
using System.ComponentModel;

#pragma warning disable SKEXP0001
namespace RAG_using_Azure_AI_Search.Plugins
{

    public class AzureAISearchPlugin 
    {
        private readonly SearchIndexClient _searchIndexClient;
        private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<AzureAISearchPlugin> _logger;
        public AzureAISearchPlugin(SearchIndexClient searchIndexClient, ITextEmbeddingGenerationService textEmbeddingGenerationService, AppSettings appSettings, ILogger<AzureAISearchPlugin> logger)
        {
            _searchIndexClient = searchIndexClient;
            _textEmbeddingGenerationService = textEmbeddingGenerationService;
            _appSettings = appSettings;
            _logger = logger;
        }

        [KernelFunction("Search")]
        [Description("Search for a speaker")]
        public async Task<string> SearchAsync(string query)
        {
            _logger.LogInformation("AzureAISearchPlugin SearchAsync called with query: {query}", query);

            var _searchClient = _searchIndexClient.GetSearchClient(_appSettings.AzureSearch.Index);

            ReadOnlyMemory<float> embedding = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(query);

            var vectorQuery = new VectorizedQuery(embedding)
            {
                KNearestNeighborsCount = _appSettings.AzureSearch.TopK,
                Fields = { _appSettings.AzureSearch.VectorField }
            };

            var options = new SearchOptions
            {
                VectorSearch = new VectorSearchOptions
                {
                    Queries = { vectorQuery }
                },
                Size = _appSettings.AzureSearch.Size
            };

            var response = await _searchClient.SearchAsync<Speaker>(options);

            await foreach (var result in response.Value.GetResultsAsync())
            {
                return result.Document.Chunk ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
