using System;
using System.Reflection.Metadata.Ecma335;

namespace CScript {
    public class TypeChecker : ParseTree.Visitor {
        public class Environment {
            public Dictionary<string, string> Map { get; protected set; } // Key: name, Value: type
            public Environment Parent { get; protected set; }
            public Environment(Environment parent) {
                Parent = parent;
                Map = new Dictionary<string, string>();
            }

            public void Create(string type, string name, Location loc) {
                if (Map.ContainsKey(name)) {
                    Compiler.Error("Type Checker", "Duplicate variable name: " + name, loc);
                }

                Map.Add(name, type);
            }

            public string Get(string name) {
                if (Map.ContainsKey(name)) {
                    return Map[name];
                }
                if (Parent != null) {
                    return Parent.Get(name);
                }
                return null;
            }
        }

        protected Environment Variables = null;
        protected TypeDatabase Types = null;
        public Dictionary<ParseTree.Expression, string> ExpressionTypes { get; protected set; }

        protected ParseTree.Declaration.Function CurrentFunction = null;
        protected ParseTree.Statement.Control LastReturn = null;

        public TypeChecker(List<ParseTree.Declaration.File> parseTree, TypeDatabase typeDatabase) {
            ExpressionTypes = new Dictionary<ParseTree.Expression, string>();
            Variables = new Environment(null);
            Types = typeDatabase;

            foreach (string varName in Types.VariableNames) {
                ParseTree.Declaration.Variable var = Types.Variables[varName];
                Variables.Create(var.Type.GetPath(), var.Name.Lexeme, var.Name.Location);
            }

            foreach (string funcName in Types.FunctionNames) {
                ParseTree.Declaration.Function func = Types.Functions[funcName];
                Variables.Create(func.GetDelegateName(), func.Name.Lexeme, func.Name.Location);
            }

            foreach (string enumName in Types.EnumNames) {
                ParseTree.Declaration.Enum enm = Types.Enums[enumName];
                Variables.Create(enm.Name.Lexeme, enm.Name.Lexeme, enm.Name.Location);
            }

            foreach (ParseTree.Declaration.File file in parseTree) {
                if (file.Accept(this) == null) {
                    Compiler.Error("Type Checker", "Type checker failed", new Location(file.Path, 0, 0));
                    break;
                }
            }

            List<string> visited = new List<string>();
            foreach(KeyValuePair<string, ParseTree.Declaration.Function> function in Types.Functions) {
                string delegateName = function.Value.GetDelegateName();
                if (!visited.Contains(delegateName)) {
                    visited.Add(delegateName);
                    if (function.Value.GetDelegate().Accept(this) == null) {
                        Compiler.Error("Type Checker", "Type checker failed on implied delegate", function.Value.Name.Location);
                        break;
                    }
                }
            }
        }
        public void PushVarEnv() {
            Variables = new Environment(Variables);
        }

        public void PopVarEnv() {
            Variables = Variables.Parent;
        }

        public object Visit(ParseTree.Type.Primitive t) {
            return t;
        }
        public object Visit(ParseTree.Type.Array t) {
            if (t.Content.Accept(this) == null) {
                return null;
            }
            return t;
        }
        public object Visit(ParseTree.Type.Map t) {
            if (t.Key.Accept(this) == null) {
                return null;
            }
            if (t.Value.Accept(this) == null) {
                return null;
            }
            return t;
        }
        public object Visit(ParseTree.Declaration.File d) {
            foreach (ParseTree.Declaration decl in d.Content) {
                if (decl.Accept(this) == null) {
                    return null;
                }
            }
            return d;
        }
        public object Visit(ParseTree.Declaration.Variable d) {
            if (d.Type.Accept(this) == null) {
                return null;
            }
            // Registered in Variables outside of this call tree
            if (d.Initializer != null) {
                if (d.Initializer.Accept(this) == null) {
                    return null;
                }
            }
            return d;
        }
        public object Visit(ParseTree.Declaration.Function d) {
            CurrentFunction = d;
            LastReturn = null;
            PushVarEnv();
            if (d.Return.Accept(this) == null) {
                return null;
            }
            foreach (ParseTree.Declaration.Function.Paramater param in d.Paramaters) {
                if (param.Type.Accept(this) == null) {
                    return null;
                }
                Variables.Create(param.Type.GetPath(), param.Name.Lexeme, param.Name.Location);
            }
            if (d.Body != null) {
                if (d.Body.Accept(this) == null) {
                    return null;
                }
            }
            PopVarEnv();
            CurrentFunction = null;

