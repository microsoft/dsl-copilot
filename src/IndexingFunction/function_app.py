import logging
import os
import uuid

import azure.functions as func
from openai import AzureOpenAI
from azure.core.credentials import AzureKeyCredential
from azure.search.documents.indexes import SearchIndexClient
from azure.search.documents import SearchClient
from azure.search.documents.indexes.models import SearchIndex
from azure.search.documents.indexes.models import (
    ComplexField,
    CorsOptions,
    SearchIndex,
    ScoringProfile,
    SearchFieldDataType,
    SimpleField,
    VectorSearch,
    SearchField,
    SearchableField,
    VectorSearchAlgorithmConfiguration,
    VectorSearchProfile,
    HnswAlgorithmConfiguration
)
from llama_index.vector_stores.azureaisearch import AzureAISearchVectorStore
from llama_index.vector_stores.azureaisearch import IndexManagement
from llama_index.core.settings import Settings
from llama_index.readers.azstorage_blob import AzStorageBlobReader
from llama_index.embeddings.azure_openai import AzureOpenAIEmbedding

from llama_index_service import LlamaIndexService

app = func.FunctionApp()

@app.blob_trigger(arg_name="indexBlob", path="dsl-content",
                               connection="function_storage_connection") 
def blob_trigger(indexBlob: func.InputStream):
    llama_index_service: LlamaIndexService = LlamaIndexService()
    blob_name: str = indexBlob.name.split("/")[-1]    
    aoai_model_name: str = os.environ["OpenAIModelName"]
    aoai_api_key: str = os.environ["OpenAIAPIKey"]
    aoai_endpoint: str = os.environ["OpenAIEndpoint"]
    aoai_api_version: str = os.environ["OpenAIAPIVersion"]
    embed_model = __create_embedding_model__()
    vector_store = __create_vector_store__()
    blob_loader = __create_blob_loader__(blob_name)
    
    llama_index_service.index_documents(
        aoai_model_name,
        aoai_api_key,
        aoai_endpoint,
        aoai_api_version,
        vector_store,
        embed_model,
        blob_loader
    )

# Create the Azure AI Search Index
def __create_search_index__() -> SearchIndexClient: 
    search_service_endpoint: str = os.environ["AISearchEndpoint"]
    search_service_api_key: str = os.environ["AISearchAPIKey"]
    credential = AzureKeyCredential(search_service_api_key)
    
    return SearchIndexClient(
        endpoint=search_service_endpoint,
        credential=credential,
    )

# Create the Azure AI Search Vector Store
def __create_vector_store__() -> AzureAISearchVectorStore:
    search_index_name: str = os.environ["AISearchIndexName"]
    index_client: SearchIndexClient = __create_search_index__()
    metadata_fields = {}

    return AzureAISearchVectorStore(
        search_or_index_client=index_client,
        filterable_metadata_field_keys=metadata_fields,
        index_name=search_index_name,
        index_management=IndexManagement.CREATE_IF_NOT_EXISTS,
        id_field_key="id",
        chunk_field_key="chunk",
        embedding_field_key="embedding",
        embedding_dimensionality=1536,
        metadata_string_field_key="metadata",
        doc_id_field_key="doc_id",
        language_analyzer="en.lucene",
        vector_algorithm_type="exhaustiveKnn",
    )

# Create the Azure OpenAI Embedding Model
def __create_embedding_model__() -> AzureOpenAIEmbedding:
    aoai_api_key: str = os.environ["OpenAIAPIKey"]
    aoai_endpoint: str = os.environ["OpenAIEndpoint"]
    aoai_api_version: str = os.environ["OpenAIAPIVersion"]
    aoai_embedding_model_name: str = os.environ["OpenAIEmbeddingModelName"]

    embed_model = AzureOpenAIEmbedding(
        model=aoai_embedding_model_name,
        deployment_name=aoai_embedding_model_name,
        api_key=aoai_api_key,
        azure_endpoint=aoai_endpoint,
        api_version=aoai_api_version,
    )
    
    return embed_model

# Create the Azure Blob Loader
def __create_blob_loader__(blob_name: str) -> AzStorageBlobReader:
    container_name: str = os.environ["StorageAccountContainerName"]
    account_url: str = os.environ["StorageAccountUrl"]
    connection_string: str = os.environ["StorageAccountConnectionString"]
    
    return AzStorageBlobReader(
        container_name=container_name,
        blob=blob_name,
        account_url=account_url,
        connection_string=connection_string,
    )



@app.blob_trigger(arg_name="myblob", path="code-content",
                               connection="function_storage_connection") 
def code_indexing(myblob: func.InputStream):
    logging.info(f"Python blob trigger function processed blob"
                f"Name: {myblob.name}"
                f"Blob Size: {myblob.length} bytes")
    
    indexName = os.environ["AISeachCodeIndexName"]
    searchIndexClient = __create_search_index__()
    indexes = searchIndexClient.list_index_names()
    
    if indexName not in indexes:
        __create_code_index(searchIndexClient, indexName)

    __insert_code_documents(searchIndexClient)

def __create_code_index(searchIndexClient: SearchIndexClient, indexName: str):
    vector_search = VectorSearch(
        profiles=[VectorSearchProfile(name="vector-config", algorithm_configuration_name="algorithms-config")],
        algorithms=[HnswAlgorithmConfiguration(name="algorithms-config")],
    )
    index = SearchIndex(
        name=os.environ["AISeachCodeIndexName"],
        fields=[
            SimpleField(name="id", type=SearchFieldDataType.String, key=True, sortable=True, filterable=True, facetable=True),
            SearchableField(name="prompt", type=SearchFieldDataType.String),
            SimpleField(name="response", type=SearchFieldDataType.String),
            SearchField(name="embedding", type=SearchFieldDataType.Collection(SearchFieldDataType.Single),
                        searchable=True, vector_search_dimensions=1536, vector_search_profile_name="vector-config"),
        ],
        vector_search=vector_search,
    )

    searchIndexClient.create_index(index)

def __insert_code_documents(searchIndexClient: SearchIndexClient):
    search_client = searchIndexClient.get_search_client(os.environ["AISeachCodeIndexName"])
    client = AzureOpenAI(
        azure_endpoint = os.environ["OpenAIEndpoint"], 
        api_key=os.environ["OpenAIAPIKey"],  
        api_version=os.environ["OpenAIAPIVersion"]
    )
    
    document_id = str(uuid.uuid4())
    embedding = client.embeddings.create(input="Hello Worl", model=os.environ["OpenAIEmbeddingModelName"]).data[0].embedding
    DOCUMENT = {
        "id": document_id,
        "prompt": "Hello World",
        "response": "Testing",
        "embedding": embedding,
    }

    result = search_client.upload_documents(documents=[DOCUMENT])
    logging.info(f"Upload of new document with id {document_id} succeeded: {result[0].succeeded}")