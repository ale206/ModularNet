using System.ComponentModel;
using System.Globalization;

namespace ModularNet.Shared.Helpers;

public static class EnumHelper
{
    public static string GetDescription<T>(this T e) where T : IConvertible
    {
        if (e is Enum)
        {
            var type = e.GetType();
            var values = Enum.GetValues(type);

            foreach (int val in values)
                if (val == e.ToInt32(CultureInfo.InvariantCulture))
                {
                    var memInfo = type.GetMember(type.GetEnumName(val) ?? throw new InvalidOperationException());
                    var descriptionAttribute = memInfo[0]
                        .GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .FirstOrDefault() as DescriptionAttribute;

                    if (descriptionAttribute != null) return descriptionAttribute.Description;
                }
        }

        return string.Empty;
    }

    private static T? GetValueFromDescription<T>(string description) where T : Enum
    {
        foreach (var field in typeof(T).GetFields())
            if (Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                if (attribute.Description == description)
                    return (T?)field.GetValue(null);
            }
            else
            {
                if (field.Name == description)
                    return (T?)field.GetValue(null);
            }

        throw new ArgumentException("Not found.", nameof(description));
        // Or return default(T);
    }
}