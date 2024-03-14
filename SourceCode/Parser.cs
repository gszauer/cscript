namespace CScript {
    public class Parser {
        protected class State {
            public List<Token> Tokens { get; protected set; }
            public List<ParseTree.Declaration.File> Tree { get; protected set; }
            public TypeDatabase Types { get; protected set; }

            public int Current = 0;
            public State(List<Token> tokens, TypeDatabase types) {
                Tokens = tokens;
                Tree = new List<ParseTree.Declaration.File>();
                Types = types;
                Types.AddTokens(tokens);
                Current = 0;
            }
        }

        protected static void Error(string str, Location loc) {
            Compiler.Error("Parser", str, loc);
        }
        public static List<ParseTree.Declaration.File> Parse(List<Token> inputStream, TypeDatabase types) {
            State state = new State(inputStream, types);

            List<ParseTree.Declaration> fileContent = null;
            while (!IsAtEnd(state)) {
                if (Check(state, Symbol.FILE)) {
                    Token name = Consume(state, Symbol.FILE);
                    fileContent = new List<ParseTree.Declaration>();
                    state.Tree.Add(new ParseTree.Declaration.File(name.Lexeme, fileContent));
                }
                else {
                    if (fileContent == null) {
                        Error("No file?", new Location("Generated", 0, 0));
                    }
                    ParseTree.Declaration decl = ParseDeclaration(state);
                    fileContent.Add(decl);
                }
            }

            return state.Tree;
        }
        
        protected static ParseTree.Declaration ParseDeclaration(State state) {
            Token peek = Peek(state, 0);
            if (Check(state, Symbol.STRUCT)) {
                return ParseStructDeclaration(state);
            }
            else if (Check(state, Symbol.ENUM)) {
                return ParseEnumDeclaration(state);
            }
            else if (Check(state, Symbol.DELEGATE)) {
                return ParseDelegateDeclaration(state);
            }

            // Variable or Function
            ParseTree.Type type = ParseType(state);
            Token name = Consume(state, Symbol.IDENTIFIER, Symbol.TYPE_STRING);
            if(name.Symbol != Symbol.IDENTIFIER) {
                name.Symbol = Symbol.IDENTIFIER;
            }
            state.Types.ValidateNewTypeName(name);

            if (Check(state, Symbol.SEMICOLON, Symbol.EQUAL)) {
                return ParseVariableDeclaration(state, type, name);
            }
            else if (Check(state, Symbol.LPAREN)) {
                return ParseFunctionDeclaration(state, type, name);
            }

            Error("Could not parse top level declaration", peek.Location);
            return null;
        }
        protected static ParseTree.Statement ParseStatement(State s) {
            Token peek = Peek(s, 0);
            if (Check(s, Symbol.LBRACE)) {
                return ParseBlockStatement(s);
            }
            else if (Check(s, Symbol.IF)) {
                return ParseIfStatement(s);
            }
            else if (Check(s, Symbol.RETURN, Symbol.CONTINUE, Symbol.BREAK)) {
                return ParseControlStatement(s);
            }
            else if (Check(s, Symbol.WHILE)) {
                return ParseWhileStatement(s);
            }
            else if (Check(s, Symbol.FOR)) {
                return ParseForStatement(s);
            }
            
            if (PeekVariable(s)) {
                return ParseVariableStatement(s);
            }

            return ParseExpressionStatement(s);
        }
        protected static ParseTree.Expression ParseExpression(State s) {
            ParseTree.Expression result = ParseAssignmentExpression(s);
            return result;
        }

        protected static ParseTree.Expression ParseAssignmentExpression(State s) {
            ParseTree.Expression result = ParseLogicalExpression(s);

            Symbol[] symbols = new Symbol[] { //Symbol.TILDE_EQUAL <- this is an equality check later
                Symbol.PLUS_EQUAL, Symbol.MINUS_EQUAL, Symbol.SLASH_EQUAL,
                Symbol.STAR_EQUAL, Symbol.POW_EQUAL, Symbol.EQUAL, Symbol.MOD_EQUAL
            };

            if (Check(s, symbols)) {
                Token oper = Consume(s, symbols);
                ParseTree.Expression value = ParseAssignmentExpression(s);

                if (result is ParseTree.Expression.Get) {
                    ParseTree.Expression.Get getExpr = (ParseTree.Expression.Get)result;
                    if (getExpr.Object == null) { // Variable expression
                        result = new ParseTree.Expression.Set(null, getExpr.Property, oper, value);
                    }
                    else { // Actual get
                        result = new ParseTree.Expression.Set(getExpr.Object, getExpr.Property, oper, value);
                    }
                }
                else if (result is ParseTree.Expression.Call) {
                    ParseTree.Expression.Call callExpr = (ParseTree.Expression.Call)result;
                    if (callExpr.CallSite.Symbol != Symbol.LBRACKET) {
                        Error("Can't assign to result of call expression", oper.Location);
                        return null;
                    }
                    else {
                        result = new ParseTree.Expression.Set(callExpr, null, oper, value);
                    }
                }
                else {
                    Error("Left side of assignment should be a variable or get expression", oper.Location);
                    return null;
                }
            }

            return result;
        }

        protected static ParseTree.Expression ParseLogicalExpression(State s) {
            ParseTree.Expression result = ParseEqualityExpression(s);

            while (Check(s, Symbol.AND, Symbol.OR)) {
                Token oper = Consume(s, Symbol.AND, Symbol.OR);
                ParseTree.Expression right = ParseEqualityExpression(s);
                result = new ParseTree.Expression.Binary(oper, result, right);
            }

            return result;
        }
        protected static ParseTree.Expression ParseEqualityExpression(State s) {
            ParseTree.Expression result = ParseComparisonExpression(s);

            Symbol[] symbols = new Symbol[] {
                Symbol.EQUAL_EQUAL, Symbol.NOT_EQUAL, Symbol.TILDE_EQUAL
            };
            while (Check(s, symbols)) {
                Token oper = Consume(s, symbols);
                ParseTree.Expression right = ParseComparisonExpression(s);
                result = new ParseTree.Expression.Binary(oper, result, right);
            }

            return result;
        }

        protected static ParseTree.Expression ParseComparisonExpression(State s) {
            ParseTree.Expression result = ParseTermExpression(s);

            Symbol[] symbols = new Symbol[] {
                Symbol.GREATER, Symbol.GREATER_EQUAL, Symbol.LESS, Symbol.LESS_EQUAL
            };
            while (Check(s, symbols)) {
                Token oper = Consume(s, symbols);
                ParseTree.Expression right = ParseTermExpression(s);
                result = new ParseTree.Expression.Binary(oper, result, right);
            }

            return result;
        }

        protected static ParseTree.Expression ParseTermExpression(State s) {
            ParseTree.Expression result = ParseFactorExpression(s);

            while (Check(s, Symbol.PLUS, Symbol.MINUS)) {
                Token oper = Consume(s, Symbol.PLUS, Symbol.MINUS);
                ParseTree.Expression right = ParseFactorExpression(s);
                result = new ParseTree.Expression.Binary(oper, result, right);
            }

            return result;
        }

        protected static ParseTree.Expression ParseFactorExpression(State s) {
            ParseTree.Expression result = ParsePowerExpression(s);

            while (Check(s, Symbol.SLASH, Symbol.STAR, Symbol.MOD)) {
                Token oper = Consume(s, Symbol.SLASH, Symbol.STAR, Symbol.MOD);
                ParseTree.Expression right = ParsePowerExpression(s);
                result = new ParseTree.Expression.Binary(oper, result, right);
            }

            return result;
        }

        protected static ParseTree.Expression ParsePowerExpression(State s) {
            ParseTree.Expression result = ParseUnaryExpression(s);

            while (Check(s, Symbol.POW)) {
                Token oper = Consume(s, Symbol.POW);
                ParseTree.Expression right = ParseUnaryExpression(s);
                result = new ParseTree.Expression.Binary(oper, result, right);
            }

            return result;
        }

        protected static ParseTree.Expression ParseUnaryExpression(State s) {
            Symbol[] symbols = new Symbol[] {
                Symbol.NOT, Symbol.MINUS, Symbol.TILDE, Symbol.PLUS_PLUS, Symbol.MINUS_MINUS
            };

            if (Check(s, symbols)) {
                Token oper = Consume(s, symbols);
                ParseTree.Expression right = ParseUnaryExpression(s);
                return new ParseTree.Expression.Unary(oper, right, ParseTree.Expression.Unary.OperatorType.PREFIX);
            }

            return ParsePostExpression(s);
        }

        protected static ParseTree.Expression ParsePostExpression(State s) {
            ParseTree.Expression left = ParseCsatExpression(s);
            if (Check(s, Symbol.PLUS_PLUS, Symbol.MINUS_MINUS)) {
                Token oper = Consume(s, Symbol.PLUS_PLUS, Symbol.MINUS_MINUS);
                return new ParseTree.Expression.Unary(oper, left, ParseTree.Expression.Unary.OperatorType.POSTFIX);
            }

            return left;
        }

        protected static ParseTree.Expression ParseCsatExpression(State s) {
            ParseTree.Expression result = ParseCallExpression(s);
            if (Check(s, Symbol.AS)) {
                Token oper = Consume(s, Symbol.AS);
                ParseTree.Type varType = ParseType(s);
                return new ParseTree.Expression.Cast(result, oper, varType);
            }
            return result;
        }

        protected static ParseTree.Expression ParseCallExpression(State s) {
            ParseTree.Expression result = ParsePrimaryExpression(s, true);

            while (Check(s, Symbol.LPAREN, Symbol.LBRACKET, Symbol.DOT)) {
                Token peek = Peek(s, 0);
                if (Check(s, Symbol.LPAREN, Symbol.LBRACKET)) {
                    List<ParseTree.Expression> arguments = ParseFunctionArguments(s, peek.Symbol == Symbol.LBRACKET);
                    if (!(result is ParseTree.Expression.Get) && !(result is ParseTree.Expression.Call)) {
                        Error("Can only call the result of get or call expressions", peek.Location);
                    }
                    result = new ParseTree.Expression.Call(peek, result, arguments);
                }
                else if (Check(s, Symbol.DOT)) {
                    Consume(s, Symbol.DOT);
                    Token prop = Consume(s, Symbol.IDENTIFIER);
                    result = new ParseTree.Expression.Get(result, prop);
                }
            }

            return result;
        }
        protected static ParseTree.Expression ParseNewExpression(State s) {
            if (Check(s, Symbol.NEW)) {
                Token oper = Consume(s, Symbol.NEW);
                ParseTree.Type type = ParseType(s);
                List<ParseTree.Expression> args = ParseFunctionArguments(s, false);
                return new ParseTree.Expression.New(oper, type, args);
            }

            return ParseCallExpression(s);
        }
        protected static ParseTree.Expression ParsePrimaryExpression(State s, bool stringAsLiteral = false) {
            Token debug_peek = Peek(s, 0);

            bool isLiteral = Check(s, Symbol.LIT_BOOL, Symbol.LIT_CHAR,Symbol.LIT_NUM, Symbol.LIT_STRING, Symbol.LIT_NULL);

            if (isLiteral) {
                return ParseLiteralExpression(s);
            }
            if (Check(s, Symbol.NEW)) {
                return ParseNewExpression(s);
            }
            else if (Check(s, Symbol.LPAREN)) {
                Consume(s, Symbol.LPAREN);
                ParseTree.Expression target = ParseExpression(s);
                Consume(s, Symbol.RPAREN);
                return new ParseTree.Expression.Group(target);
            }
            else if (Check(s, Symbol.IDENTIFIER)) {
                Token name = Consume(s, Symbol.IDENTIFIER);
                return new ParseTree.Expression.Get(null, name);
            }
            else if (stringAsLiteral && Check(s, Symbol.TYPE_STRING)) {
                Token name = Consume(s, Symbol.TYPE_STRING);
                name.Symbol = Symbol.IDENTIFIER;
                return new ParseTree.Expression.Get(null, name);
            }

            if (Check(s, Symbol.TYPE_BOOL, Symbol.TYPE_CHAR, Symbol.TYPE_NUM, Symbol.TYPE_OBJECT, Symbol.TYPE_STRING, Symbol.TYPE_VOID)) {
                Error("Parsed a primitive type when something else (variable?) was expected", debug_peek.Location);
                return null;
            }

            Error("Could not parse primary expression", debug_peek.Location);
            return null;
        }

        protected static ParseTree.Expression ParseLiteralExpression(State s) {
            Token literal = Consume(s, Symbol.LIT_BOOL, Symbol.LIT_CHAR, Symbol.LIT_NULL, Symbol.LIT_NUM, Symbol.LIT_STRING);
            return new ParseTree.Expression.Literal(literal);
        }

        protected static List<ParseTree.Expression> ParseFunctionArguments(State s, bool index) {
            List<ParseTree.Expression> result = new List<ParseTree.Expression>();
            Symbol left = index ? Symbol.LBRACKET : Symbol.LPAREN;
            Symbol right = index ? Symbol.RBRACKET : Symbol.RPAREN;
            if (Check(s, left)) {
                Consume(s, left);
                while (!Check(s, right)) {
                    ParseTree.Expression iter = ParseExpression(s);
                    result.Add(iter);
                    if (Check(s, Symbol.COMMA)) {
                        Consume(s, Symbol.COMMA);
                    }
                }
                Consume(s, right);
            }
            return result;
        }

        protected static ParseTree.Statement.For ParseForStatement(State s) {
            Token keyword = Consume(s, Symbol.FOR);
            Consume(s, Symbol.LPAREN);

            ParseTree.Type initializerType = null;
            if (!Check(s, Symbol.SEMICOLON)) {
                initializerType = ParseType(s);
            }

            List<ParseTree.Statement.Variable> initializers = new List<ParseTree.Statement.Variable>();
            while (!Check(s, Symbol.SEMICOLON)) {
                Token name = Consume(s, Symbol.IDENTIFIER);
                ParseTree.Expression init = null;
                if (Check(s, Symbol.EQUAL)) {
                    Consume(s, Symbol.EQUAL);
                    init = ParseExpression(s);
                }
                if (Check(s, Symbol.COMMA)) {
                    Consume(s, Symbol.COMMA);
                }
                initializers.Add(new ParseTree.Statement.Variable(initializerType.MakeCopy(), name, init));
            }
            Consume(s, Symbol.SEMICOLON);

            ParseTree.Expression condition = null;
            if (!Check(s, Symbol.SEMICOLON)) {
                condition = ParseExpression(s);
            }
            Consume(s, Symbol.SEMICOLON);

            if (Check(s, Symbol.SEMICOLON)) {
                Error("Too many semicolons in for loop", keyword.Location);
            }

            List<ParseTree.Expression> iterators = new List<ParseTree.Expression>();
            while (!Check(s, Symbol.RPAREN)) {
                ParseTree.Expression iter = ParseExpression(s);
                iterators.Add(iter);
                if (Check(s, Symbol.COMMA)) {
                    Consume(s, Symbol.COMMA);
                }
            }
            Consume(s, Symbol.RPAREN);

            ParseTree.Statement.Block body = null;
            if (Check(s, Symbol.SEMICOLON)) {
                Consume(s, Symbol.SEMICOLON);
            }
            else {
                body = ParseBlockStatement(s);
            }

            ParseTree.Statement.For result = new ParseTree.Statement.For(keyword, initializers, condition, iterators, body);
            return result;
        }
        protected static ParseTree.Statement.If ParseIfStatement(State s) {
            Token keyword = Consume(s, Symbol.IF);
            Consume(s, Symbol.LPAREN);
            ParseTree.Expression condition = null;
            if (!Check(s, Symbol.RPAREN)) {
                condition = ParseExpression(s);
            }
            Consume(s, Symbol.RPAREN);
            ParseTree.Statement.Block body = ParseBlockStatement(s);

            ParseTree.Statement.If result = new ParseTree.Statement.If(keyword, condition, body);
            ParseTree.Statement.If iter = result;

            while (Check(s, Symbol.ELSE)) {
                if (Peek(s, 1).Symbol != Symbol.IF) {
                    break;
                }

                Consume(s, Symbol.ELSE);
                keyword = Consume(s, Symbol.IF);
                condition = null;
                Consume(s, Symbol.LPAREN);
                if (!Check(s, Symbol.RPAREN)) {
                    condition = ParseExpression(s);
                }
                Consume(s, Symbol.RPAREN);
                body = ParseBlockStatement(s);

                ParseTree.Statement.If elseif = new ParseTree.Statement.If(keyword, condition, body);
                iter.Next = elseif;
                iter = elseif;
            }

            if (Check(s, Symbol.ELSE)) {
                keyword = Consume(s, Symbol.ELSE);
                condition = null;
                body = ParseBlockStatement(s);
                ParseTree.Statement.If else_ = new ParseTree.Statement.If(keyword, condition, body);
                iter.Next = else_;
            }

            return result;
        }

        protected static ParseTree.Statement.While ParseWhileStatement(State s) {
            Token keyword = Consume(s, Symbol.WHILE);
            Consume(s, Symbol.LPAREN);
            ParseTree.Expression condition = null;
            if (!Check(s, Symbol.RPAREN)) {
                condition = ParseExpression(s);
            }
            Consume(s, Symbol.RPAREN);
            ParseTree.Statement.Block body = null;
            if (Check(s, Symbol.SEMICOLON)) {
                Consume(s, Symbol.SEMICOLON);
            }
            else {
                body = ParseBlockStatement(s);
            }

            return new ParseTree.Statement.While(keyword, condition, body);
        }
        protected static ParseTree.Statement.Control ParseControlStatement(State s) {
            Token keyword = Consume(s, Symbol.RETURN, Symbol.CONTINUE, Symbol.BREAK);
            ParseTree.Expression initializer = null;
            if (keyword.Symbol == Symbol.RETURN) {
                if (!Check(s, Symbol.SEMICOLON)) {
                    initializer = ParseExpression(s);
                }
            }
            Consume(s, Symbol.SEMICOLON);

            return new ParseTree.Statement.Control(keyword, initializer);
        }
        protected static ParseTree.Statement.Expression ParseExpressionStatement(State s) {
            ParseTree.Expression target = ParseExpression(s);
            Consume(s, Symbol.SEMICOLON);
            return new ParseTree.Statement.Expression(target);
        }
        protected static bool PeekVariable(State s) {
            if (Check(s, Symbol.TYPE_BOOL, Symbol.TYPE_CHAR, Symbol.TYPE_NUM, Symbol.TYPE_OBJECT, Symbol.TYPE_STRING, Symbol.IDENTIFIER)) {
                int c = 0;
                Token type = Peek(s, c++);
                Token name = Peek(s, c++);
                if (name.Symbol == Symbol.LBRACKET) {
                    int skip = 0;
                    while(Peek(s, c).Symbol != Symbol.RBRACKET) {
                        if (Peek(s, c).Symbol == Symbol.LBRACKET) {
                            skip += 1;
                        }
                        c += 1;
                        if (Peek(s, c).Symbol == Symbol.RBRACKET) {
                            if (skip == 0) {
                                break;
                            }
                            else {
                                skip -= 1;
                            }
                            c += 1;
                        }
                    }

                    Token rbracket = Peek(s, c);
                    if (rbracket.Symbol != Symbol.RBRACKET) {
                        Error("Expected [", rbracket.Location);
                    }
                    c += 1;

                    name = Peek(s, c++);
                }
                if (name.Symbol == Symbol.IDENTIFIER) {
                    Token control = Peek(s, c);
                    if (control.Symbol == Symbol.SEMICOLON || control.Symbol == Symbol.EQUAL) {
                        return true;
                    }
                }
            }

            return false;
        }
        protected static ParseTree.Declaration.Delegate ParseDelegateDeclaration(State s) {
            Consume(s, Symbol.DELEGATE);

            ParseTree.Type returnType = ParseType(s);
            Token name = Consume(s, Symbol.IDENTIFIER);
            s.Types.ValidateNewTypeName(name);

            Consume(s, Symbol.LPAREN);

            List<ParseTree.Declaration.Function.Paramater> paramaters = new List<ParseTree.Declaration.Function.Paramater>();
            while (!IsAtEnd(s) && !Check(s, Symbol.RPAREN)) {
                ParseTree.Type paramType = ParseType(s);
                Token paramName = Consume(s, Symbol.IDENTIFIER);

                paramaters.Add(new ParseTree.Declaration.Function.Paramater(paramType, paramName));

                if (Check(s, Symbol.COMMA)) {
                    Consume(s, Symbol.COMMA);
                }
            }
            Consume(s, Symbol.RPAREN);
            Consume(s, Symbol.SEMICOLON);

            ParseTree.Declaration.Delegate result = new ParseTree.Declaration.Delegate(returnType, name, paramaters);
            s.Types.RegisterDelegate(result);
            return result;
        }
        protected static ParseTree.Declaration.Enum ParseEnumDeclaration(State s) {
            Consume(s, Symbol.ENUM);

            Token name = Consume(s, Symbol.IDENTIFIER);
            s.Types.ValidateNewTypeName(name);

            Consume(s, Symbol.LBRACE);
            List<ParseTree.Declaration.Enum.Member> members = new List<ParseTree.Declaration.Enum.Member>();
            int memberVal = 0;
            while (!IsAtEnd(s) && !Check(s, Symbol.RBRACE)) {
                Token memberName = Consume(s, Symbol.IDENTIFIER);

                if (Check(s, Symbol.EQUAL)) {
                    Consume(s, Symbol.EQUAL);
                    Token newVal = Consume(s, Symbol.LIT_NUM);
                    if (newVal.Lexeme.Contains('.')) {
                        Error("Enum value must be an integer, decimals not allowed", newVal.Location);
                    }
                    int v = (int)Double.Parse(newVal.Lexeme);
                    if (v < memberVal) {
                        Error("Value (" + v + " can't be lower than enum counter (" + memberVal + ")", newVal.Location);
                    }
                    memberVal = v;
                }

                members.Add(new ParseTree.Declaration.Enum.Member(memberName, memberVal));

                memberVal += 1;

                if (Check(s, Symbol.COMMA)) {
                    Consume(s, Symbol.COMMA);
                }
            }
            Consume(s, Symbol.RBRACE);

            ParseTree.Declaration.Enum result = new ParseTree.Declaration.Enum(name, members);
            s.Types.RegisterEnum(result);
            return result;
        }
        protected static ParseTree.Declaration.Struct ParseStructDeclaration(State s) {
            Consume(s, Symbol.STRUCT);
            Token name = Consume(s, Symbol.IDENTIFIER);
            s.Types.ValidateNewTypeName(name);
            Consume(s, Symbol.LBRACE);
            List<ParseTree.Statement.Variable> variables = new List<ParseTree.Statement.Variable>();
            while (!IsAtEnd(s) && !Check(s, Symbol.RBRACE)) {
                variables.Add(ParseVariableStatement(s));
            }
            Consume(s, Symbol.RBRACE);

            ParseTree.Declaration.Struct result = new ParseTree.Declaration.Struct(name, variables);
            s.Types.RegisterStruct(result);
            return result;
        }
        protected static ParseTree.Declaration.Function ParseFunctionDeclaration(State s, ParseTree.Type type = null, Token name = null) {
            if (type == null && name == null) {
                type = ParseType(s);
                name = Consume(s, Symbol.IDENTIFIER);
            }

            List<ParseTree.Declaration.Function.Paramater> paramaters = new List<ParseTree.Declaration.Function.Paramater>();
            Consume(s, Symbol.LPAREN);
            while (!IsAtEnd(s) && !Check(s, Symbol.RPAREN)) {
                ParseTree.Type paramType = ParseType(s);
                Token paramName = Consume(s, Symbol.IDENTIFIER);
                ParseTree.Declaration.Function.Paramater paramater = new ParseTree.Declaration.Function.Paramater(paramType, paramName);
                paramaters.Add(paramater);

                if (Check(s, Symbol.COMMA)) {
                    Consume(s, Symbol.COMMA);
                }
            }
            Consume(s, Symbol.RPAREN);

            ParseTree.Statement.Block body = null;
            if (Check(s, Symbol.SEMICOLON)) {
                Compiler.Error("Parser", "Function must have body", Peek(s, 0).Location);
            }
            else {
                body = ParseBlockStatement(s);
            }

            ParseTree.Declaration.Function result = new ParseTree.Declaration.Function(type, name, paramaters, body);
            s.Types.RegisterFunction(result);
            return result;
        }
        protected static ParseTree.Statement.Block ParseBlockStatement(State s) {
            Token lbrace = Consume(s, Symbol.LBRACE);
            List<ParseTree.Statement> body = new List<ParseTree.Statement>();
            while (!IsAtEnd(s) && !Check(s, Symbol.RBRACE)) {
                ParseTree.Statement bod = ParseStatement(s);
                body.Add(bod);
            }
            Consume(s, Symbol.RBRACE);

            ParseTree.Statement.Block result = new ParseTree.Statement.Block(lbrace, body);
            return result;
        }
        protected static ParseTree.Declaration.Variable ParseVariableDeclaration(State s, ParseTree.Type type = null, Token name = null) {
            if (type == null && name == null) {
                type = ParseType(s);
                name = Consume(s, Symbol.IDENTIFIER);
            }

            ParseTree.Expression initializer = null;
            if (Check(s, Symbol.EQUAL)) {
                Consume(s, Symbol.EQUAL);
                initializer = ParseExpression(s);
            }

            Consume(s, Symbol.SEMICOLON);

            ParseTree.Declaration.Variable result = new ParseTree.Declaration.Variable(type, name, initializer);
            s.Types.RegisterVariable(result);
            return result;
        }

        protected static ParseTree.Statement.Variable ParseVariableStatement(State s) {
            ParseTree.Type type = ParseType(s);
            Token name = Consume(s, Symbol.IDENTIFIER);
            s.Types.ValidateNewLocalVariableName(name);

            ParseTree.Expression initializer = null;
            if (Check(s, Symbol.EQUAL)) {
                Consume(s, Symbol.EQUAL);
                initializer = ParseExpression(s);
            }

            Consume(s, Symbol.SEMICOLON);

            ParseTree.Statement.Variable result = new ParseTree.Statement.Variable(type, name, initializer);
            return result;
        }
        protected static bool IsAtEnd(State s) {
            return s.Current >= s.Tokens.Count;
        }
        protected static bool Check(State s, params Symbol[] types) {
            if (s.Current >= s.Tokens.Count) {
                return false;
            }
            Symbol peek = s.Tokens[s.Current].Symbol;
            foreach (Symbol type in types) {
                if (peek == type) {
                    return true;
                }
            }
            return false;
        }
        protected static Token Consume(State s, params Symbol[] types) {
            Token peek = Peek(s, 0);
            foreach (Symbol type in types) {
                if (peek.Symbol == type) {
                    s.Current += 1; // Advance
                    return peek;
                }
            }

            string error = " Expected: (";
            for (int i = 0, size = types.Length; i < size; ++i) {
                error += types[i].ToString();
                if (i != size - 1) {
                    error += ", ";
                }
            }
            error += "), Got: " + peek.Symbol.ToString() + " / " + peek.Lexeme;

            Error("Could not consume symbol: " + error, peek.Location);
            return null;
        }
        protected static Token Peek(State s, int offset) {
            int _current = s.Current + offset;
            if (_current < 0) {
                Error("Can't peek below zero", new Location("Generated", 0, 0));
            }
            if (_current >= s.Tokens.Count) {
                Error("Can't peek past end", new Location("Generated", 0, 0));
            }

            return s.Tokens[_current];
        }
        protected static ParseTree.Type ParseType(State s) {
            Symbol[] primitives = new Symbol[] { 
                Symbol.TYPE_BOOL, Symbol.TYPE_CHAR, 
                Symbol.TYPE_NUM,  Symbol.TYPE_OBJECT, 
                Symbol.TYPE_STRING, Symbol.TYPE_VOID,
                Symbol.IDENTIFIER
            };

            ParseTree.Type result = null;

            if (Check(s, primitives)) {
                Token primitiveType = Consume(s, primitives);
                result = new ParseTree.Type.Primitive(primitiveType);
            }
            else {
                Check(s, primitives);
                Error("No type primitive found", Peek(s, 0).Location);
            }
            
            while(Check(s, Symbol.LBRACKET)) {
                Token bracket = Consume(s, Symbol.LBRACKET);
                if (Check(s, Symbol.RBRACKET)) {
                    result = new ParseTree.Type.Array(result);
                    Consume(s, Symbol.RBRACKET);
                }
                else {
                    ParseTree.Type key = ParseType(s);
                    result = new ParseTree.Type.Map(key, result);
                    Consume(s, Symbol.RBRACKET);
                }
            }

            s.Types.RegisterType(result);
            return result;
        }
    }
}
