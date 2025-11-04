using System.Text;
using Markdown.TokensUtils.Abstractions;

namespace Markdown.TokensUtils;

public class Tokenizer : ITokenizer
{
    public IEnumerable<Token> Tokenize(string? line)
    {
        ArgumentNullException.ThrowIfNull(line);

        var tokenizeLineStringBuilder = new StringBuilder();
        var i = 0;
        while (i < line.Length)
        {
            var currentChar = line[i];
            
            if (IsSpecialSymbol(currentChar) && tokenizeLineStringBuilder.Length > 0)
            {
                yield return new Token(tokenizeLineStringBuilder.ToString(), TokenType.Text);
                tokenizeLineStringBuilder.Clear();
            }
            
            switch (currentChar)
            {
                case '\\':
                    yield return new Token("\\", TokenType.Escape);
                    break;
                case '_' when i + 1 < line.Length && line[i + 1] == '_':
                    yield return new Token("__", TokenType.Strong);
                    i++;
                    break;
                case '_':
                    yield return new Token("_", TokenType.Italic);
                    break;
                case '#' when i + 1 < line.Length && line[i + 1] == ' ':
                    yield return new Token("#", TokenType.Header);
                    i++;
                    break;
                default:
                    tokenizeLineStringBuilder.Append(currentChar);
                    break;
            }
            i++;
        }

        if (tokenizeLineStringBuilder.Length > 0)
            yield return new Token(tokenizeLineStringBuilder.ToString(), TokenType.Text);

        yield return new Token(string.Empty, TokenType.End);
    }

    private bool IsSpecialSymbol(char c)
    {
        return c switch
        {
            '\\' or '_' or '#' => true,
            _ => false
        };
    }
}