            if (LastReturn == null) { // Nothing was returned
                if (d.Return.GetPath() != "void") {
                    Compiler.Error("Type checker", "Function " + d.Name.Lexeme + " has a return type of " + d.Return.GetPath() + " it must return something", d.Return.GetLocation());
                    return null;
                }
            }
            else { // Something was returned
                if (d.Return.GetPath() == "void") {
                    if (LastReturn.Value != null) {
                        Compiler.Error("Type checker", "Function " + d.Name.Lexeme + " has a returns void, but " + ExpressionTypes[LastReturn.Value] + " was retuerned", d.Return.GetLocation());
                        return null;
                    }
                }
            }

            LastReturn = null;
            return d;
        }
        public object Visit(ParseTree.Declaration.Enum d) {
            return d;
        }
        public object Visit(ParseTree.Declaration.Delegate d) {
            PushVarEnv();
            if (d.Return.Accept(this) == null) {
                return null;
            }
            foreach (ParseTree.Declaration.Function.Paramater param in d.Paramaters) {
                if (param.Type.Accept(this) == null) {
                    return null;
                }
            }
            PopVarEnv();
            return d;
        }

        public object Visit(ParseTree.Declaration.Struct d) {
            PushVarEnv();
            foreach (ParseTree.Statement.Variable v in d.Members) {
                if (v.Accept(this) == null) {
                    return null;
                }
            }
            PopVarEnv();
            return d;
        }
        public object Visit(ParseTree.Statement.Block s) {
            PushVarEnv();
            foreach (ParseTree.Statement stmt in s.Body) {
                if (stmt.Accept(this) == null) {
                    return null;
                }
            }
            PopVarEnv();
            return s;
        }
        public object Visit(ParseTree.Statement.Variable s) {
            if (s.Type.Accept(this) == null) {
                return null;
            }
            Variables.Create(s.Type.GetPath(), s.Name.Lexeme, s.Name.Location);
            if (s.Initializer != null) {
                if (s.Initializer.Accept(this) == null) {
                    return null;
                }
            }

            if (s.Initializer != null) {
                string objectType = s.Type.GetPath();
                string valueType = ExpressionTypes[s.Initializer];
                if (!Types.CanAssign(objectType, valueType)) {
                    Types.CanAssign(objectType, valueType);
                    Compiler.Error("Type Checker", "Variable statement is declared as " + objectType + " but initializer type is " + valueType, s.Name.Location);
                    return null;
                }
            }

            return s;
        }
       
        public object Visit(ParseTree.Statement.Expression s) {
            if (s.Target.Accept(this) == null) {
                return null;
            }
            return s;
        }
        public object Visit(ParseTree.Statement.Control s) {
            if (s.Value != null) {
                if (s.Value.Accept(this) == null) {
                    return null;
                }
            }

            if (s.Keyword.Symbol == Symbol.RETURN) {
                LastReturn = s;
            }

            if (CurrentFunction != null) {
                string _return = CurrentFunction.Return.GetPath();
                if (_return == "void") {
                    if (s.Value != null) {
                        Compiler.Error("Type Checker", "void function can't return anything, use return;", s.Keyword.Location);
                        return null;
                    }
                }
                else {
                    if (s.Value == null) {
                        Compiler.Error("Type Checker", "non void function must return sumeting", s.Keyword.Location);
                        return null;
                    }
                    string return_ = ExpressionTypes[s.Value];
                    if (!Types.CanAssign(_return, return_)) { 
                        Compiler.Error("Type Checker", "function '" + CurrentFunction.Name.Lexeme + "' returns " + _return + " but return expression is " + return_, s.Keyword.Location);
                        return null;
                    }
                }
            }

