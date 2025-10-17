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

        public AzureAISearchPlugin(SearchIndexClient searchIndexClient, ITextEmbeddingGenerationService textEmbeddingGenerationService)
        {
            _searchIndexClient = searchIndexClient;
            _textEmbeddingGenerationService = textEmbeddingGenerationService;
        }

        [KernelFunction("Search")]
        [Description("Search for a speakers.")]
        public async Task<string> SearchAsync(string query)
        {
            var _searchClient = _searchIndexClient.GetSearchClient("rag-1760631170669");

            ReadOnlyMemory<float> embedding = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(query);

            var vectorQuery = new VectorizedQuery(embedding)
            {
                KNearestNeighborsCount = 2,
                Fields = { "text_vector" }
            };

            var options = new SearchOptions
            {
                VectorSearch = new VectorSearchOptions
                {
                    Queries = { vectorQuery }
                },
                Size = 2
            };

            var response = await _searchClient.SearchAsync<Speaker>(null, options);

            await foreach (var result in response.Value.GetResultsAsync())
            {
                return result.Document.Chunk ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
