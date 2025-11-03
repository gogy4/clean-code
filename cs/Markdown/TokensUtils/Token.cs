namespace Markdown.TokensUtils;

public class Token
{
    public string Value { get; set; }
    public string TokenType { get; set; }

    public Token(string value, string tokenType)
    {
        Value = value;
        TokenType = tokenType;
    }
}