import logging
import os
import uuid
import json

import azure.functions as func
from openai import AzureOpenAI
from azure.identity import DefaultAzureCredential
from azure.storage.blob import BlobClient
from azure.search.documents.indexes import SearchIndexClient
from azure.search.documents.indexes.models import SearchIndex
from azure.search.documents.indexes.models import (
    SearchIndex,
    SearchFieldDataType,
    SimpleField,
    VectorSearch,
    SearchField,
    SearchableField,
    VectorSearchProfile,
    HnswAlgorithmConfiguration,
    HnswParameters
)

import yaml
from llama_index.vector_stores.azureaisearch import AzureAISearchVectorStore
from llama_index.vector_stores.azureaisearch import IndexManagement
from llama_index.core.settings import Settings
from llama_index.readers.azstorage_blob import AzStorageBlobReader
from llama_index.embeddings.azure_openai import AzureOpenAIEmbedding

from search_client_factory import SearchClientFactory
from llama_index_service import LlamaIndexService

app = func.FunctionApp()

@app.blob_trigger(arg_name="indexBlob", path="dsl-content",
                               connection="function_storage_connection") 
def blob_trigger(indexBlob: func.InputStream):
    logging.info(f"Indexing blob: {indexBlob.name}")
    
    llama_index_service: LlamaIndexService = LlamaIndexService()
    blob_name: str = indexBlob.name.split("/")[-1]    
    aoai_model_name: str = os.environ["OpenAIModelName"]
    aoai_api_key: str = os.environ["OpenAIAPIKey"]
    aoai_endpoint: str = os.environ["OpenAIEndpoint"]
    aoai_api_version: str = os.environ["OpenAIAPIVersion"]
    search_index_name: str = os.environ["AISearchIndexName"]
    embed_model = __create_embedding_model__()
    vector_store = __create_vector_store__(search_index_name)
    blob_loader = __create_blob_loader__(blob_name)
    
    logging.info(f"Storing chunk data in Azure AI Search Index: {search_index_name}")
    llama_index_service.index_documents(
        aoai_model_name,
        aoai_api_key,
        aoai_endpoint,
        aoai_api_version,
        vector_store,
        embed_model,
        blob_loader
    )

    __move_index_blob(indexBlob, blob_name)

# Create the Azure AI Search Vector Store
def __create_vector_store__(search_index_name: str) -> AzureAISearchVectorStore:
    search_service_endpoint: str = os.environ["AISearchEndpoint"]
    search_service_api_key: str = os.environ["AISearchAPIKey"]
    index_client: SearchIndexClient =  SearchClientFactory(search_service_endpoint, search_service_api_key).create_search_index_client()
    metadata_fields = {}

    logging.info(f"Creating Azure AI Search Vector Store: {search_index_name}")

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

    logging.info(f"Creating Azure OpenAI Embedding Model: {aoai_embedding_model_name}")

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
    
    logging.info(f"Creating Azure Blob Loader for container {container_name} and blob {blob_name}.")

    return AzStorageBlobReader(
        container_name=container_name,
        blob=blob_name,
        account_url=account_url,
        connection_string=connection_string,
    )

# Move the index blob from the DSL content container to the indexed container
def __move_index_blob(indexBlob: func.InputStream, blob_name: str):
    source_container_name: str = os.environ["StorageAccountContainerName"]
    destination_container_name: str = os.environ["IndexContainerName"]
    account_url = os.environ["StorageAccountUrl"]
    credential = DefaultAzureCredential()
    source_service_client = BlobClient(account_url, credential=credential, container_name=source_container_name, blob_name=blob_name)
    destination_blob_service_client = BlobClient(account_url, credential=credential, container_name=destination_container_name, blob_name=blob_name)

    logging.info(f"Uploading index blob {blob_name} to container {destination_container_name}.")
    destination_blob_service_client.upload_blob(indexBlob, blob_type="BlockBlob")

    logging.info(f"Deleting index blob {blob_name} from container {source_container_name}.")
    source_service_client.delete_blob()

@app.blob_trigger(arg_name="codeExamples", path="code-content",
                               connection="function_storage_connection") 
def code_indexing(codeExamples: func.InputStream):
   
    dsl_examples = yaml.safe_load(codeExamples)
    prompts = dsl_examples['prompts']
    language = dsl_examples['language']

    index_name = os.environ["AISeachCodeIndexName"]
    search_service_endpoint: str = os.environ["AISearchEndpoint"]
    search_service_api_key: str = os.environ["AISearchAPIKey"]
    index_client: SearchIndexClient =  SearchClientFactory(search_service_endpoint, search_service_api_key).create_search_index_client()
    indexes = index_client.list_index_names()
    
    if index_name not in indexes:
        __create_code_index(index_client, index_name)

    __insert_code_documents(index_client, prompts, language)

def __create_code_index(index_client: SearchIndexClient, indexName: str):
    hnsw_parameters = HnswParameters(metric= "cosine")
    vector_search = VectorSearch(
        profiles=[VectorSearchProfile(name="vector-config", algorithm_configuration_name="algorithms-config")],
        algorithms=[HnswAlgorithmConfiguration(name="algorithms-config", parameters=hnsw_parameters)],
    )
    index = SearchIndex(
        name=os.environ["AISeachCodeIndexName"],
        fields=[
            SearchField(name="id", type=SearchFieldDataType.String, key=True, searchable=True, filterable=True, sortable=False, facetable=False),
            SimpleField(name="tags",  type=SearchFieldDataType.Collection(SearchFieldDataType.String), filterable=True),
            SearchField(name="payload", type=SearchFieldDataType.String, filterable=False, searchable=True, sortable=False, facetable=False),
            SearchField(name="embedding", type=SearchFieldDataType.Collection(SearchFieldDataType.Single),
                        searchable=True, vector_search_dimensions=1536, vector_search_profile_name="vector-config"),
        ],
        vector_search=vector_search,
    )

    index_client.create_index(index)

def __insert_code_documents(searchIndexClient: SearchIndexClient, prompts, language: str):
    search_client = searchIndexClient.get_search_client(os.environ["AISeachCodeIndexName"])
    client = AzureOpenAI(
        azure_endpoint = os.environ["OpenAIEndpoint"], 
        api_key=os.environ["OpenAIAPIKey"],  
        api_version=os.environ["OpenAIAPIVersion"]
    )

    
    for prompt in prompts:
        code_embedding = client.embeddings.create(input=prompt["prompt"], model=os.environ["OpenAIEmbeddingModelName"]).data[0].embedding
        document_id = str(uuid.uuid4())
        embedding = code_embedding

        payload = {
            "prompt": prompt['prompt'],
            "additionalDetails": prompt['prompt'],
            "response": prompt['response']
        }
        
        DOCUMENT = {
            "id": document_id,
            "tags": [f"language:{language}"],
            "payload": json.dumps(payload),
            "embedding": embedding,
        }

        result = search_client.upload_documents(documents=[DOCUMENT], )
        logging.info(f"Upload of new document with id {document_id} succeeded: {result[0].succeeded}")

