using System.Text;
using Markdown.Dto;

namespace Markdown.TagUtils.Abstractions;

public interface ITagManager
{
    public void EndProcess(Stack<Tag> tagsStack, StringBuilder result);
    public void HeaderProcess(Token token, Stack<Tag> tagsStack);
    public void ItalicProcess(Token token, Stack<Tag> tagsStack, StringBuilder result);
    public void StrongProcess(Token token, Stack<Tag> tagsStack, StringBuilder result);

}