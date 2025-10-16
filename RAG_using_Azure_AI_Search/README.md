## Azure AI Search with Retrieval-Augmented Generation (RAG)

This repository demonstrates how to implement Retrieval-Augmented Generation (RAG) using Azure AI Search and OpenAI's GPT models. The project showcases how to enhance the capabilities of language models by integrating them with a powerful search engine to retrieve relevant documents and generate more accurate and context-aware responses.


## Infrastructure Setup

Create the necessary Azure resources 

```
- [Resource Group] VizSphereDemoAzureAISearch 
	 	
		- [AI Foundry] vizspheredemoopenai (Azure AI Foundry)

			   - ProjectName = AISearchRAG
		
			   - [Model] gpt-35-turbo (Base Model Deployment) Token = 20k
			
			   - [Embedding] text-embedding-ada-002 (Embedding Model)
	
		- [Storage] vizspheredemostd (Azure Storage Account)
				- Container = data (Blob Container)
				- Upload sample data speakers.csv 

		- [AI Search] vizspheredemoazureaisearch (Azure AI Search)
```


## Prerequisites	
- An Azure account with access to create resources.
- .NET 6.0 SDK or later installed on your machine.
- Visual Studio 2022 or later (optional but recommended).


[sample data]: # (Path: RAG_using_Azure_AI_Search/Data/sample-data.csv)]

	1. Upload to Azure Storage Account (vizspheredemostd) in a container named 'speaker'.
	2. Azure AI Search (vizspheredemoazureaIsearch) will index this data.
	3. Semantic Kernel will be used for enhanced search capabilities.




	https://medium.com/data-science-collective/step-by-step-guide-on-building-agentic-rag-with-microsoft-semantic-kernel-and-azure-ai-search-3dcee5bf38ba