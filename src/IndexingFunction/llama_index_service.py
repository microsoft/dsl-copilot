from llama_index.llms.azure_openai import AzureOpenAI
from llama_index.core import (StorageContext, VectorStoreIndex)
from llama_index.core.settings import Settings
from llama_index.core.vector_stores.types import VectorStore
from llama_index.readers.azstorage_blob import AzStorageBlobReader
from llama_index.embeddings.azure_openai import AzureOpenAIEmbedding

class LlamaIndexService:

    # Index the documents
    def index_documents(self, 
        aoai_model_name: str, 
        aoai_api_key: str, 
        aoai_endpoint: str, 
        aoai_api_version: str,
        vector_store: VectorStore,
        embed_model: AzureOpenAIEmbedding,
        blob_loader: AzStorageBlobReader
    ) -> VectorStoreIndex:

        documents = blob_loader.load_data()
        llm = AzureOpenAI(
            model=aoai_model_name,
            deployment_name=aoai_model_name,
            api_key=aoai_api_key,
            azure_endpoint=aoai_endpoint,
            api_version=aoai_api_version,
        )
        
        storage_context = StorageContext.from_defaults(vector_store=vector_store)
        
        Settings.llm = llm
        Settings.embed_model = embed_model

        index = VectorStoreIndex.from_documents(
            documents, storage_context=storage_context
        )

        return index
    