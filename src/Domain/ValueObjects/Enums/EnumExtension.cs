using System.ComponentModel;
using System.Reflection;

namespace Domain.ValueObjects.Enums;

internal static class EnumExtension
{
    #pragma warning disable CS8600
    public static string GetDescription(this Enum value)
    {
        Type type = value.GetType();
 // Converting null literal or possible null value to non-nullable type.
        string name = Enum.GetName(type, value);

        if (name != null)
        {
            FieldInfo field = type.GetField(name);
            if (field != null)
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                {
                    return attr.Description;
                }
            }
        }

        return value.ToString();
    }
}
