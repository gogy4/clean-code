namespace Markdown.TokensUtils;

public static class TagRenderer
{
    public static string WrapEm(string content) => $"<em>{content}</em>";
    //остальные методы по аналогии для каждого тега
}