using System.Text;

namespace Markdown.Dto;

public class Tag
{
    public TokenType Type { get; set; }
    public string Marker { get; set; }
    public StringBuilder Content { get; set; }

    public Tag(TokenType type, string marker, StringBuilder content)
    {
        Type = type;
        Marker = marker;
        Content = content;
    }
}