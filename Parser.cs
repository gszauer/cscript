
namespace CScript {
    class Parser {
        public List<Pass0.Statement> Program { get; protected set; } // List of statements

        protected List<Token> mTokens;
        protected int mCurrent;
        protected Token mCurrentFunctionReturnType;
        public Parser(List<Token> tokens) {
            Program = new List<Pass0.Statement>();
            mTokens = tokens;
            mCurrent = 0;
            mCurrentFunctionReturnType = null;

            Pass0.Statement statement = ParseDeclaration();
            while (statement != null) {
                Program.Add(statement);
                statement = ParseDeclaration();
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
                t.Type == TokenType.TYPE_FLOAT ||
                t.Type == TokenType.TYPE_CHAR ||
                t.Type == TokenType.TYPE_BOOL ||
                t.Type == TokenType.IDENTIFIER;
        }

        Token ConsumeVariableType() {
            if (IsVariableType(Current)) {
                Token t = Current;
                mCurrent++;
                return t;
            }
            throw new CompilerException(ExceptionSource.PARSER, Current.Location, "Invalid variable type: " + Current.Lexeme);
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

        Pass0.Statement ParseDeclaration() {
            if (Current.Type == TokenType.EOF) {
                return null;
            }

            if ((IsVariableType(Current) || Current.Type == TokenType.TYPE_VOID) && Peek(1).Type == TokenType.IDENTIFIER) {
                if (Peek(2).Type == TokenType.EQUAL || Peek(2).Type == TokenType.SEMICOLON) {
                    return ParseVarDeclStatement();
                }
                else if (Peek(2).Type == TokenType.LPAREN) {
                    return ParseFunDeclStatement();
                }
            }

            throw new CompilerException(ExceptionSource.PARSER, Current.Location, "Error parsing top level declaration. Only variable and function declarations are allowed at the top level. Lexeme: " + Current.Lexeme);
        }

        Pass0.Statement ParseFunDeclStatement() {
            if (mCurrentFunctionReturnType != null) {
                throw new CompilerException(ExceptionSource.PARSER, Current.Location, "Can't parse nested functions");
            }

            Token returnType = Current.Type != TokenType.TYPE_VOID? ConsumeVariableType() : Consume(TokenType.TYPE_VOID);
            mCurrentFunctionReturnType = returnType;
            Token functionName = Consume("Function name can only be an identifier", TokenType.IDENTIFIER);
            Consume("Function name must be followed by (", TokenType.LPAREN);
            List<Pass0.FunParamater> paramaters = new List<Pass0.FunParamater>();
            while(Current.Type != TokenType.RPAREN && Current.Type != TokenType.EOF) {
                Token paramType = ConsumeVariableType();
                Token paramName = Consume("Param name can only be an identifier", TokenType.IDENTIFIER);
                Pass0.FunParamater param = new Pass0.FunParamater(paramName, paramType);
                if (Current.Type == TokenType.COMMA) {
                    Consume(TokenType.COMMA);
                }
                paramaters.Add(param);
            }
            Consume("Function argument list must be followed by )", TokenType.RPAREN);

            Consume("Function arguments must be followed by {", TokenType.LBRACE);
            List<Pass0.Statement> functionBody = new List<Pass0.Statement>();
            while (Current.Type != TokenType.RBRACE && Current.Type != TokenType.EOF) {
                functionBody.Add(ParseStatement());
            }
            Consume("Function body must be followed by }", TokenType.RBRACE);
            mCurrentFunctionReturnType = null;

            return new Pass0.FunDeclStatement(returnType, functionName, paramaters, functionBody);
        }

        Pass0.Statement ParseStatement() {
            if (Current.Type == TokenType.PRINT) {
                return ParsePrintStatement();
            }
            else if (Current.Type == TokenType.LBRACE) {
                return ParseBlockStatement();
            }
            else if (Current.Type == TokenType.RETURN) {
                return ParseReturnStatement();
            }
            if (IsVariableType(Current) && Peek(1).Type == TokenType.IDENTIFIER) {
                if (Peek(2).Type == TokenType.EQUAL || Peek(2).Type == TokenType.SEMICOLON) {
                    return ParseVarDeclStatement();
                }
            }
            return ParseExpressionStatement();
        }


        Pass0.Statement ParseReturnStatement() {
            if (mCurrentFunctionReturnType == null) {
                throw new CompilerException(ExceptionSource.PARSER, Current.Location, "Return statement can only appear inside of a function");
            }

            Token statement = Consume(TokenType.RETURN);
            Pass0.Expression returnValue = null;

            if (Current.Type != TokenType.SEMICOLON) {
                returnValue = ParseExpression();
            }

            Consume("Return statement must be followed by a semicolon", TokenType.SEMICOLON);

            if (mCurrentFunctionReturnType.Type == TokenType.TYPE_VOID) {
                if (returnValue != null) {
                    throw new CompilerException(ExceptionSource.PARSER, Current.Location, "Can't return a value from a void function");
                }
            }
            else {
                if (returnValue == null) {
                    throw new CompilerException(ExceptionSource.PARSER, Current.Location, "Function must return a " + mCurrentFunctionReturnType.Type);
                }
            }

            return new Pass0.ReturnStatement(statement, returnValue);
        }
        Pass0.Statement ParseBlockStatement() {
            Token brace = Consume("Block statement must start with {", TokenType.LBRACE);
            List<Pass0.Statement> body = new List<Pass0.Statement>();
            while (Current.Type != TokenType.RBRACE && Current.Type != TokenType.EOF) {
                body.Add(ParseStatement());
            }
            Consume("Block statement must end with }", TokenType.RBRACE);
            return new Pass0.BlockStatement(brace, body);
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
            Token variableType = ConsumeVariableType();
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
            return ParseAssignment();
        }

        Pass0.Expression ParseAssignment() {
            Pass0.Expression expr = ParseAddSub();

            if (Current.Type == TokenType.EQUAL) {
                Consume(TokenType.EQUAL);

                if (expr is Pass0.VariableExpression) {
                    Pass0.VariableExpression var = (Pass0.VariableExpression)expr;
                    Pass0.Expression value = ParseAssignment();

                    return new Pass0.AssignmentExpression(var.Name, value);
                }
                throw new CompilerException(ExceptionSource.PARSER, expr.Location, "Assignment is only valid for variable expressions");
            }

            return expr;
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

            return ParseCall();
        }

        Pass0.Expression ParseCall() {
            Pass0.Expression callee = ParsePrimary();

            while(Current.Type == TokenType.LPAREN) {
                Token callSite = Consume(TokenType.LPAREN);

                List<Pass0.Expression> args = new List<Pass0.Expression>();
                while(Current.Type != TokenType.RPAREN && Current.Type != TokenType.EOF) {
                    args.Add(ParseExpression());
                    if (Current.Type == TokenType.COMMA) {
                        Consume(TokenType.COMMA);
                    }
                }

                if (!(callee is Pass0.VariableExpression || callee is Pass0.CallExpression)) {
                    throw new CompilerException(ExceptionSource.PARSER, callee.Location, "Calee must be a variable or call expression");
                }

                callee = new Pass0.CallExpression(callee, callSite, args);
                Consume("Call expression must end with )", TokenType.RPAREN);
            }

            return callee;
        }

        Pass0.Expression ParsePrimary() {
            Token current = Current;

            if (current.Type == TokenType.LIT_NUMBER || current.Type == TokenType.LIT_TRUE || current.Type == TokenType.LIT_FALSE || current.Type == TokenType.LIT_CHAR) {
                Consume(TokenType.LIT_NUMBER, TokenType.LIT_TRUE, TokenType.LIT_FALSE, TokenType.LIT_CHAR);
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
            else if (current.Type == TokenType.LIT_TRUE) {
                Consume(TokenType.LIT_TRUE);
                return new Pass0.LiteralExpression(current);
            }

            string message = "Unexpected token: " + current.Lexeme;
            throw new CompilerException(ExceptionSource.PARSER, current.Location, message);
        }
    }
}
