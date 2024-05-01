from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient
from azure.search.documents.indexes import SearchIndexClient

class SearchClientFactory():

    def __init__(self, endpoint, key):
        self.endpoint = endpoint
        self.key = key
        self.credentials = AzureKeyCredential(key)

    def create_search_client(self, index_name: str):
        return SearchClient(
            endpoint=self.endpoint,
            index_name=self.index_name,
            credential=self.credentials,
        )

    def create_search_index_client(self):
        return SearchIndexClient(endpoint=self.endpoint, credential=self.credentials)