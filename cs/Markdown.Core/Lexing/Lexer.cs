namespace Markdown.Core.Lexing;

/// <summary>
/// Сканер:
/// - идёт по символам слева направо
/// - склеивает в один токен обычный текст
/// - выделяет специальные токены
/// - применяет базовые правила экранирования
/// </summary>
public class Lexer : ILexer
{
    public IEnumerable<Token> Tokenize(ReadOnlySpan<char> source)
    {
        throw new NotImplementedException();
    }
}