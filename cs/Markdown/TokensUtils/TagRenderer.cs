namespace Markdown.TokensUtils
{
    public static class TagRenderer
    {
        public static string Wrap(TokenType type, string content)
            => type switch
            {
                TokenType.Italic      => $"<em>{content}</em>",
                _ => content
            };
    }
}