namespace Markdown.TokensUtils;

using Markdown.Dto;

public static class MapSpecialSymbol
{
    public static Token? Specialize(char c, string line, int index)
    {
        return c switch
        {
            '['  =>
                CreateLinkToken(line, index, true),
            ')' => 
                CreateLinkToken(line, index, false),
            '_' when index + 1 < line.Length && line[index + 1] == '_' => 
                CreateUnderscoreToken(line, index, true),
            '_' => 
                CreateUnderscoreToken(line, index, false),
            '#' when index + 1 < line.Length && line[index + 1] == ' ' => 
                CreateHeaderToken(line, index),
            '\\' when index + 1 < line.Length => 
                new Token(line[index].ToString(), TokenType.Escape, TokenRole.None, TokenPosition.None),
            _ => null
        };
    }

    private static Token CreateUnderscoreToken(string line, int index, bool isStrong)
    {
        var value = isStrong ? "__" : "_";
        var type = isStrong ? TokenType.Strong : TokenType.Italic;
        var prevChar = index > 0 ? line[index - 1] : (char?)null;
        var nextChar = index + value.Length < line.Length ? line[index + value.Length] : (char?)null;
        var canOpen = nextChar != null && !char.IsWhiteSpace(nextChar.Value);
        var canClose = prevChar != null && !char.IsWhiteSpace(prevChar.Value);
        
        return new Token(value, type, canOpen, canClose);
    }

    private static Token CreateLinkToken(string line, int index, bool isOpen)
    {
        var value = isOpen ? "[" : ")";
        var prevChar = index > 0 ? line[index - 1] : (char?)null;
        var nextChar = index + value.Length < line.Length ? line[index + value.Length] : (char?)null;
        var canOpen = nextChar != null && !char.IsWhiteSpace(nextChar.Value);
        var canClose = prevChar != null && !char.IsWhiteSpace(prevChar.Value);
        return new Token(value, TokenType.Link, isOpen && canOpen, !isOpen && canClose);
    }

    private static Token CreateHeaderToken(string line, int index)
    {
        const string value = "#";
        var nextChar = index + value.Length < line.Length ? line[index + value.Length] : (char?)null;
        var canOpenAndClose = nextChar != null && char.IsWhiteSpace(nextChar.Value);
        return new Token(value, TokenType.Header, canOpenAndClose, canOpenAndClose);
    }
}