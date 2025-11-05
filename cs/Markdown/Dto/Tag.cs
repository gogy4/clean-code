using System.Text;

namespace Markdown.Dto;

public record Tag(Token Token, StringBuilder Content, bool isOpen);