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

            public object VisitTypeExpression(Pass1.TypeExpression expr, Environment env) {
                object result = EvaluateExpression(expr.Expression, env);

                if (expr.Dynamic) {
                    TypeId dynamicType = GetRuntimeType(result);
                    return dynamicType;
                }
                TypeId staticType = expr.Expression.Type;
                return staticType;

                throw new InterpreterException(expr.Location, "Getting unknown type literal");
            }

            public object VisitIsExpression(Pass1.IsExpression expr, Environment env) {
                object result = EvaluateExpression(expr.TestObject, env);

                // 7 as object
                // Vec3 as object
                if (expr.TestType == Types.ObjectID) {
                    return true; // EVERYTHING IS AN OBJECT
                }

                TypeId resultType = GetRuntimeType(result);

                // 7 as int
                if (resultType == expr.TestType) {
                    return true;
                }

                // obj as Vec3
                return false;
            }

            public object VisitAsExpression(Pass1.AsExpression expr, Environment env) {
                object result = EvaluateExpression(expr.Castee, env);
                TypeId resultType = GetRuntimeType(result);

                if (resultType == expr.Type) { // Obj a = Vec3(); Vec3 b = a as Vec3;
                    return result;
                }

                if (expr.Type == Types.ObjectID) { // Anything can be cast to an object
                    return result;
                }

                if (resultType == Types.ObjectID && result == null) {
                    return GetRuntimeValue(expr.Type); // Null
                }

                if (resultType == Types.IntID || resultType == Types.FloatID || resultType == Types.CharID || resultType == Types.BoolID) { // Casting a primiitive
                    if (expr.Type == Types.IntID || expr.Type == Types.FloatID || expr.Type == Types.CharID || expr.Type == Types.BoolID) {
                        return Cast(result, resultType, expr.Type);
                    }
                    return GetRuntimeValue(expr.Type);
                }
                
                else if (Types.IsStruct(resultType)) {
                    // Casting to a specific struct, but the dynamic struct type didn't match
                    // So we're casting incompatible objects. This will return null
                    return GetRuntimeValue(expr.Type); // casting to a specific struct
                }

                throw new InterpreterException(expr.Location, "Casting from " + resultType.DebugName + " to invalid type: " + expr.Type.DebugName);
            }

            public object Cast(object target, TypeId from, TypeId to) {
                if (from == Types.IntID) {
                    int i = (int)target;
                    if (to == Types.IntID) {
                        return (int)i;
                    }
                    else if (to == Types.FloatID) {
                        return (double)i;
                    }
                    else if (to == Types.CharID) {
                        return (char)i;
                    }
                    else if (to == Types.BoolID) {
                        if (i == 0) {
                            return false;
                        }
                        return true;
                    }
                }
                else if (from == Types.FloatID) {
                    double f = (double)target;
                    if (to == Types.IntID) {
                        return (int)f;
                    }
                    else if (to == Types.FloatID) {
                        return (double)f;
                    }
                    else if (to == Types.CharID) {
                        return (char)f;
                    }
                    else if (to == Types.BoolID) {
                        if (f == 0.0) {
                            return false;
                        }
                        return true;
                    }
                }
                else if (from == Types.CharID) {
                    char c = (char)target;
                    if (to == Types.IntID) {
                        return (int)c;
                    }
                    else if (to == Types.FloatID) {
                        return (double)c;
                    }
                    else if (to == Types.CharID) {
                        return (char)c;
                    }
                    else if (to == Types.BoolID) {
                        if (c == 0) {
                            return false;
                        }
                        return true;
                    }
                }
                else if (from == Types.BoolID) {
                    bool b = (bool)target;

                    if (to == Types.IntID) {
                        if (b) {
                            return 1;
                        }
                        return 0;
                    }
                    else if (to == Types.FloatID) {
                        if (b) {
                            return 1.0;
                        }
                        return 0.0;
                    }
                    else if (to == Types.CharID) {
                        if (b) {
                            return 't';
                        }
                        return '\0';
                    }
                    else if (to == Types.BoolID) {
                        return b;
                    }
                }

                throw new NotImplementedException();
            }

            public TypeId GetRuntimeType(object value) {
                if (value == null) {
                    return Types.ObjectID;
                }
                if (value is int) {
                    return Types.IntID;
                }
                if (value is double) {
                    return Types.FloatID;
                }
                if (value is char) {
                    return Types.CharID;
                }
                if (value is bool) {
                    return Types.BoolID;
                }
                if (value is StructureDeclaration) {
                    StructureDeclaration structureDeclaration = (StructureDeclaration)value;
                    return structureDeclaration.Type;
                }
                if (value is StructureInstance) {
                    StructureInstance structureInstance = (StructureInstance)value;
                    return structureInstance.Type;
                }
                throw new NotImplementedException();
            }
            public object GetRuntimeValue(TypeId type, string optLexeme = null) {
                if (Types.IsStruct(type)) {
                    if (optLexeme != null) {
                        if (optLexeme != "null") {
                            throw new NotImplementedException();
                        }
                    }
                    return null;
                }
                else if (type == Types.IntID) {
                    if (optLexeme != null) {
                        return Int32.Parse(optLexeme);
                    }
                    return 0;
                }
                else if (type == Types.FloatID) {
                    if (optLexeme != null) {
                        return Double.Parse(optLexeme);
                    }
                    return 0.0;
                }
                else if (type == Types.CharID) {
                    if (optLexeme != null) {
                        if (optLexeme == "'\\n'") {
                            return '\n';
                        }
                        if (optLexeme == "'\\t'") {
                            return '\t';
                        }
                        if (optLexeme == "'\\r'") {
                            return '\r';
                        }
                        if (optLexeme == "'\\0'") {
                            return '\0';
                        }
                        if (optLexeme.Length == 3) {
                            //string debug = char.Parse(optLexeme.Substring(1, 1)).ToString();
                            return char.Parse(optLexeme.Substring(1, 1));
                        }
                        throw new NotImplementedException();
                    }
                    return '\0';
                }
                else if (type == Types.BoolID) {
                    if (optLexeme != null) {
                        return bool.Parse(optLexeme);
                    }
                    return false;
                }
                else if (type == Types.TypeID) {
                    if (optLexeme != null) {
                        if (optLexeme == "int") {
                            return new TypeId(Types.IntID.Index, Types.IntID.DebugName);
                        }
                        if (optLexeme == "float") {
                            return new TypeId(Types.FloatID.Index, Types.FloatID.DebugName);
                        }
                        if (optLexeme == "bool") {
                            return new TypeId(Types.BoolID.Index, Types.BoolID.DebugName);
                        }
                        if (optLexeme == "char") {
                            return new TypeId(Types.CharID.Index, Types.CharID.DebugName);
                        }
                        if (optLexeme == "type") {
                            return new TypeId(Types.TypeID.Index, Types.TypeID.DebugName);
                        }
                        throw new NotImplementedException();
                    }
                    return new TypeId(Types.TypeID.Index, Types.TypeID.DebugName);
                }
                else if (type == Types.ObjectID) {
                    if (optLexeme != null) {
                        throw new NotImplementedException();
                    }
                    return null;
                }
                else if (type == Types.NullID) {
                    if (optLexeme != null && optLexeme != "null") {
                        throw new NotImplementedException();
                    }
                    return null;
                }
                throw new NotImplementedException();
            }

            public object VisitLiteralExpression(Pass1.LiteralExpression expr, Environment misc) {
                return GetRuntimeValue(expr.Type, expr.Lexeme);
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

                string print = null;
                if (obj is int || obj is double || obj is bool || obj is char) {
                    print = obj.ToString();
                }
                if (obj is TypeId) {
                    TypeId typeId = (TypeId)obj;
                    print = "<type " + typeId.Index + " " + typeId.DebugName + ">";
                }
                if (obj == null) {
                    print = "null";
                }
                if (obj is StructureInstance) {
                    StructureInstance str = (StructureInstance)obj;
                    print = "<struct " + str.Type.Index + " " + str.Name + ": \n";
                    for (int i = 0; i < str.VariableNames.Count; ++i) {
                        print += "\t<" + str.VariableTypes[i].Index + " " + str.VariableTypes[i].DebugName + " " + str.VariableNames[i] + ": ";
                        string varName = str.VariableNames[i];
                        if (str.VariableValues[varName] == null) {
                            print += "null";
                        }
                        else {
                            print += str.VariableValues[varName].ToString();
                        }
                        print += ">\n";
                    }
                    print += ">";
                }

                if (print != null) {
                    if (PrintHandler != null) {
                        PrintHandler(print);
                    }
                    else {
                        Console.Write(print);
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

            public StatementResult VisitStructDeclStatement(Pass1.StructDeclStatement stmt, Environment env) {
                StructureDeclaration newStruct = new StructureDeclaration(stmt);
                if (env != Global) {
                    throw new InterpreterException(stmt.Location, "Can't declare struct in non global scope");
                }
                env.Declare(stmt.Name, stmt.Location);
                env.Set(stmt.Name, newStruct, stmt.Location);

                return new StatementResult();
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

            public object VisitSetExpression(Pass1.SetExpression expr, Environment misc) {
                object callee = EvaluateExpression(expr.Callee, misc);

                if (!(callee is StructureInstance)) {
                    throw new InterpreterException(expr.Location, "Trying to set a non struct");
                }
                StructureInstance inst = (StructureInstance)callee;

                object value = EvaluateExpression(expr.Value, misc);
                inst.VariableValues[expr.Name] = value;
                return value;
            }

            public object VisitGetExpression(Pass1.GetExpression expr, Environment env) {
                object callee = EvaluateExpression(expr.Callee, env);

                if (!(callee is StructureInstance)) {
                    throw new InterpreterException(expr.Location, "Get expression can only be done on structures");
                }

                StructureInstance inst = (StructureInstance)callee;
                if (!inst.VariableNames.Contains(expr.Name)) {
                    throw new InterpreterException(expr.Location, "Struct " + inst.Name + " does not contain " + expr.Name);
                }

                return inst.VariableValues[expr.Name];
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
                    variableValue = GetRuntimeValue(stmt.Type);
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
