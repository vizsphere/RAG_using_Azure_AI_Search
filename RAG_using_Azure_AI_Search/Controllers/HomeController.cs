using _7_ElasticSearch_VectorStore_SemanticKernel.Models;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using RAG_using_Azure_AI_Search.Models;

#pragma warning disable SKEXP0001
namespace RAG_using_Azure_AI_Search.Controllers
{
    public class HomeController : Controller
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;
        private readonly SearchIndexClient _indexClient;
        private readonly SearchClient _searchClient;
        private readonly AppSettings _settings;
        private readonly ILogger<HomeController> _logger;

        public HomeController(Kernel kernel,  ILogger<HomeController> logger)
        {
            _kernel = kernel;
            _settings = _kernel.GetRequiredService<AppSettings>();
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            _textEmbeddingGenerationService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            _indexClient = _kernel.GetRequiredService<SearchIndexClient>();
            _searchClient = _indexClient.GetSearchClient(_settings.AzureSearch.Index);
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new SearchTerms());
        }


        [HttpPost]
        [Route("ai-search")]
        public async Task<IActionResult> AISearch([FromBody] SearchTerms search)
        {
            var result = new SearchResult();

            if (search.Input == null || search.Input.Trim().Length == 0)
            {
                result.Response = "Please enter some search terms.";

                return BadRequest(result);
            }

            ReadOnlyMemory<float> query = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(search.Input);

            var vectorQuery = new VectorizedQuery(query)
            {
                KNearestNeighborsCount = search.TopK,
                Fields = { "text_vector" }
            };

            var options = new SearchOptions
            {
                VectorSearch = new VectorSearchOptions
                {
                    Queries = { vectorQuery }
                },
                Size = search.TopK
            };

            var response = await _searchClient.SearchAsync<Speaker>(null, options);

            if (response == null || response.Value.GetResults().Count() == 0)
            {
                result.Response = "No results found.";

                return Ok(result);
            }

            await foreach (var r in response.Value.GetResultsAsync())
            {
                result.Response += r.Document.Chunk ?? string.Empty + "\n\n";
            }

            return Ok(result);
        }


        [HttpPost]
        [Route("agentic-ai-search")]
        public async Task<IActionResult> AgenticAISearch([FromBody] SearchTerms search)
        {
            var searchResult = new SearchResult();

            var settings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

            var systemMessage = "You are a knowledgeable agent specialised in retrieving data using Azure AI Search."
                              + "For user prompt"
                              + "Search the speakers in Azure AI Search";

            var chat = new ChatHistory(systemMessage);

            chat.AddUserMessage(search.Input);

            var assistantReply = await _chatCompletionService.GetChatMessageContentAsync(chat, settings, _kernel);

            if (assistantReply == null || assistantReply.ToString().Trim().Length == 0)
            {
                searchResult.Response = "No results found.";

                return Ok(searchResult);
            }

            searchResult.Response = assistantReply.ToString();

            return Ok(searchResult);
        }
    }
}