            return s;
        }
        public object Visit(ParseTree.Statement.If s) {
            ParseTree.Statement.If iter = s;
            while (iter != null) {
                if (iter.Condition != null) {
                    if (iter.Condition.Accept(this) == null) {
                        return null;
                    }
                }
                if (iter.Body != null) {
                    if (iter.Body.Accept(this) == null) {
                        return null;
                    }
                }
                iter = iter.Next;
            }
            return s;
        }
        public object Visit(ParseTree.Statement.While s) {
            if (s.Condition != null) {
                if (s.Condition.Accept(this) == null) {
                    return null;
                }
            }
            if (s.Body != null) {
                if (s.Body.Accept(this) == null) {
                    return null;
                }
            }
            return s;
        }
        public object Visit(ParseTree.Statement.For s) {
            PushVarEnv();
            if (s.Initializers != null) {
                foreach (ParseTree.Statement.Variable v in s.Initializers) {
                    if (v.Accept(this) == null) {
                        return null;
                    }
                }
            }
            if (s.Condition != null) {
                if (s.Condition.Accept(this) == null) {
                    return null;
                }
            }
            if (s.Iterators != null) {
                foreach (ParseTree.Expression e in s.Iterators) {
                    if (e.Accept(this) == null) {
                        return null;
                    }
                }
            }
            if (s.Body != null) {
                if (s.Body.Accept(this) == null) {
                    return null;
                }
            }
            PopVarEnv();
            return s;
        }
        public object Visit(ParseTree.Expression.Group e) {
            if (e.Target.Accept(this) == null) {
                return null;
            }
            ExpressionTypes.Add(e, ExpressionTypes[e.Target]);
            return e;
        }
        public object Visit(ParseTree.Expression.Literal e) {
            Symbol s = e.Value.Symbol;
            if (s == Symbol.LIT_BOOL) {
                ExpressionTypes.Add(e, "bool");
            }
            else if (s == Symbol.LIT_CHAR) {
                ExpressionTypes.Add(e, "char");
            }
            else if (s == Symbol.LIT_NULL) {
                ExpressionTypes.Add(e, "$null");
            }
            else if (s == Symbol.LIT_NUM) {
                ExpressionTypes.Add(e, "num");
            }
            else if (s == Symbol.LIT_STRING) {
                ExpressionTypes.Add(e, "string");
            }
            else {
                Compiler.Error("Type Checker", "Unknown literal", e.Value.Location);
                return null;
            }
            return e;
        }
        public object Visit(ParseTree.Expression.Get e) {
            if (e.Object != null) {
                if (e.Object.Accept(this) == null) {
                    return null;
                }
            }

            if (e.Object == null) { // Variable
                string env = Variables.Get(e.Property.Lexeme);
                if (env == null) {
                    Compiler.Error("Type Checker", "REferencing unknown variable: " + e.Property.Lexeme, e.Property.Location);
                }
                ExpressionTypes.Add(e, env);
            }
            else { // Getter
                string objectType = ExpressionTypes[e.Object];
                string propName = e.Property.Lexeme;

                if (Types.IsStruct(objectType)) {
                    ParseTree.Declaration.Struct st = Types.Structs[objectType];
                    ParseTree.Statement.Variable vr = st.Get(propName);
                    if (vr == null) {
                        Compiler.Error("Type Checker", "Struct " + st.Name.Lexeme + " does not contain property " + propName, e.Property.Location);
                        return null;
                    }
                    ExpressionTypes.Add(e, vr.Type.GetPath());
                }
                else if (Types.IsEnum(objectType)) {
                    ParseTree.Declaration.Enum en = Types.Enums[objectType];
                    if (!(e.Object is ParseTree.Expression.Get)) {
                        Compiler.Error("Type checker", "Can only access enum using it's type, not values", e.Property.Location);
                        return null;
                    }
                    ParseTree.Expression.Get g = (ParseTree.Expression.Get)e.Object;
                    if (g.Object != null) {
                        Compiler.Error("Type checker", "Invalid Enum Getter", e.Property.Location);
                        return null;
                    }
                    if (!Types.IsEnum(g.Property.Lexeme)) {
                        Compiler.Error("Type checker", "Invalid Enum Getter", e.Property.Location);
                        return null;
                    }
                    ExpressionTypes.Add(e, en.Name.Lexeme);
                }
                else {
                    Compiler.Error("Type Checker", "Invalid getter", e.Property.Location);
                    return null;
                }
            }

            return e;
        }
        public object Visit(ParseTree.Expression.Set e) {
            if (e.Object != null) {
                if (e.Object.Accept(this) == null) {
                    return null;
                }
            }
            if (e.Value.Accept(this) == null) {
                return null;
            }

            string targetType = null;
            string valueType = ExpressionTypes[e.Value];

            if (e.Object == null) {  // Setting Variable
                targetType = Variables.Get(e.Property.Lexeme);
            }
            else { // Setting property
                if (!Types.IsStruct(ExpressionTypes[e.Object])) {
                    Compiler.Error("Type checker", "Only structs can use dot setters", e.Operator.Location);
                    return null;
                }
                ParseTree.Declaration.Struct st = Types.Structs[ExpressionTypes[e.Object]];
                ParseTree.Statement.Variable vr = st.Get(e.Property.Lexeme);
                if (vr == null) {
                    Compiler.Error("Type checker", "(setter) struct " + st.Name.Lexeme + " does not contain property " + vr.Name.Lexeme, e.Operator.Location);
                    return null;
                }

                targetType = vr.Type.GetPath();
            }

