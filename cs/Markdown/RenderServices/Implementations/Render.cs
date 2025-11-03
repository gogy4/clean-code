namespace Markdown;

public class Render : IRender
{
    private readonly IRenderFragment renderFragment = new RenderFragment();

    public string RenderText(string markdown)
    {
        //сделаем коллекцию строк из большого текста
        //будем бежать по каждой строке и применять рендер к ней
        //после либо создадим новую коллекцию, в которой будут измененные строки либо будем изменять текущую(я за
        //то, чтобы изменять текущую)
        //создадим строку через стрингбилдер из коллекции и вернем ее.
        throw new NotImplementedException();
    }
}