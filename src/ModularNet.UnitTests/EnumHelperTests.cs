using System.ComponentModel;
using ModularNet.Domain.Enums;
using ModularNet.Shared.Helpers;

namespace ModularNet.UnitTests;

public class EnumHelperTests
{
    private enum TestEnumWithoutDescription
    {
        FirstValue = 1,
        SecondValue = 2
    }

    [Fact]
    public void GetDescriptionReturnsDescriptionWhenAttributeExists()
    {
        // Arrange
        var auditType = AuditType.Login;

        // Act
        var description = auditType.GetDescription();

        // Assert
        Assert.Equal("Login", description);
    }

    [Fact]
    public void GetDescriptionReturnsEmptyStringWhenAttributeDoesNotExist()
    {
        // Arrange
        var testEnum = TestEnumWithoutDescription.FirstValue;

        // Act
        var description = testEnum.GetDescription();

        // Assert
        Assert.Equal(string.Empty, description);
    }

    [Fact]
    public void GetDescriptionReturnsEmptyStringWhenNotAnEnum()
    {
        // Arrange
        const int nonEnumValue = 42;

        // Act
        var description = nonEnumValue.GetDescription();

        // Assert
        Assert.Equal(string.Empty, description);
    }

    [Fact]
    public void GetValueFromDescriptionReturnsEnumWhenDescriptionMatches()
    {
        // Arrange
        const string description = "Memory";

        // Act
        var cacheType = EnumHelper.GetValueFromDescription<CacheType>(description);

        // Assert
        Assert.Equal(CacheType.InMemory, cacheType);
    }

    [Fact]
    public void GetValueFromDescriptionReturnsEnumWhenNameMatches()
    {
        // Arrange
        const string fieldName = "SecondValue";

        // Act
        var testEnum = EnumHelper.GetValueFromDescription<TestEnumWithoutDescription>(fieldName);

        // Assert
        Assert.Equal(TestEnumWithoutDescription.SecondValue, testEnum);
    }

    [Fact]
    public void GetValueFromDescriptionThrowsArgumentExceptionWhenNotFound()
    {
        // Arrange
        const string unknownDescription = "NonExistentDescription";

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            EnumHelper.GetValueFromDescription<CacheType>(unknownDescription));
    }
}