            Symbol o = e.Operator.Symbol;
            if (o == Symbol.PLUS_EQUAL) {
                if (targetType == "string") {
                    ExpressionTypes.Add(e, "string");
                }
                else if (targetType == "num" && valueType == "num") {
                    ExpressionTypes.Add(e, "num");
                }
            }
            else if (o == Symbol.MINUS_EQUAL || o == Symbol.SLASH_EQUAL ||
                o == Symbol.STAR_EQUAL || o == Symbol.POW_EQUAL ||
                0 == Symbol.MOD_EQUAL) {
                if (targetType == "num" && valueType == "num") {
                    ExpressionTypes.Add(e, "num");
                }
            }
            else if (o == Symbol.EQUAL) {
                if (!Types.CanAssign(targetType, valueType)) {
                    Compiler.Error("Type checker", "can't assign value " + valueType + " to target " + targetType, e.Operator.Location);
                    return null;
                }
                ExpressionTypes.Add(e, targetType);
            }

            if (!ExpressionTypes.ContainsKey(e)) {
                Compiler.Error("type checker", "Operation " + e.Operator.Lexeme + " is invalid with types " + targetType + " and " + valueType, e.Operator.Location);
                return null;
            }

            return e;
        }
        public object Visit(ParseTree.Expression.Unary e) {
            if (e.Object.Accept(this) == null) {
                return null;
            }

            Symbol s = e.Operator.Symbol;
            string objectType = ExpressionTypes[e.Object];

            if (e.Precedence == ParseTree.Expression.Unary.OperatorType.PREFIX) {
                if (s == Symbol.NOT) {
                    if (objectType != "bool") {
                        Compiler.Error("Type checker", "prefix not should be bool", e.Operator.Location);
                    }
                    ExpressionTypes.Add(e, "bool");
                    return e;
                }
                else if (s == Symbol.MINUS || s == Symbol.PLUS_PLUS || s == Symbol.MINUS_MINUS) {
                    if (objectType != "num") {
                        Compiler.Error("Type checker", "prefix " + e.Operator.Lexeme + " should be num", e.Operator.Location);
                    }
                    ExpressionTypes.Add(e, "num");
                    return e;
                }
            }
            else {
                if (s == Symbol.PLUS_PLUS || s == Symbol.MINUS_MINUS) {
                    if (objectType != "num") {
                        Compiler.Error("Type checker", "postfix " + e.Operator.Lexeme + " should be num", e.Operator.Location);
                    }
                    ExpressionTypes.Add(e, "num");
                    return e;
                }
            }
            Compiler.Error("Type checker", "Could not type check unary", e.Operator.Location);
            return null;
        }
        public object Visit(ParseTree.Expression.Binary e) {
            if (e.Left.Accept(this) == null) {
                return null;
            }
            if (e.Right.Accept(this) == null) {
                return null;
            }

            string leftType = ExpressionTypes[e.Left];
            string rightType = ExpressionTypes[e.Right];

