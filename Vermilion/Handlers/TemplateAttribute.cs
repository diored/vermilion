namespace DioRed.Vermilion.Handlers;

[AttributeUsage(AttributeTargets.Parameter)]
public class TemplateAttribute : Attribute
{
    public TemplateAttribute(string pattern)
    {
        Pattern = pattern;
    }

    public string Pattern { get; }
}