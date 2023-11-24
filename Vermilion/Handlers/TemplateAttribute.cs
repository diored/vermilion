namespace DioRed.Vermilion.Handlers;

[AttributeUsage(AttributeTargets.Parameter)]
public class TemplateAttribute(string pattern) : Attribute
{
    public string Pattern { get; } = pattern;
}