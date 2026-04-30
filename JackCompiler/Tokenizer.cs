using System;
using System.Collections.Generic;
using System.Linq;

namespace JackCompiling
{
    public class Tokenizer
    {
        private readonly Queue<RawToken> _tokens;
        private readonly Stack<Token> _pushBackStack;

        public Tokenizer(string text)
        {
            _tokens = new Queue<RawToken>();
            _pushBackStack = new Stack<Token>();

            var currentToken = string.Empty;
            var currentTokenLine = 1;
            var currentTokenCol = 1;
            var line = 1;
            var col = 1;
            var i = 0;
            var symbols = new HashSet<char> { '{', '}', '(', ')', '[', ']', '.', ',', ';', '+', '-', '*', '/', '&', '|', '<', '>', '=', '~' };

            while (i < text.Length)
            {
                var c = text[i];

                if (char.IsWhiteSpace(c))
                {
                    AddCurrentToken();
                    MoveNext();
                }
                else if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
                {
                    // Однострочный комментарий пропускаем до '\n'. Сам '\n' обработается как пробел.
                    AddCurrentToken();
                    MoveNext();
                    MoveNext();
                    while (i < text.Length && text[i] != '\n')
                        MoveNext();
                }
                else if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
                {
                    // Многострочный комментарий пропускаем, но строки и колонки внутри него продолжаем считать.
                    AddCurrentToken();
                    MoveNext();
                    MoveNext();
                    while (i + 1 < text.Length && !(text[i] == '*' && text[i + 1] == '/'))
                        MoveNext();
                    if (i + 1 >= text.Length)
                        throw new FormatException("Unclosed comment");
                    MoveNext();
                    MoveNext();
                }
                else if (c == '"')
                {
                    AddCurrentToken();
                    // У строковой константы позиция считается от открывающей кавычки.
                    var stringToken = string.Empty;
                    var stringLine = line;
                    var stringCol = col;
                    stringToken += c;
                    MoveNext();
                    while (i < text.Length && text[i] != '"')
                    {
                        if (text[i] == '\n')
                            throw new FormatException("Unclosed string constant");
                        stringToken += text[i];
                        MoveNext();
                    }
                    if (i >= text.Length)
                        throw new FormatException("Unclosed string constant");
                    stringToken += text[i];
                    MoveNext();
                    _tokens.Enqueue(new RawToken(stringToken, stringLine, stringCol));
                }
                else if (symbols.Contains(c))
                {
                    AddCurrentToken();
                    _tokens.Enqueue(new RawToken(c.ToString(), line, col));
                    MoveNext();
                }
                else
                {
                    StartCurrentToken();
                    currentToken += c;
                    MoveNext();
                }
            }

            AddCurrentToken();

            void StartCurrentToken()
            {
                if (string.IsNullOrEmpty(currentToken))
                {
                    currentTokenLine = line;
                    currentTokenCol = col;
                }
            }

            void AddCurrentToken()
            {
                if (!string.IsNullOrEmpty(currentToken))
                {
                    _tokens.Enqueue(new RawToken(currentToken, currentTokenLine, currentTokenCol));
                    currentToken = string.Empty;
                }
            }

            void MoveNext()
            {
                if (text[i] == '\n')
                {
                    line++;
                    col = 1;
                }
                else
                {
                    col++;
                }

                i++;
            }
        }

        /// <summary>
        /// Сначала возвращает токены, которые вернули методом PushBack.
        /// Потом читает и возвращает следующий токен, либо null, если токенов больше нет.
        /// </summary>
        public Token? TryReadNext()
        {
            if (_pushBackStack.Count > 0)
            {
                return _pushBackStack.Pop();
            }

            if (_tokens.Count == 0)
            {
                return null;
            }

            var nextToken = _tokens.Dequeue();
            var tokenType = GetTokenType(nextToken.Text);
            var value = tokenType == TokenType.StringConstant
                ? nextToken.Text[1..^1]
                : nextToken.Text;
            return new Token(tokenType, value, nextToken.LineNumber, nextToken.ColNumber);
        }

        /// <summary>
        /// Возвращает ранее прочитанный токен обратно в токенайзер.
        /// Если token - null, то игнорирует его.
        /// </summary>
        public void PushBack(Token? token)
        {
            if (token != null)
            {
                _pushBackStack.Push(token);
            }
        }

        private TokenType GetTokenType(string token)
        {
            var keywords = new HashSet<string>
            {
                "class", "constructor", "function", "method", "field", "static", "var", "int", "char", "boolean",
                "void", "true", "false", "null", "this", "let", "do", "if", "else", "while", "return"
            };
            if (keywords.Contains(token))
            {
                return TokenType.Keyword;
            }

            var symbols = new HashSet<char> { '{', '}', '(', ')', '[', ']', '.', ',', ';', '+', '-', '*', '/', '&', '|', '<', '>', '=', '~' };
            if (token.Length == 1 && symbols.Contains(token[0]))
            {
                return TokenType.Symbol;
            }

            if (int.TryParse(token, out var intValue))
            {
                if (intValue >= 0 && intValue <= 32767)
                    return TokenType.IntegerConstant;
                throw new FormatException($"Invalid token: {token}");
            }

            if (token.StartsWith("\"") && token.EndsWith("\"") && !token[1..^1].Contains('"') && !token.Contains("\n"))
            {
                return TokenType.StringConstant;
            }

            if ((char.IsLetter(token[0]) || token[0] == '_') && token.All(c => char.IsLetterOrDigit(c) || c == '_'))
            {
                return TokenType.Identifier;
            }

            throw new FormatException($"Invalid token: {token}");
        }

        private record RawToken(string Text, int LineNumber, int ColNumber);
    }
}
