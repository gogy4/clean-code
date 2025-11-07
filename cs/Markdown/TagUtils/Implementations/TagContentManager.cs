using System.Text;
using Markdown.Dto;
using Markdown.TagUtils.Abstractions;
using Markdown.TokensUtils;

namespace Markdown.TagUtils.Implementations;

public class TagContentManager : ITagContentManager
{
    public void AppendToParentOrResult(Stack<Tag> tagsStack, StringBuilder result, string content)
    {
        if (tagsStack.Count > 0)
        {
            tagsStack.Peek().Append(content);
        }
        else
        {
            result.Append(content);
        }
    }
        
    public void CloseTopTag(Stack<Tag> tagsStack, StringBuilder result)
    {
        var top = tagsStack.Pop();
        var content = top.Content.ToString();

        var markerLength = top.Token.Value.Length;
        var innerContent = content.Length > markerLength
            ? content[markerLength..]
            : string.Empty;

        var wrapped = TagRender.Wrap(top.Token.Type, innerContent);
        AppendToParentOrResult(tagsStack, result, wrapped);
    }
}