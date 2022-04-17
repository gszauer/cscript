namespace CScript {
    namespace Runtime {
        enum StatementResultType {
            NORMAL
        }
        class StatementResult {
            public StatementResultType Type { get; protected set; }

            public StatementResult(StatementResultType type) {
                this.Type = type;
            }

            public StatementResult() {
                Type = StatementResultType.NORMAL;
            }
        }
        class Interpreter : Pass0.ExpressionVisitor<object>, Pass0.StatementVisitor<StatementResult> {
            public Environment Global { get; protected set; }
            public Interpreter(AbstractSyntaxTree ast) {
                Global = new Environment(null);
                foreach(Pass0.Statement s in ast.Program) {
                    ExecuteStatement(s, Global);
                }
            }
            public void RunFunction(string name) {
                throw new NotImplementedException();
            }

            protected object EvaluateExpression(Pass0.Expression e, object o) {
                return e.Accept(this, o);
            }
            protected StatementResult ExecuteStatement(Pass0.Statement s, object o) {
                return s.Accept(this, o);
            }

            public object VisitBinaryExpression(Pass0.BinaryExpression expr, object misc) {
                object left = EvaluateExpression(expr.Left, misc);
                object right = EvaluateExpression(expr.Right, misc);

                if (expr.Operator.Type == TokenType.PLUS) {
                    if (left is string || right is string) {
                        if (left == null) {
                            left = "null";
                        }
                        if (right == null) {
                            right = "null";
                        }

                        return left.ToString() + right.ToString();
                    }
                    if (left is int && right is int) {
                        return ((int)left) + ((int)right);
                    }
                    if (left is double && right is double) {
                        return ((double)left) + ((double)right);
                    }
                    throw new InterpreterException(expr.Location, "Addition is only defined between ints, floats, and any string");
                }
                else if (expr.Operator.Type == TokenType.MINUS) {
                    if (left is int && right is int) {
                        return ((int)left) - ((int)right);
                    }
                    if (left is double && right is double) {
                        return ((double)left) - ((double)right);
                    }
                    throw new InterpreterException(expr.Location, "Only two integers or two doubles can subtract");
                }
                else if (expr.Operator.Type == TokenType.STAR) {
                    if (left is int && right is int) {
                        return ((int)left) * ((int)right);
                    }
                    if (left is double && right is double) {
                        return ((double)left) * ((double)right);
                    }
                    throw new InterpreterException(expr.Location, "Only two integers or two doubles can multiply");
                }
                else if (expr.Operator.Type == TokenType.SLASH) {
                    if (left is int && right is int) {
                        return ((int)left) / ((int)right);
                    }
                    if (left is double && right is double) {
                        return ((double)left) / ((double)right);
                    }
                    throw new InterpreterException(expr.Location, "Only two integers or two doubles can divide");
                }

                throw new InterpreterException(expr.Location, "Binary expression failed");
            }
            public object VisitUnaryExpression(Pass0.UnaryExpression expr, object misc) {
                object value = EvaluateExpression(expr.Expression, misc);

                if (expr.Operator.Type == TokenType.MINUS) {
                    if (!(value is int) && !(value is double)) {
                        throw new InterpreterException(expr.Token.Location, "Trying to negate not an int or double");
                    }
                    if (value is int) {
                        return -((int)value);
                    }
                    else if (value is double) {
                        return -((double)value);
                    }
                }
                else if (expr.Operator.Type == TokenType.NOT) {
                    if (!(value is bool)) {
                        throw new InterpreterException(expr.Token.Location, "Trying to '!' not a not boolean");
                    }
                    return !((bool)value);
                }

                throw new InterpreterException(expr.Location, "Unary expression failed");
            }
            public object VisitVariableExpression(Pass0.VariableExpression expr, object misc) {
                Environment env = (Environment)misc;
                return env.Get(expr.Name.Lexeme, expr.Location);
            }
            public object VisitLiteralExpression(Pass0.LiteralExpression expr, object misc) {
                if (expr.Type == TokenType.LIT_NUMBER) {
                    if (expr.Lexeme.Contains(".")) {
                        return Double.Parse(expr.Lexeme);
                    }
                    return Int32.Parse(expr.Lexeme);
                }
                throw new InterpreterException(expr.Token.Location, "Unknown literal expression type");
            }
            public StatementResult VisitPrintStatement(Pass0.PrintStatement stmt, Object misc) {
                object obj = EvaluateExpression(stmt.Expression, misc);

                if (obj is int) {
                    Console.Write(obj.ToString());
                    return new StatementResult();
                }

                throw new InterpreterException(stmt.Location, "Printing unknown type");
            }
            public StatementResult VisitVarDeclStatement(Pass0.VarDeclStatement stmt, Object misc) {
                string variableName = stmt.Name.Lexeme;
                
                object variableValue = null;
                if (stmt.Initializer != null) {
                    variableValue = EvaluateExpression(stmt.Initializer, misc);
                }
                else {
                    if (stmt.Type.Type == TokenType.TYPE_INT) {
                        variableValue = 0;
                    }
                    else {
                        throw new InterpreterException(stmt.Location, "Can't assign default value to type");
                    }
                }

                Environment env = (Environment)misc;
                env.Declare(variableName, stmt.Location);
                env.Set(variableName, variableValue, stmt.Location);

                return new StatementResult();
            }
            public StatementResult VisitExpressionStatement(Pass0.ExpressionStatement stmt, Object misc) {
                EvaluateExpression(stmt.Expression, misc);
                return new StatementResult();
            }
        }
    }
}