            if (e.Operator.Symbol == Symbol.PLUS) {
                if (leftType == "string") {
                    if (rightType == "num" || rightType == "char" || rightType == "bool" || rightType == "string" ||
                        Types.IsEnum(rightType)) {
                        ExpressionTypes.Add(e, "string");
                        return e;
                    }
                    else if (rightType == "object") {
                        if (e.Right is ParseTree.Expression.Call) {
                            ParseTree.Expression.Call call = e.Right as ParseTree.Expression.Call;
                            string arrayFunctionName = call.ArrayFunctionName;
                            if (arrayFunctionName == "at") {
                                ExpressionTypes.Add(e, Types.Indexed(ExpressionTypes[call.Arguments[0]]));
                                return e;
                            }
                        }
                    }
                    else if (rightType == "vec3" || rightType == "vec2" || rightType == "vec4") {
                        ExpressionTypes.Add(e, "string");
                        return e;
                    }
                    Compiler.Error("Type Checker", "Invalid string concat between: " + leftType + " and " + rightType, e.Operator.Location);
                }
                else if (leftType == "vec2" && rightType == "vec2") {
                    ExpressionTypes.Add(e, "vec2");
                    return e;
                }
                else if (leftType == "vec3" && rightType == "vec3") {
                    ExpressionTypes.Add(e, "vec3");
                    return e;
                }
                else if (leftType == "vec4" && rightType == "vec4") {
                    ExpressionTypes.Add(e, "vec4");
                    return e;
                }
            }
            else if (e.Operator.Symbol == Symbol.MINUS) {
                if (leftType == "vec3" && rightType == "vec3") {
                    ExpressionTypes.Add(e, "vec3");
                    return e;
                }
                if (leftType == "vec2" && rightType == "vec2") {
                    ExpressionTypes.Add(e, "vec2");
                    return e;
                }
                if (leftType == "vec4" && rightType == "vec4") {
                    ExpressionTypes.Add(e, "vec4");
                    return e;
                }
            }
            else if (e.Operator.Symbol == Symbol.STAR) {
                if (leftType == "vec3" && rightType == "num") {
                    ExpressionTypes.Add(e, "vec3");
                    return e;
                }
                if (leftType == "vec2" && rightType == "num") {
                    ExpressionTypes.Add(e, "vec2");
                    return e;
                }
                if (leftType == "vec4" && rightType == "num") {
                    ExpressionTypes.Add(e, "vec4");
                    return e;
                }
            }
            else if (e.Operator.Symbol == Symbol.NOT_EQUAL || e.Operator.Symbol == Symbol.EQUAL_EQUAL) {
                if (!Types.CanAssign(rightType, leftType)) { 
                    Compiler.Error("Type Checker", "Can't compare" + leftType + " to " + rightType, e.Operator.Location);
                }

                ExpressionTypes.Add(e, "bool");
                return e;
            }
           
            if (leftType == rightType) {
                if (leftType == "num" && (e.Operator.Symbol == Symbol.LESS || e.Operator.Symbol == Symbol.GREATER ||
                    e.Operator.Symbol == Symbol.LESS_EQUAL || e.Operator.Symbol == Symbol.GREATER_EQUAL ||
                    e.Operator.Symbol == Symbol.EQUAL_EQUAL || e.Operator.Symbol == Symbol.NOT_EQUAL ||
                    e.Operator.Symbol == Symbol.TILDE_EQUAL)) {
                    ExpressionTypes.Add(e, "bool");
                }
                else {
                    ExpressionTypes.Add(e, leftType);
                }
                return e;
            }

            Compiler.Error("Type Checker", "Invalid operator " + e.Operator.Lexeme + " between: " + leftType + " and " + rightType, e.Operator.Location);
            return null;
        }
        public object Visit(ParseTree.Expression.Call e) {
            if (e.Object.Accept(this) == null) {
                return null;
            }
            foreach (ParseTree.Expression arg in e.Arguments) {
                if (arg.Accept(this) == null) {
                    return null;
                }
            }
            string objectType = ExpressionTypes[e.Object];

            if (e.CallSite.Symbol == Symbol.LBRACKET) {
                if (Types.IsArray(objectType)) {
                    if (e.Arguments.Count != 1) {
                        Compiler.Error("Type Checker", "Array must be indexed with only one argument", e.CallSite.Location);
                        return null;
                    }
                    string argType = ExpressionTypes[e.Arguments[0]];
                    if (argType != "num") {
                        Compiler.Error("Type Checker", "Array can only be indexed by numbers, not " + argType, e.CallSite.Location);
                        return null;
                    }
                }
                else if (Types.IsMap(objectType)) {
                    if (e.Arguments.Count != 1) {
                        Compiler.Error("Type Checker", "Map must be indexed with only one argument", e.CallSite.Location);
                        return null;
                    }
                    string keyType = Types.GetMap(objectType).Key.GetPath();
                    string argType = ExpressionTypes[e.Arguments[0]];
                    if (!Types.CanAssign(keyType, argType)) {
                        Compiler.Error("Type Checker", "Map can only be indexed by " + keyType + ", not " + argType, e.CallSite.Location);
                        return null;
                    }
                }
                else if (objectType == "string") {
                    if (e.Arguments.Count != 1) {
                        Compiler.Error("Type Checker", "String must be indexed with only one argument", e.CallSite.Location);
                        return null;
                    }
                    string argType = ExpressionTypes[e.Arguments[0]];
                    if (argType != "num") {
                        Compiler.Error("Type Checker", "String can only be indexed by numbers, not " + argType, e.CallSite.Location);
                        return null;
                    }
                }
                else {
                    Compiler.Error("Type checker", "Unknown type being indexed: " + objectType, e.CallSite.Location);
                }

                ExpressionTypes.Add(e, Types.Indexed(objectType));
                return e;
            }

