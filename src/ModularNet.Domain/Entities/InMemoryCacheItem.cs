namespace ModularNet.Domain.Entities;

public class InMemoryCacheItem
{
    public DateTime ExpirationDate { get; set; }
    public string ItemKey { get; set; } = string.Empty;
    public dynamic? ItemValue { get; set; }
}