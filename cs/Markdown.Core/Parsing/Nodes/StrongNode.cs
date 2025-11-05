namespace Markdown.Core.Parsing.Nodes;

/// <summary>
/// Полужирный (__...__): контейнер инлайнов внутри выделения
/// </summary>
public class StrongNode : InlineNode
{
    public IList<InlineNode> Inlines { get; }

    public StrongNode()
    {
        Inlines = new List<InlineNode>();
    }
}