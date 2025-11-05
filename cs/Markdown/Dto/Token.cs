namespace Markdown.Dto;

public record Token(string Value, TokenType Type, bool CanOpen, bool CanClose, bool InsideWord);