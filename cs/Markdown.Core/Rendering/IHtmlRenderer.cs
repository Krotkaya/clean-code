using Markdown.Core.Parsing.Nodes;

namespace Markdown.Core.Rendering;

public interface IHtmlRenderer
{
    string Render (DocumentNode document);
}