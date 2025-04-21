using System.ComponentModel;

namespace ModularNet.Domain.Enums;

public enum CacheType
{
    [Description("Memory")] InMemory = 1,
    [Description("Redis")] Redis = 2
}