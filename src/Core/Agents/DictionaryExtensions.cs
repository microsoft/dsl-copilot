namespace DslCopilot.Core.Agents;

public static class DictionaryExtensions
{
    public static void Add<TKey, TValue>(
      this Dictionary<TKey, TValue> dict, KeyValuePair<TKey, TValue> item)
      where TKey : notnull => dict.Add(item.Key, item.Value);
}