            if (!Types.IsDelegate(objectType)) {
                Compiler.Error("Type checker", "Trying to call invalid type", e.CallSite.Location);
            }
            ParseTree.Declaration.Delegate del = Types.Delegates[objectType];
            string patchedUpReturnType = del.Return.GetPath();

            string mapFunctionName = e.MapFunctionName;
            string arrayFunctionName = e.ArrayFunctionName;
            if (mapFunctionName != null) { // Extra type check arguments
                if (e.Arguments.Count < 1) {
                    Compiler.Error("type checker", "map functions require at least one argument", e.CallSite.Location);
                }

                ParseTree.Expression arg0 = e.Arguments[0];
                string arg0Type = ExpressionTypes[arg0];
                if (!Types.IsMap(arg0Type)) {
                    Compiler.Error("type checker", "first argument of a map function must be a map, not: " + arg0Type, e.CallSite.Location);
                }

                ParseTree.Type.Map map = Types.GetMap(arg0Type);
                string keyType = map.Key.GetPath();
                string valType = map.Value.GetPath();

                if (mapFunctionName == "clear" || mapFunctionName == "keys" || mapFunctionName == "values") {
                    if (e.Arguments.Count != 1) {
                        Compiler.Error("type checker", "map.clear, map.keys, map.values can only have 1 argument", e.CallSite.Location);
                    }

                    if (mapFunctionName == "keys") {
                        patchedUpReturnType = keyType + "[]";
                    }
                    else if (mapFunctionName == "values") {
                        patchedUpReturnType = valType;
                    }
                }
                else if (mapFunctionName == "remove" || mapFunctionName == "get" || mapFunctionName == "has") {
                    if (e.Arguments.Count != 2) {
                        Compiler.Error("type delete", "map.delete, map.get, map.has must have 2 arguments", e.CallSite.Location);
                    }
                    ParseTree.Expression arg1 = e.Arguments[1];
                    string arg1Type = ExpressionTypes[arg1];

                    if (!Types.CanAssign(keyType, arg1Type)) {
                        Compiler.Error("type delete", mapFunctionName + " expected key type: " + keyType + ", got: " + arg1Type, e.CallSite.Location);
                    }

                    if (mapFunctionName == "remove" || mapFunctionName == "get") {
                        patchedUpReturnType = valType;
                    }
                    else if (mapFunctionName == "has") {
                        patchedUpReturnType = "bool";
                    }
                }
                else if (mapFunctionName == "set") {
                    if (e.Arguments.Count != 3) {
                        Compiler.Error("type checker", "map.set must have 3 arguments", e.CallSite.Location);
                    }
                    ParseTree.Expression arg1 = e.Arguments[1];
                    string arg1Type = ExpressionTypes[arg1];
                    ParseTree.Expression arg2 = e.Arguments[2];
                    string arg2Type = ExpressionTypes[arg2];

                    if (!Types.CanAssign(keyType, arg1Type)) {
                        Compiler.Error("type delete", "map.delete, map.get, map.has, expected key type: " + keyType + ", got: " + arg1Type, e.CallSite.Location);
                    }

                    if (!Types.CanAssign(valType, arg2Type)) {
                        Compiler.Error("type delete", "map.set, expected key type: " 
                            + keyType + ", got: " + arg1Type + " expected val type: " +
                            valType + ", got: " + arg2Type, e.CallSite.Location);
                    }
                }
                else {
                    Compiler.Error("Type Checker", "Unknown map function", e.CallSite.Location);
                }
            }
            else if (arrayFunctionName != null) {
                if (e.Arguments.Count < 1) {
                    Compiler.Error("type checker", "array functions require at least one argument", e.CallSite.Location);
                }

                ParseTree.Expression arg0 = e.Arguments[0];
                string arg0Type = ExpressionTypes[arg0];
                if (!Types.IsArray(arg0Type)) {
                    Compiler.Error("type checker", "first argument of an array function must be an array, not: " + arg0Type, e.CallSite.Location);
                }
                ParseTree.Type.Array array = Types.GetArray(arg0Type);
                string indexedType = Types.Indexed(arg0Type);

                if (arrayFunctionName == "length" || arrayFunctionName == "pop" || arrayFunctionName == "clear" || 
                    arrayFunctionName == "reverse" || arrayFunctionName == "shift" || arrayFunctionName == "copy") {
                    if (e.Arguments.Count != 1) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can only have 1 argument", e.CallSite.Location);
                    }

                    if (arrayFunctionName == "pop" || arrayFunctionName == "shift") {
                        patchedUpReturnType = indexedType;
                    }
                    else if (arrayFunctionName == "copy") {
                        patchedUpReturnType = arg0Type;
                    }
                }
                else if (arrayFunctionName == "first" || arrayFunctionName == "last" || arrayFunctionName == "add") {
                    if (e.Arguments.Count != 2) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can only have 2 argument", e.CallSite.Location);
                    }

