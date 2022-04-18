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
        class Interpreter : Pass1.ExpressionVisitor<object>, Pass1.StatementVisitor<StatementResult> {
            public Environment Global { get; protected set; }
            protected TypeTable Types;
            public Interpreter(AbstractSyntaxTree ast) {
                Global = new Environment(null);
                Types = ast.Types;
                foreach (Pass1.Statement s in ast.Program) {
                    ExecuteStatement(s, Global);
                }
                Types = null;
            }
            public void RunFunction(string name) {
                throw new NotImplementedException();
            }

            protected object EvaluateExpression(Pass1.Expression e, object o) {
                return e.Accept(this, o);
            }
            protected StatementResult ExecuteStatement(Pass1.Statement s, object o) {
                return s.Accept(this, o);
            }

            public object VisitBinaryExpression(Pass1.BinaryExpression expr, object misc) {
                object left = EvaluateExpression(expr.Left, misc);
                object right = EvaluateExpression(expr.Right, misc);

                if (expr.Operator == TokenType.PLUS) {
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
                else if (expr.Operator == TokenType.MINUS) {
                    if (left is int && right is int) {
                        return ((int)left) - ((int)right);
                    }
                    if (left is double && right is double) {
                        return ((double)left) - ((double)right);
                    }
                    throw new InterpreterException(expr.Location, "Only two integers or two doubles can subtract");
                }
                else if (expr.Operator == TokenType.STAR) {
                    if (left is int && right is int) {
                        return ((int)left) * ((int)right);
                    }
                    if (left is double && right is double) {
                        return ((double)left) * ((double)right);
                    }
                    throw new InterpreterException(expr.Location, "Only two integers or two doubles can multiply");
                }
                else if (expr.Operator == TokenType.SLASH) {
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
            public object VisitUnaryExpression(Pass1.UnaryExpression expr, object misc) {
                object value = EvaluateExpression(expr.Expression, misc);

                if (expr.Operator == TokenType.MINUS) {
                    if (value is int) {
                        return -((int)value);
                    }
                    else if (value is double) {
                        return -((double)value);
                    }
                    throw new InterpreterException(expr.Location, "Trying to negate not an int or double");
                }
                else if (expr.Operator == TokenType.NOT) {
                    if (!(value is bool)) {
                        throw new InterpreterException(expr.Location, "Trying to '!' not a not boolean");
                    }
                    return !((bool)value);
                }

                throw new InterpreterException(expr.Location, "Unary expression failed");
            }
            public object VisitVariableExpression(Pass1.VariableExpression expr, object misc) {
                Environment env = (Environment)misc;
                return env.Get(expr.Name, expr.Location);
            }
            public object VisitLiteralExpression(Pass1.LiteralExpression expr, object misc) {
                if (expr.Type == Types.IntID) {
                    return Int32.Parse(expr.Lexeme);
                }
                if (expr.Type == Types.FloatID) {
                    return Double.Parse(expr.Lexeme);
                }
                else if (expr.Type == Types.CharID) {
                    if (expr.Lexeme == "'\\n'") {
                        return '\n';
                    }
                    if (expr.Lexeme == "'\\t'") {
                        return '\t';
                    }
                    if (expr.Lexeme == "'\\r'") {
                        return '\r';
                    }
                    if (expr.Lexeme == "'\\0'") {
                        return '\0';
                    }
                    if (expr.Lexeme.Length == 3) {
                        string debug = char.Parse(expr.Lexeme.Substring(1, 1)).ToString();
                        return char.Parse(expr.Lexeme.Substring(1, 1));
                    }
                }
                else if (expr.Type == Types.BoolID) {
                    return bool.Parse(expr.Lexeme);
                }
                
                throw new InterpreterException(expr.Location, "Unknown literal expression type");
            }
            public StatementResult VisitPrintStatement(Pass1.PrintStatement stmt, Object misc) {
                object obj = EvaluateExpression(stmt.Expression, misc);

                if (obj is int || obj is double || obj is bool || obj is char) {
                    Console.Write(obj.ToString());
                    return new StatementResult();
                }

                throw new InterpreterException(stmt.Location, "Printing unknown type");
            }

            public object VisitAssignmentExpression(Pass1.AssignmentExpression expr, object misc) {
                Environment env = (Environment)misc;
                object value = EvaluateExpression(expr.Value, misc);
                env.Set(expr.Name, value, expr.Location);
                return value;
            }
            public StatementResult VisitVarDeclStatement(Pass1.VarDeclStatement stmt, Object misc) {
                string variableName = stmt.Name;
                
                object variableValue = null;
                if (stmt.Initializer != null) {
                    variableValue = EvaluateExpression(stmt.Initializer, misc);
                }
                else {
                    if (stmt.Type == Types.IntID) {
                        variableValue = 0;
                    }
                    else if (stmt.Type == Types.FloatID) {
                        variableValue = 0.0f;
                    }
                    else if (stmt.Type == Types.CharID) {
                        variableValue = '\0';
                    }
                    else if (stmt.Type == Types.BoolID) {
                        variableValue = false;
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
            public StatementResult VisitExpressionStatement(Pass1.ExpressionStatement stmt, Object misc) {
                EvaluateExpression(stmt.Expression, misc);
                return new StatementResult();
            }
        }
    }
}
