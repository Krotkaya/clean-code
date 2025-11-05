using Markdown.Core.Lexing;
using Markdown.Core.Parsing.Nodes;
namespace Markdown.Core.Parsing;

/// <summary>
/// Парсер:
/// - делит документ на блоки (заголовки и абзацы)
/// - внутри блоков собирает инлайны (Текст/Курсив/Жирный)
/// - следует спецификации (границы, цифры, пересечения, пустые выделения)
/// </summary>
public class Parser : IParser
{
    public DocumentNode Parse(IEnumerable<Token> tokens)
    {
        throw new NotImplementedException();
    }
}