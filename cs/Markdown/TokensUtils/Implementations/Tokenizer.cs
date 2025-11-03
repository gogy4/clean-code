using Markdown.TokensUtils.Abstractions;

namespace Markdown.TokensUtils;

public class Tokenizer : ITokenizer
{
    public IEnumerable<Token> Tokenize(string line)
    {
        // Проходит по строке посимвольно, обрабатывает экранирование и спецсимволы
        throw new NotImplementedException();
    }
}