using System.Text;
using Markdown.Dto;

namespace Markdown.TagUtils.Abstractions;

public interface ITagContentManager
{
    public void AppendToParentOrResult(Stack<Tag> tagsStack, StringBuilder result, string content);
    public void CloseTopTag(Stack<Tag> tagsStack, StringBuilder result, bool isFinal = false);
}