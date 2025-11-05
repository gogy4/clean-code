using Markdown.Dto;

namespace Markdown.TokensUtils
{
    public static class TagRender
    {
        public static string Wrap(TokenType type, string content)
            => type switch
            {
                TokenType.Italic => $"<em>{content}</em>",
                TokenType.Strong => $"<strong>{content}</strong>",
                TokenType.Escape => $"<escaped>{content}</escaped>",
                TokenType.Header => $"<h1>{content}</h1>",
                _ => content
            };
    }
}