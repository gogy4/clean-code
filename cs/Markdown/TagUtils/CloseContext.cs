using Markdown.Dto;

namespace Markdown.TagUtils;

public static class CloseContext
{
    public static bool IsInvalidCloseContext(Tag parent, Token token, int markerLength)
    {
        if (parent == null)
            return true;

        if (parent.Token.Type != token.Type)
            return true;

        if (parent.Token.Position == TokenPosition.Inside && parent.ContainsSpace)
            return true;

        var contentLength = parent.Content.Length - markerLength;
        return contentLength <= 0 || parent.HasOnlyDigits;
    }
}