using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;
using RAG_using_Azure_AI_Search.Models;
using System.Diagnostics;

#pragma warning disable SKEXP0010 
#pragma warning disable SKEXP0020 
#pragma warning disable SKEXP0001 
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
        //private readonly AzureAISearchCollection<string, Speaker> _collection;
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
            //_collection = _kernel.GetRequiredService<AzureAISearchCollection<string, Speaker>>();
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new SearchTerms());
        }

        [HttpPost]
        public async Task<IActionResult> Index(SearchTerms search)
        {
            //ReadOnlyMemory<float> query = await _textEmbeddingGenerationService.GenerateEmbeddingsAsync(new[] { search.Input });
            
            //var vectorQuery = new VectorizedQuery(query)
            //{
            //    KNearestNeighborsCount = 3,
            //    Fields = { "text_vector" }
            //};


            return View(search);
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
