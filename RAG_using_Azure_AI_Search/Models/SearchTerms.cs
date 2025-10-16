namespace RAG_using_Azure_AI_Search.Models
{
    public class SearchTerms
    {
        public string Input { get; set; }

        public int TopK { get; set; } = 3;    

        public string Response { get; set; }
    }
}
