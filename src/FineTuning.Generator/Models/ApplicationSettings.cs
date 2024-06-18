namespace DSL.FineTuning.Pipeline.Models
{
    internal record ApplicationSettings(
        GlobalSettings GlobalSettings,
        OpenAI OpenAI,
        AzureAISearch AzureAISearch,
        LanguageStorage LanguageStorage
    );

    internal record GlobalSettings(
        string PromptFileLocation,
        string LanguageName,
        string TrainingDataLocation
    );

    internal record OpenAI(
        string Endpoint,
        string ModelName,
        string EmbeddingModelName,
        string Key
    );

    internal record AzureAISearch(
        string ResourceName,
        string Endpoint,
        string Key,
        string IndexName
    );

    internal record LanguageStorage(
        string AccountName,
        string AccessKey
    );
}
