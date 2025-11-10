using System.Text;
using Markdown.Dto;

namespace Markdown.TagUtils.Abstractions;

public interface ITagManager
{
    public void EndProcess();
    public void HeaderProcess(Token token);
    public void ItalicProcess(Token token);
    public void StrongProcess(Token token);
    public void LinkProcess(Token token);
}