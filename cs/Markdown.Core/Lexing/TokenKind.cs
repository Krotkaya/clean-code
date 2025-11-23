namespace Markdown.Core.Lexing;

public enum TokenKind
{
    Text, // Обычный текст
    Underscore, // Курсивный шрифт
    DoubleUnderscore, // Полужирный шрифт
    Hash, //Заголовок
    Space, //Одиночный пробел
    NewLine, //Перевод строки
    Eof, // Конец входа
    LeftBracket, // Квадратная скобка '[' открывает текст ссылки
    RightBracket, // Квадратная скобка ']' закрывает текст ссылки
    LeftParen, // Круглая скобка '(' открывает адрес ссылки
    RightParen, // Круглая скобка ')' закрывает адрес ссылки
}