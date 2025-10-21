using _7_ElasticSearch_VectorStore_SemanticKernel.Models;
using Azure.AI.OpenAI.Chat;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using RAG_using_Azure_AI_Search.Models;
using System.Reflection;
using System.Text;

#pragma warning disable SKEXP0001
#pragma warning disable AOAI001
#pragma warning disable SKEXP0010

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
                Fields = { _settings.AzureSearch.VectorField }
            };

            var options = new SearchOptions
            {
                VectorSearch = new VectorSearchOptions
                {
                    Queries = { vectorQuery }
                },
                Size = search.TopK
            };

            var response = await _searchClient.SearchAsync<Speaker>(options);

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
            var sb = new StringBuilder();
            IReadOnlyList<ChatCitation> citations = null;
            var searchResult = new SearchResult();

            var promptExecutionSettings = new AzureOpenAIPromptExecutionSettings { AzureChatDataSource = GetAzureSearchDataSource(), ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

            var systemMessage = "You are a knowledgeable agent specialised in retrieving data using Azure AI Search."
                              + "For user."
                              + "Search required information in Azure AI Search.";

            var chatHistory = new ChatHistory(systemMessage);
            chatHistory.AddUserMessage(search.Input);

            _logger.LogInformation("Agentic AI Search Prompt: {prompt}", chatHistory);

            var assistantReply = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, promptExecutionSettings, _kernel);

            _logger.LogInformation("Agentic AI Search Assistant Reply: {assistantReply}", assistantReply.ToString());

            chatHistory.AddAssistantMessage(assistantReply.ToString()); 

            if (assistantReply == null || assistantReply.ToString().Trim().Length == 0)
            {
                searchResult.Response = "No results found.";

                return Ok(searchResult);
            }

            if (search.IncludeCitations)
            {
                citations = GetCitations(assistantReply);
            }

            searchResult.Response = GetSearchText(citations, assistantReply);

            return Ok(searchResult);
        }


        #region Private Methods
        
        private AzureSearchChatDataSource GetAzureSearchDataSource()
        {
            return new AzureSearchChatDataSource
            {
                Endpoint = new Uri(_settings.AzureSearch.Endpoint),
                Authentication = DataSourceAuthentication.FromApiKey(_settings.AzureSearch.ApiKey),
                IndexName = _settings.AzureSearch.Index
            };
        }

        private IReadOnlyList<ChatCitation> GetCitations(ChatMessageContent chatMessageContent)
        {
            var message = chatMessageContent.InnerContent as OpenAI.Chat.ChatCompletion;
            var messageContext = message.GetMessageContext();

            return messageContext.Citations;
        }

        private string GetSearchText(IReadOnlyList<ChatCitation> citations, ChatMessageContent chatMessageContent)
        {
            var sb = new StringBuilder();


            sb.AppendLine("Search results:");
            sb.AppendLine(chatMessageContent.ToString());
            sb.AppendLine("\n");

            if(citations != null)
            {
                sb.AppendLine("-----------");
                sb.AppendLine(" Citations:");
                sb.AppendLine("-----------");

                int count = 1;
                foreach (var citation in citations)
                {
                    sb.Append(count.ToString());
                    sb.AppendLine($"- Source: {citation.Title}: {citation.FilePath}: {citation.Url}");
                    sb.AppendLine($"- Content: {citation.Content}");
                    sb.AppendLine($"\n");
                    count += 1;
                }
            }

            return sb.ToString();
        }

        #endregion
    }
}
