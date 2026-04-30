using System.Collections.Generic;

namespace JackCompiling
{
    public class Parser
    {
        private readonly Tokenizer tokenizer;

        public Parser(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        public ClassSyntax ReadClass()
        {
            // class className { classVarDec* subroutineDec* }
            var classToken = tokenizer.Read("class");
            var name = tokenizer.Read(TokenType.Identifier);
            var open = tokenizer.Read("{");
            var classVars = tokenizer.ReadList(TryReadClassVarDec);
            var subroutineDec = tokenizer.ReadList(TryReadSubroutineDec);
            var close = tokenizer.Read("}");

            return new ClassSyntax(classToken, name, open, classVars, subroutineDec, close);
        }

        public StatementsSyntax ReadStatements()
        {
            // statements - это список команд. Заканчивается он перед закрывающей }.
            var statements = new List<StatementSyntax>();

            while (true)
            {
                var token = tokenizer.TryReadNext();
                if (token is null)
                    return new StatementsSyntax(statements);

                if (token.Value == "}")
                {
                    // Закрывающая скобка принадлежит внешнему блоку, поэтому возвращаем ее обратно.
                    tokenizer.PushBack(token);
                    return new StatementsSyntax(statements);
                }

                statements.Add(ReadStatement(token));
            }
        }

        public SubroutineCall ReadSubroutineCall()
        {
            // Вызов всегда начинается с имени: f() или ClassName.f().
            var name = tokenizer.Read(TokenType.Identifier);
            return ReadSubroutineCall(name);
        }

        public ParameterListSyntax ReadParameterList()
        {
            // parameterList: (type varName) (',' type varName)* или пустой список перед ')'.
            var parameters = tokenizer.ReadDelimitedList(ReadParameter, ",", ")");
            return new ParameterListSyntax(parameters);
        }

        public ExpressionSyntax ReadExpression()
        {
            // expression: term (op term)*
            var term = ReadTerm();
            var tail = new List<ExpressionTail>();

            while (true)
            {
                var token = tokenizer.TryReadNext();
                if (!token.IsOneOf("+", "-", "*", "/", "&", "|", "<", ">", "="))
                {
                    tokenizer.PushBack(token);
                    return new ExpressionSyntax(term, tail);
                }

                var nextTerm = ReadTerm();
                tail.Add(new ExpressionTail(token!, nextTerm));
            }
        }

        public TermSyntax ReadTerm()
        {
            // term - это минимальная часть выражения: число, строка, переменная, вызов, скобки или унарная операция.
            var token = tokenizer.Read();

            if (token.TokenType is TokenType.IntegerConstant or TokenType.StringConstant)
                return new ValueTermSyntax(token, null);

            if (token.IsOneOf("true", "false", "null", "this"))
                return new ValueTermSyntax(token, null);

            if (token.IsOneOf("-", "~"))
                return new UnaryOpTermSyntax(token, ReadTerm());

            if (token.Value == "(")
            {
                var expression = ReadExpression();
                var close = tokenizer.Read(")");
                return new ParenthesizedTermSyntax(token, expression, close);
            }

            if (token.TokenType == TokenType.Identifier)
                return ReadIdentifierTerm(token);

            throw new ExpectedException("term", token);
        }

        private ClassVarDecSyntax? TryReadClassVarDec(Token kindKeyword)
        {
            // classVarDec начинается только с field или static.
            if (!kindKeyword.IsOneOf("field", "static"))
                return null;

            var type = ReadType();
            var names = ReadNamesList();
            var semicolon = tokenizer.Read(";");

            return new ClassVarDecSyntax(kindKeyword, type, names, semicolon);
        }

        private SubroutineDecSyntax? TryReadSubroutineDec(Token kindKeyword)
        {
            // subroutineDec начинается только с constructor, function или method.
            if (!kindKeyword.IsOneOf("constructor", "function", "method"))
                return null;

            var returnType = ReadReturnType();
            var name = tokenizer.Read(TokenType.Identifier);
            var openArgs = tokenizer.Read("(");
            var parameterList = ReadParameterList();
            var closeArgs = tokenizer.Read(")");
            var subroutineBody = ReadSubroutineBody();

            return new SubroutineDecSyntax(kindKeyword, returnType, name, openArgs, parameterList, closeArgs,
                subroutineBody);
        }

        private SubroutineBodySyntax ReadSubroutineBody()
        {
            // subroutineBody: { varDec* statements }
            var open = tokenizer.Read("{");
            var varDec = tokenizer.ReadList(TryReadVarDec);
            var statements = ReadStatements();
            var close = tokenizer.Read("}");

            return new SubroutineBodySyntax(open, varDec, statements, close);
        }

        private VarDecSyntax? TryReadVarDec(Token kindKeyword)
        {
            // varDec начинается только с var.
            if (!kindKeyword.Is(TokenType.Keyword, "var"))
                return null;

            var type = ReadType();
            var names = ReadNamesList();
            var semicolon = tokenizer.Read(";");

            return new VarDecSyntax(kindKeyword, type, names, semicolon);
        }

        private StatementSyntax ReadStatement(Token firstToken)
        {
            // По первому слову выбираем конкретный вид statement.
            return firstToken.Value switch
            {
                "let" => ReadLetStatement(firstToken),
                "do" => ReadDoStatement(firstToken),
                "return" => ReadReturnStatement(firstToken),
                "if" => ReadIfStatement(firstToken),
                "while" => ReadWhileStatement(firstToken),
                _ when firstToken.TokenType == TokenType.Identifier => ReadStatementWithoutKeyword(firstToken),
                _ => throw new ExpectedException("statement", firstToken)
            };
        }

        private LetStatementSyntax ReadLetStatement(Token letToken)
        {
            // let varName ('[' expression ']')? = expression ;
            var varName = tokenizer.Read(TokenType.Identifier);
            return ReadLetStatement(letToken, varName);
        }

        private LetStatementSyntax ReadLetStatement(Token letToken, Token varName)
        {
            // Сюда попадают оба варианта: обычный "let x = ..." и короткий "x = ...".
            var index = TryReadIndexing();
            var eq = tokenizer.Read("=");
            var value = ReadExpression();
            var semicolon = tokenizer.Read(";");

            return new LetStatementSyntax(letToken, varName, index, eq, value, semicolon);
        }

        private DoStatementSyntax ReadDoStatement(Token doToken)
        {
            // do subroutineCall ;
            var subroutineCall = ReadSubroutineCall();
            return ReadDoStatement(doToken, subroutineCall);
        }

        private DoStatementSyntax ReadDoStatement(Token doToken, SubroutineCall subroutineCall)
        {
            // Сюда попадают оба варианта: обычный "do f();" и короткий "f();".
            var semicolon = tokenizer.Read(";");

            return new DoStatementSyntax(doToken, subroutineCall, semicolon);
        }

        private StatementSyntax ReadStatementWithoutKeyword(Token firstToken)
        {
            // Если команда начинается с имени, то ключевые слова let/do пропущены.
            // По следующему токену понимаем, что это: присваивание или вызов подпрограммы.
            var nextToken = tokenizer.TryReadNext();

            if (nextToken.IsOneOf("=", "["))
            {
                tokenizer.PushBack(nextToken);
                return ReadLetStatement(CreateMissingKeyword("let", firstToken), firstToken);
            }

            if (nextToken.IsOneOf("(", "."))
            {
                tokenizer.PushBack(nextToken);
                var subroutineCall = ReadSubroutineCall(firstToken);
                return ReadDoStatement(CreateMissingKeyword("do", firstToken), subroutineCall);
            }

            tokenizer.PushBack(nextToken);
            throw new ExpectedException("statement", firstToken);
        }

        private ReturnStatementSyntax ReadReturnStatement(Token returnToken)
        {
            // return может быть пустым: return; или с выражением: return expression;
            var token = tokenizer.Read();
            if (token.Value == ";")
                return new ReturnStatementSyntax(returnToken, null, token);

            tokenizer.PushBack(token);
            var returnValue = ReadExpression();
            var semicolon = tokenizer.Read(";");

            return new ReturnStatementSyntax(returnToken, returnValue, semicolon);
        }

        private IfStatementSyntax ReadIfStatement(Token ifToken)
        {
            // if (expression) { statements } (else { statements })?
            var open = tokenizer.Read("(");
            var condition = ReadExpression();
            var close = tokenizer.Read(")");
            var openTrue = tokenizer.Read("{");
            var trueStatements = ReadStatements();
            var closeTrue = tokenizer.Read("}");
            var elseClause = TryReadElseClause();

            return new IfStatementSyntax(ifToken, open, condition, close, openTrue, trueStatements, closeTrue,
                elseClause);
        }

        private ElseClause? TryReadElseClause()
        {
            var token = tokenizer.TryReadNext();
            if (!token.Is(TokenType.Keyword, "else"))
            {
                tokenizer.PushBack(token);
                return null;
            }

            var open = tokenizer.Read("{");
            var falseStatements = ReadStatements();
            var close = tokenizer.Read("}");

            return new ElseClause(token!, open, falseStatements, close);
        }

        private WhileStatementSyntax ReadWhileStatement(Token whileToken)
        {
            // while (expression) { statements }
            var open = tokenizer.Read("(");
            var condition = ReadExpression();
            var close = tokenizer.Read(")");
            var openStatements = tokenizer.Read("{");
            var statements = ReadStatements();
            var closeStatements = tokenizer.Read("}");

            return new WhileStatementSyntax(whileToken, open, condition, close, openStatements, statements,
                closeStatements);
        }

        private Parameter ReadParameter()
        {
            // Один параметр состоит из типа и имени: int x.
            var type = ReadType();
            var name = tokenizer.Read(TokenType.Identifier);

            return new Parameter(type, name);
        }

        private ExpressionListSyntax ReadExpressionList()
        {
            // expressionList: expression (',' expression)* или пустой список перед ')'.
            var expressions = tokenizer.ReadDelimitedList(ReadExpression, ",", ")");
            return new ExpressionListSyntax(expressions);
        }

        private TermSyntax ReadIdentifierTerm(Token name)
        {
            // После имени может быть обычная переменная, обращение к массиву или вызов функции.
            var token = tokenizer.TryReadNext();

            if (token?.Value == "[")
            {
                var expression = ReadExpression();
                var close = tokenizer.Read("]");
                return new ValueTermSyntax(name, new Indexing(token, expression, close));
            }

            if (token.IsOneOf("(", "."))
            {
                tokenizer.PushBack(token);
                return new SubroutineCallTermSyntax(ReadSubroutineCall(name));
            }

            tokenizer.PushBack(token);
            return new ValueTermSyntax(name, null);
        }

        private SubroutineCall ReadSubroutineCall(Token name)
        {
            // subroutineCall: subroutineName '(' expressionList ')' или objectOrClass '.' subroutineName '(' expressionList ')'
            MethodObjectOrClass? objectOrClass = null;
            var subroutineName = name;
            var open = tokenizer.Read();

            if (open.Value == ".")
            {
                objectOrClass = new MethodObjectOrClass(name, open);
                subroutineName = tokenizer.Read(TokenType.Identifier);
                open = tokenizer.Read("(");
            }
            else if (open.Value != "(")
            {
                throw new ExpectedException("( or .", open);
            }

            var arguments = ReadExpressionList();
            var close = tokenizer.Read(")");

            return new SubroutineCall(objectOrClass, subroutineName, open, arguments, close);
        }

        private Indexing? TryReadIndexing()
        {
            var token = tokenizer.TryReadNext();
            if (token?.Value != "[")
            {
                tokenizer.PushBack(token);
                return null;
            }

            var index = ReadExpression();
            var close = tokenizer.Read("]");

            return new Indexing(token, index, close);
        }

        private List<Token> ReadNamesList()
        {
            // Имена в объявлениях идут через запятую и заканчиваются перед ';'.
            return tokenizer.ReadDelimitedList(() => tokenizer.Read(TokenType.Identifier), ",", ";");
        }

        private Token ReadReturnType()
        {
            var token = tokenizer.Read();
            if (token.Is(TokenType.Keyword, "void"))
                return token;

            tokenizer.PushBack(token);
            return ReadType();
        }

        private Token ReadType()
        {
            // type: int, char, boolean или имя класса.
            var token = tokenizer.Read();
            if (token.IsOneOf("int", "char", "boolean") || token.TokenType == TokenType.Identifier)
                return token;

            throw new ExpectedException("type", token);
        }

        private static Token CreateMissingKeyword(string value, Token firstToken)
        {
            // В дереве LetStatementSyntax/DoStatementSyntax поле для let/do обязательно.
            // Если слова не было в тексте, создаем служебный токен на позиции первого реального токена команды.
            return new Token(TokenType.Keyword, value, firstToken.LineNumber, firstToken.ColNumber);
        }
    }
}
