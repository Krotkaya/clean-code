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
    public IEnumerable<Token> Tokenize(ReadOnlyMemory<char> source)
    {
        var tokensSpan = source.Span;
        var tokens = new List<Token>();
        var i = 0;
        var length = source.Length;

        while (i < length)
        {
            var symbol = tokensSpan[i];

            switch (symbol)
            {
                case '#' when IsAtLineStart(tokens) && i + 1 < length && tokensSpan[i + 1] == ' ':
                    tokens.Add(new Token(TokenKind.Hash, source.Slice(i, 1), i));
                    i += 1;
                    continue;
                
                case '#':
                    tokens.Add(new Token(TokenKind.Text, source.Slice(i, 1), i));
                    i += 1;
                    continue;
                
                case '\\' when i + 1 < length:
                    var nextToken = tokensSpan[i + 1];
                    if (nextToken == '_' && i + 2 < length && tokensSpan[i + 2] == '_')
                    {
                        tokens.Add(new Token(TokenKind.Text, source.Slice(i + 1, 2), i));
                        i += 3;
                        continue;
                    }

                    if (IsSpecialCharacter(nextToken))
                    {
                        tokens.Add(new Token(TokenKind.Text, source.Slice(i + 1, 1), i));
                        i += 2;
                        continue;
                    }

                    tokens.Add(new Token(TokenKind.Text, source.Slice(i, 1), i));
                    i += 1;
                    continue;
                
                case '\\':
                    tokens.Add(new Token(TokenKind.Text, source.Slice(i, 1), i));
                    i += 1;
                    continue;
                
                case '_' when i + 1 < length && tokensSpan[i + 1] == '_':
                    tokens.Add(new Token(TokenKind.DoubleUnderscore, source.Slice(i, 2), i));
                    i += 2;
                    continue;
                
                case '_' when i + 1 <= length:
                    tokens.Add(new Token(TokenKind.Underscore, source.Slice(i, 1), i));
                    i += 1;
                    continue;
                
                case ' ':
                    tokens.Add(new Token(TokenKind.Space, source.Slice(i, 1), i));
                    i += 1;
                    continue;
                
                case '\n':
                    tokens.Add(new Token(TokenKind.NewLine, source.Slice(i, 1), i));
                    i += 1;
                    continue;
                
                case '\r' when i + 1 < length && tokensSpan[i + 1] == '\n':
                    tokens.Add(new Token(TokenKind.NewLine, source.Slice(i, 2), i));
                    i += 2;
                    continue;
                
                case '[':
                    tokens.Add(new Token(TokenKind.LeftBracket, source.Slice(i, 1), i));
                    i += 1;
                    continue;
                
                case ']':
                    tokens.Add(new Token(TokenKind.RightBracket, source.Slice(i, 1), i));
                    i += 1;
                    continue;
                
                case '(':
                    tokens.Add(new Token(TokenKind.LeftParen, source.Slice(i, 1), i));
                    i += 1;
                    continue;
                
                case ')':
                    tokens.Add(new Token(TokenKind.RightParen, source.Slice(i, 1), i));
                    i += 1;
                    continue;
            }
            
            var startText = i;
            while (i < length)
            {
                symbol = tokensSpan[i];
                if (IsSpecialCharacter(symbol) || symbol is ' ' or '\n' or '\r')
                    break;
                i++;
            }

            if (i > startText)
            {
                tokens.Add(new Token(TokenKind.Text, source.Slice(startText, i - startText), startText));
            }
        }
        tokens.Add(new Token(TokenKind.Eof, source[..0], length));
        return tokens;
    }
    
    private static bool IsSpecialCharacter(char c) =>
        c is '#' or '_' or '\\' or '[' or ']' or '(' or ')';

    private static bool IsAtLineStart(List<Token> tokens) =>
        tokens.Count == 0 || tokens[^1].Kind == TokenKind.NewLine;
}