                    ParseTree.Expression arg1 = e.Arguments[1];
                    string arg1Type = ExpressionTypes[arg1];
                    if (!Types.CanAssign(indexedType, arg1Type)) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can't assign " + arg1Type + " to " + indexedType, e.CallSite.Location);
                    }
                }
                else if (arrayFunctionName == "insert") {
                    if (e.Arguments.Count != 3) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can only have 3 argument", e.CallSite.Location);
                    }

                    ParseTree.Expression arg2 = e.Arguments[2];
                    string arg2Type = ExpressionTypes[arg2];
                    if (!Types.CanAssign(indexedType, arg2Type)) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can't assign " + arg2Type + " to " + indexedType, e.CallSite.Location);
                    }

                    ParseTree.Expression arg1 = e.Arguments[1];
                    string arg1Type = ExpressionTypes[arg1];
                    if (arg1Type != "num") {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " second argument must be num", e.CallSite.Location);
                    }
                }
                else if (arrayFunctionName == "at") {
                    if (e.Arguments.Count != 2) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can only have 2 argument", e.CallSite.Location);
                    }

                    ParseTree.Expression arg1 = e.Arguments[1];
                    string arg1Type = ExpressionTypes[arg1];
                    if (!Types.CanAssign("num", arg1Type)) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can't assign " + arg1Type + " to num", e.CallSite.Location);
                    }

                    if (arrayFunctionName == "at") {
                        patchedUpReturnType = indexedType;
                    }
                }
                else if (arrayFunctionName == "join") {
                    if (e.Arguments.Count != 2) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can only have 2 argument", e.CallSite.Location);
                    }

                    ParseTree.Expression arg1 = e.Arguments[1];
                    string arg1Type = ExpressionTypes[arg1];
                    if (!Types.CanAssign("string", arg1Type)) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can't assign " + arg1Type + " to string", e.CallSite.Location);
                    }

                    patchedUpReturnType = "string";
                }
                else if (arrayFunctionName == "concat") {
                    if (e.Arguments.Count != 2) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can only have 2 argument", e.CallSite.Location);
                    }

                    ParseTree.Expression arg1 = e.Arguments[1];
                    string arg1Type = ExpressionTypes[arg1];
                    if (arg0Type != arg1Type) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " type " + arg1Type + " != " + arg0Type, e.CallSite.Location);
                    }

                    if (arrayFunctionName == "concat") {
                        patchedUpReturnType = arg0Type;
                    }
                }
                else if (arrayFunctionName == "slice" || arrayFunctionName == "delete") {
                    if (e.Arguments.Count != 3) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can only have 3 argument", e.CallSite.Location);
                    }

                    ParseTree.Expression arg = e.Arguments[1];
                    string argType = ExpressionTypes[arg];
                    if (!Types.CanAssign("num", argType)) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can't assign " + argType + " to num", e.CallSite.Location);
                    }

                    arg = e.Arguments[2];
                    argType = ExpressionTypes[arg];
                    if (!Types.CanAssign("num", argType)) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can't assign " + argType + " to num", e.CallSite.Location);
                    }

                    if (arrayFunctionName == "slice") {
                        patchedUpReturnType = arg0Type;
                    }
                }
                else if (arrayFunctionName == "sort") {
                    if (e.Arguments.Count != 2) {
                        Compiler.Error("type checker", "array." + arrayFunctionName + " can only have 3 argument", e.CallSite.Location);
                    }

                    ParseTree.Expression arg = e.Arguments[1];
                    string argType = ExpressionTypes[arg];
                    if (!Types.IsDelegate(argType)) {
                        if (argType != "$null") {
                            Compiler.Error("type checker", "array." + arrayFunctionName + " second argument must be a delegate", e.CallSite.Location);
                        }
                    }
                }
            }


            if (del.Paramaters.Count != e.Arguments.Count) {
                Compiler.Error("Type Checker", "Calling delegate " + del.Name.Lexeme + " with the wrong arity", e.CallSite.Location);
                return null;
            }

            List<string> overrides = null;
            if (arrayFunctionName == "sort") {
                overrides = new List<string>();
                overrides.Add("string");
                overrides.Add("string");
            }

            for (int i = 0, size = del.Paramaters.Count; i < size; ++i) {
                ParseTree.Declaration.Function.Paramater param = del.Paramaters[i];
                ParseTree.Expression arg = e.Arguments[i];

                string paramType = param.Type.GetPath();
                string argType = ExpressionTypes[arg];
                
                if (!Types.CanAssign(paramType, argType, overrides)) { 
                    Compiler.Error("Type Checker", "Argument " + i + " expects " + paramType + " but got " + argType, e.CallSite.Location);
                    return null;
                }
            }

            ExpressionTypes.Add(e, patchedUpReturnType);
            return e;
        }
        public object Visit(ParseTree.Expression.Cast e) {
            if (e.Object != null) {
                if (e.Object.Accept(this) == null) {
                    return null;
                }
            }
            if (e.Target != null) {
                if (e.Target.Accept(this) == null) {
                    return null;
                }
            }
            ExpressionTypes.Add(e, e.Target.GetPath());
            return e;
        }
        public object Visit(ParseTree.Expression.New e) {
            if (e.Target.Accept(this) == null) {
                return null;
            }
            foreach (ParseTree.Expression arg in e.Arguments) {
                if (arg.Accept(this) == null) {
                    return null;
                }
            }

            ParseTree.Type newType = e.Target;
            string newTypeName = newType.GetPath();
            ExpressionTypes.Add(e, newTypeName);

            if (Types.IsArray(newTypeName)) {
                string indexed = Types.Indexed(newTypeName);

                foreach (ParseTree.Expression arg in e.Arguments) {
                    string argType = ExpressionTypes[arg];
                    if (!Types.CanAssign(indexed, argType) ) { 
                        Compiler.Error("Type Checker", 
                            "array " + newTypeName + " constructor can only accept arguments that are " + 
                            indexed + ", but was provided " + argType, e.Keyword.Location
                        );
                        return null;
                    }
                }

                return e;
            }
            else if (Types.IsMap(newTypeName)) {
                string valueType = Types.Indexed(newTypeName);
                string keyType = Types.GetMap(newTypeName).Key.GetPath();

                for (int i = 0, size = e.Arguments.Count; i < size; ++i) {
                    string argType = ExpressionTypes[e.Arguments[i]];
                    string compType = keyType;
                    if (i % 2 != 0) {
                        compType = valueType;
                    }

                    if (argType != compType) {
                        int j = i;
                        if (i % 2 == 0) {
                            j = i + 1;
                        }
                        string error = "map " + newTypeName + " constructor expectes arguments in (" +
                            keyType + ", " + valueType + ") order but was provided (" +
                            ExpressionTypes[e.Arguments[j - 1]];
                        if (j  < size) {
                            error += ", " + ExpressionTypes[e.Arguments[j]] + ")";
                        }
                        else {
                            error += ", <end of list>)";
                        }
                        Compiler.Error("Type Checker", error, e.Keyword.Location);
                        return null;
                    }
                }

                return e;
            }
            else if (Types.IsStruct(newTypeName)) {
                List<string> structTypes = new List<string>();
                List<string> argTypes = new List<string>();

                foreach(ParseTree.Statement.Variable member in Types.GetStruct(newTypeName).Members) {
                    structTypes.Add(member.Type.GetPath());
                }

                for (int i = 0, size = e.Arguments.Count; i < size; ++i) {
                    argTypes.Add(ExpressionTypes[e.Arguments[i]]);
                }

                if (argTypes.Count > structTypes.Count) {
                    Compiler.Error("Type checker", "Too many arguments provided. " + newTypeName + " has " + structTypes.Count + " members, but new was given " + e.Arguments.Count + " arguments", e.Keyword.Location);
                    return null;
                }

                for (int i = 0, sise = argTypes.Count; i < sise; ++i) {
                    if (!Types.CanAssign(structTypes[i], argTypes[i])) {
                        Compiler.Error("Type checker", "New was given: " + argTypes[i] + " but struct expected: " + structTypes[i], e.Keyword.Location);
                        return null;
                    }
                }

                return e;
            }
            else if (Types.IsEnum(newTypeName)) {
                Compiler.Error("Type Checker", "Can't call new on enum", e.Keyword.Location);
                return null;
            }
            else if (Types.IsDelegate(newTypeName)) {
                Compiler.Error("Type Checker", "Can't call new on delegate", e.Keyword.Location);
                return null;
            }

            Compiler.Error("Type checker", "Could not invoke new", e.Keyword.Location);
            return null;
        }
    }
}