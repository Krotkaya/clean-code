namespace Markdown.Core.Parsing.Nodes;

/// <summary>
/// Обычный текст без разметки, конечный лист дерева
/// </summary>
public class TextNode : InlineNode
{
    public string Text { get; }

    public TextNode(string text)
    {
        Text = text;
    }
}