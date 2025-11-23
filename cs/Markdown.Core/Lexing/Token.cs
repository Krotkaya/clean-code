namespace Markdown.Core.Lexing;

/// <summary>
/// Одна часть (токен) входного текста
/// </summary>
public readonly struct Token(TokenKind kind, ReadOnlyMemory<char> slice, int position)
{
    public TokenKind Kind { get; init; } = kind;
    public ReadOnlyMemory<char> Slice { get; init; } = slice;
    public int Position { get; init; } = position;

    public override string ToString()
    {
        return $"{Kind} '{Slice}' at {Position}";
    }
}