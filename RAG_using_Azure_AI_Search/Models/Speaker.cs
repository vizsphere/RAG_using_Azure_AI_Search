using Azure.Search.Documents.Indexes;
using System.Text.Json.Serialization;

namespace _7_ElasticSearch_VectorStore_SemanticKernel.Models
{
    public class Speaker
    {

        [SearchableField(IsFilterable = true)]
        [JsonPropertyName("chunk")]
        public string Chunk { get; set; }


        [SimpleField(IsFilterable = true, IsSortable = true)]
        [JsonPropertyName("text_vector")]
        public IList<float> TextVector { get; set; }
    }
}
