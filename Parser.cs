
namespace CScript {
    class Parser {
        public List<Pass0.Statement> Program { get; protected set; } // List of statements

        protected List<Token> mTokens;
        protected int mCurrent;
        public Parser(List<Token> tokens) {
            Program = new List<Pass0.Statement>();
            mTokens = tokens;
            mCurrent = 0;

            Pass0.Statement statement = ParseStatement();
            while (statement != null) {
                Program.Add(statement);
                statement = ParseStatement();
            }

            mTokens = null;
        }

        Token Current {
            get {
                if (mCurrent >= mTokens.Count) {
                    return new Token(TokenType.EOF, -1, "Error", "Error, read past the end of token stream");
                }
                return mTokens[mCurrent];
            }
        }

        bool IsVariableType(Token t) {
            return t.Type == TokenType.TYPE_INT ||
                t.Type == TokenType.IDENTIFIER;
        }

        Token Peek(int howMany) {
            if (mCurrent + howMany >= mTokens.Count) {
                return new Token(TokenType.EOF, -1, "Error", "Error, read past the end of token stream");
            }
            return mTokens[mCurrent + howMany];
        }

        Token Consume(string error, params TokenType[] types) {
            Token current = Current;
            foreach (TokenType type in types) {
                if (type == current.Type) {
                    mCurrent++;
                    return current;
                }
            }
            throw new CompilerException(ExceptionSource.PARSER, current.Location, error);
        }

        Token Consume(params TokenType[] types) {
            string error = "Trying to consume: " + Current.Type + ", but expecting: ";
            for (int i = 0; i < types.Length - 1; ++i) {
                error += types[i] + ", ";
            }
            error += types[types.Length - 1] + ".";
            return Consume(error, types);
        }

        Pass0.Statement ParseStatement() {
            if (Current.Type == TokenType.EOF) {
                return null;
            }

            if (Current.Type == TokenType.PRINT) {
                return ParsePrintStatement();
            }
            if (IsVariableType(Current) && Peek(1).Type == TokenType.IDENTIFIER) {
                if (Peek(2).Type == TokenType.EQUAL || Peek(2).Type == TokenType.SEMICOLON) {
                    return ParseVarDeclStatement();
                }
                // Functions
            }
            return ParseExpressionStatement();
        }

        Pass0.Statement ParsePrintStatement() {
            Token print = Consume(TokenType.PRINT);
            Consume("Expected '(' after print keyword", TokenType.LPAREN);

            Pass0.Expression expression = null;
            if (Current.Type != TokenType.RPAREN) {
                expression = ParseExpression();
            }

            Consume("Expected ')' after print arguments", TokenType.RPAREN);
            Consume("Expected ';' after print statement", TokenType.SEMICOLON);
            
            return new Pass0.PrintStatement(print, expression);
        }

        Pass0.Statement ParseVarDeclStatement() {
            Token variableType = Consume("Invalid variable declaration type: " + Current.Lexeme, TokenType.IDENTIFIER, TokenType.TYPE_INT);
            Token variableName = Consume("Invalid variable declaration name: " + Current.Lexeme, TokenType.IDENTIFIER);
            Pass0.Expression initializer = null;
            if (Current.Type == TokenType.EQUAL) {
                Consume(TokenType.EQUAL);
                initializer = ParseExpression();
            }
            Consume("Expected ';' after variable declarataion", TokenType.SEMICOLON);

            return new Pass0.VarDeclStatement(variableType, variableName, initializer);
        }

        Pass0.Statement ParseExpressionStatement() {
            Pass0.Expression expression = ParseExpression();
            Consume("Expected ';' after expression", TokenType.SEMICOLON);
            return new Pass0.ExpressionStatement(expression);
        }

        Pass0.Expression ParseExpression() {
            return ParseAddSub();
        }

        Pass0.Expression ParseAddSub() {
            Pass0.Expression left = ParseMulDiv();

            while (Current.Type == TokenType.PLUS || Current.Type == TokenType.MINUS) {
                Token _operator = Consume(TokenType.PLUS, TokenType.MINUS);
                Pass0.Expression right = ParseMulDiv();
                left = new Pass0.BinaryExpression(left, _operator, right);
            }

            return left;
        }

        Pass0.Expression ParseMulDiv() {
            Pass0.Expression left = ParseUnary();

            while (Current.Type == TokenType.STAR || Current.Type == TokenType.SLASH) {
                Token _operator = Current;
                Consume(TokenType.STAR, TokenType.SLASH);
                Pass0.Expression right = ParseUnary();
                left = new Pass0.BinaryExpression(left, _operator, right);
            }

            return left;
        }

        Pass0.Expression ParseUnary() {
            if (Current.Type == TokenType.MINUS || Current.Type == TokenType.NOT) {
                Token _operator = Current;
                Consume(TokenType.MINUS, TokenType.NOT);
                Pass0.Expression expression = ParseUnary();
                return new Pass0.UnaryExpression(_operator, expression);
            }

            return ParsePrimary();
        }

        Pass0.Expression ParsePrimary() {
            Token current = Current;

            if (current.Type == TokenType.LIT_NUMBER) {
                Consume(TokenType.LIT_NUMBER);
                return new Pass0.LiteralExpression(current);
            }
            else if (current.Type == TokenType.IDENTIFIER) {
                Consume(TokenType.LIT_NUMBER, TokenType.IDENTIFIER);
                return new Pass0.VariableExpression(current);
            }
            else if (current.Type == TokenType.LPAREN) {
                Consume(TokenType.LPAREN);
                Pass0.Expression expression = ParseExpression();
                Consume("Expected matching )", TokenType.RPAREN);
                return expression;
            }

            string message = "Unexpected token: " + current.Lexeme;
            throw new CompilerException(ExceptionSource.PARSER, current.Location, message);
        }
    }
}
