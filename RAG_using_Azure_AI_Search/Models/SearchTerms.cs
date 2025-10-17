﻿using System.Net;

namespace RAG_using_Azure_AI_Search.Models
{
    public class SearchTerms
    {
        public string Input { get; set; }

        public int TopK { get; set; } = 2;    
    }

    public class SearchResult
    {
        public string Response { get; set; } = string.Empty;
    }
}
