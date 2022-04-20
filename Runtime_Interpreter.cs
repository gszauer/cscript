namespace CScript {
    namespace Runtime {
        enum StatementResultType {
            NORMAL,
            RETURN
        }
        class StatementResult {
            public StatementResultType Type { get; protected set; }
            public object Return { get; protected set; }

            public StatementResult(StatementResultType type, Object returnValue) {
                Type = type;
                Return = returnValue;
            }

            public StatementResult() {
                Type = StatementResultType.NORMAL;
            }
        }
        class Interpreter : Pass1.ExpressionVisitor<object, Environment>, Pass1.StatementVisitor<StatementResult, Environment> {
            public delegate void CustomPrint(string message);
            public Environment Global { get; protected set; }
            protected TypeTable Types;
            protected CustomPrint PrintHandler;
            public Interpreter(AbstractSyntaxTree ast) {
                Global = new Environment(null);
                Types = ast.Types;
                PrintHandler = null;

                foreach (Pass1.Statement s in ast.Program) {
                    ExecuteStatement(s, Global);
                }
            }

            public void UseCustomPrint(CustomPrint printHandler) {
                PrintHandler = printHandler;
            }

            public void RunFunction(string name) {
                object function = Global.Get(name, new Location(-1, "runtime"));
                if (!(function is Function)) {
                    throw new InterpreterException(new Location(-1, "runtime"), "Trying to manually call a non function: " + name);
                }
                Function fun = (Function)function;
                fun.Call(this, Global, new List<object>());
            }

            public object EvaluateExpression(Pass1.Expression e, Environment o) {
                return e.Accept(this, o);
            }
            public StatementResult ExecuteStatement(Pass1.Statement s, Environment o) {
                return s.Accept(this, o);
            }

            public object VisitBinaryExpression(Pass1.BinaryExpression expr, Environment misc) {
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
            public object VisitUnaryExpression(Pass1.UnaryExpression expr, Environment misc) {
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
            public object VisitVariableExpression(Pass1.VariableExpression expr, Environment env) {
                return env.Get(expr.Name, expr.Location);
            }
            public object VisitLiteralExpression(Pass1.LiteralExpression expr, Environment misc) {
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

            public object VisitCallExpression(Pass1.CallExpression expr, Environment env) {
                object callee = EvaluateExpression(expr.Calee, env);

                if (!(callee is Function)) {
                    throw new InterpreterException(expr.Location, "Trying to call non function");
                }
                Function function = (Function)callee;

                List<object> args = new List<object>();
                foreach(Pass1.Expression arg in expr.Arguments) {
                    args.Add(EvaluateExpression(arg, env));
                }

                return function.Call(this, env, args);
            }

            public StatementResult VisitReturnStatement(Pass1.ReturnStatement stmt, Environment env) {
                object val = null;
                if (stmt.ReturnValue != null) {
                    val = EvaluateExpression(stmt.ReturnValue, env);
                }

                return new StatementResult(StatementResultType.RETURN, val);
            }
            public StatementResult VisitPrintStatement(Pass1.PrintStatement stmt, Environment misc) {
                object obj = EvaluateExpression(stmt.Expression, misc);

                if (stmt.Expression == null) { // print();
                    return new StatementResult();
                }

                if (obj is int || obj is double || obj is bool || obj is char) {
                    if (PrintHandler != null) {
                        PrintHandler(obj.ToString());
                    }
                    else { 
                        Console.Write(obj.ToString());
                    }
                    return new StatementResult();
                }

                throw new InterpreterException(stmt.Location, "Printing unknown type");
            }

            public object VisitAssignmentExpression(Pass1.AssignmentExpression expr, Environment misc) {
                Environment env = (Environment)misc;
                object value = EvaluateExpression(expr.Value, misc);
                env.Set(expr.Name, value, expr.Location);
                return value;
            }

            public StatementResult VisitFunDeclStatement(Pass1.FunDeclStatement stmt, Environment env) {
                Function newFucntion = new UserFunction(stmt);
                if (env != Global) {
                    throw new InterpreterException(stmt.Location, "Can't declare function in non global scope");
                }
                env.Declare(stmt.Name, stmt.Location);
                env.Set(stmt.Name, newFucntion, stmt.Location);

                return new StatementResult();
            }

            public StatementResult VisitBlockStatement(Pass1.BlockStatement stmt, Environment env) {
                Environment block = new Environment(env);
                foreach(Pass1.Statement e in stmt.Body) {
                    StatementResult result = ExecuteStatement(e, block);
                    if (result.Type == StatementResultType.RETURN) {
                        return new StatementResult(StatementResultType.RETURN, result.Return);
                    }
                    if (result.Type != StatementResultType.NORMAL) {
                        throw new NotImplementedException();
                    }
                }

                return new StatementResult();
            }
            public StatementResult VisitVarDeclStatement(Pass1.VarDeclStatement stmt, Environment misc) {
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
            public StatementResult VisitExpressionStatement(Pass1.ExpressionStatement stmt, Environment misc) {
                EvaluateExpression(stmt.Expression, misc);
                return new StatementResult();
            }
        }
    }
}
