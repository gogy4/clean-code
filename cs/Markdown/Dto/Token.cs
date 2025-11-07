namespace Markdown.Dto;

public class Token
{
    public string Value { get; private set; }
    public TokenType Type { get; private set; }
    public TokenRole Role { get; private set; }
    public TokenPosition Position { get; private set; }

    public Token(string value, TokenType type, TokenRole role, TokenPosition position)
    {
        Value = value;
        Type = type;
        Role = role;
        Position = position;
    }

    public Token(string value, TokenType type, bool canOpen, bool canClose)
    {
        Value = value;
        Type = type;
        DetermineRole(canOpen, canClose);
        DeterminePosition(canOpen, canClose);
    }
    
    
    private void DetermineRole(bool canOpen, bool canClose)
    {
        Role = (canOpen, canClose) switch
        {
            (true, true) => TokenRole.Both,
            (true, false) => TokenRole.Open,
            (false, true) => TokenRole.Close,
            (false, false) => TokenRole.None
        };
    }

    private void DeterminePosition(bool canOpen, bool canClose)
    {
        Position = (canOpen, canClose) switch
        {
            (true, true) => TokenPosition.Inside,
            (true, false) => TokenPosition.Begin,
            (false, true) => TokenPosition.End,
            (false, false) => TokenPosition.None
        };
    }
    
}