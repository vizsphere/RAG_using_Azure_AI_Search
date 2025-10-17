using _7_ElasticSearch_VectorStore_SemanticKernel.Models;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;
using RAG_using_Azure_AI_Search.Models;
using System.Diagnostics;
#pragma warning disable SKEXP0001
namespace RAG_using_Azure_AI_Search.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ITextGenerationService _textGenerationService;
        private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;
        private readonly SearchIndexClient _indexClient;
        private readonly SearchClient _searchClient;
        private readonly AppSettings _settings;

        public HomeController(Kernel kernel, AppSettings settings, ILogger<HomeController> logger)
        {
            _settings = settings;
            _kernel = kernel;
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            _textGenerationService = _kernel.GetRequiredService<ITextGenerationService>();
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

            if(search.Input == null || search.Input.Trim().Length == 0)
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

            result.Response = response.Value.GetResults()
                .Select(r => r.Document)
                .Aggregate(string.Empty, (current, doc) => current + ("\n" + doc.Chunk + "\n\n ----"));
            
            return Ok(result);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
