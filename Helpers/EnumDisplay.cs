using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ArticleApi.Helpers;

public static class EnumDisplay
{
    public static string GetDisplayName(Enum display) 
    {
        var field = display.GetType().GetField(display.ToString());
        var attr = field?.GetCustomAttribute<DisplayAttribute>();
        return attr?.Name ?? display.ToString();
    }
}