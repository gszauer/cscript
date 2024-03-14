using System.Linq.Expressions;

namespace CScript {
    internal class Javascript : ParseTree.Visitor {
        protected string js = null;
        protected TypeDatabase Types = null;
        protected Dictionary<ParseTree.Expression, string> ExpressionTypes = null;
        protected int Indent = 0;
        public string Result { get { return js; } }
        public Javascript(List<ParseTree.Declaration.File> parseTree, TypeDatabase types, TypeChecker checker) {
            ExpressionTypes = checker.ExpressionTypes;
            js = "";
            Types = types;
            foreach (ParseTree.Declaration.File file in parseTree) {
                file.Accept(this);
            }
        }
        protected void ApplyIndent() {
            for (int j = 0; j < Indent; ++j) {
                js += "\t";
            }
        }
        public object Visit(ParseTree.Type.Primitive t) { // Done
            // Nothing to do
            return t;
        }
        public object Visit(ParseTree.Type.Array t) { // Done
            // Nothing to do
            return t;
        }
        public object Visit(ParseTree.Type.Map t) { // Done
            // Nothing to do
            return t;
        }
        public object Visit(ParseTree.Declaration.File d) { // Done
            for (int i = 0, size = d.Content.Count; i < size; ++i) {
                d.Content[i].Accept(this);
            }

            return d;
        }
        public object Visit(ParseTree.Declaration.Variable d) { // Done
            string dName = d.Name.Lexeme;
            if (dName == "array" || dName == "map" || dName == "string" || dName == "print" || dName == "Math" || dName == "math") {
                return d;
            }

            js += "var " + d.Name.Lexeme + " = ";

            if (d.Initializer != null) {
                d.Initializer.Accept(this);
            }
            else {
                string ty = d.Type.GetPath();
                if (Types.IsNullable(ty) || ty == "$null") {
                    js += "null";
                }
                else {
                    if (ty == "bool") {
                        js += "false";
                    }
                    else if (ty == "num") {
                        js += "0";
                    }
                    else if (ty == "char") {
                        js += "'\0'";
                    }
                    else {
                        Compiler.Error("Javascript Generater", "Confused about declaratioin.variable type: " + ty, d.Name.Location);
                    }
                }
            }

            js += ";\n\n";

            return d;
        }
        public object Visit(ParseTree.Declaration.Function d) {  // Done
            string dName = d.Name.Lexeme;
            

            js += "function " + d.Name.Lexeme + "(";

            for (int i = 0, size = d.Paramaters.Count; i < size; ++i) {
                d.Paramaters[i].Type.Accept(this);
                js += d.Paramaters[i].Name.Lexeme;
                if (i < size - 1) {
                    js += ", ";
                }
            }
            js += ")";
            if (d.Body != null) {
                js += " ";
                d.Body.Accept(this);
            }
            else {
                Compiler.Error("Javascript Generater", "Function " + d.Name.Lexeme + " must have a body", d.Name.Location);
            }

            js += "\n";
            return d;
        }
        public object Visit(ParseTree.Declaration.Enum d) {  // Done
            js += "const " + d.Name.Lexeme + " = {\n";
            for (int i = 0, size = d.Members.Count; i < size; ++i) { 
                js += "\t" + d.Members[i].Name.Lexeme + ": " + d.Members[i].Value.ToString();
                if (i < size - 1) {
                    js += ",";
                }
                js += "\n";
            }
            js += "}\n";
            
            js += "\n";
            return d;
        }
        public object Visit(ParseTree.Declaration.Delegate d) {
            // Nothing to do
            return d;
        }
        public object Visit(ParseTree.Declaration.Struct d) {
            string dName = d.Name.Lexeme;
            if (dName == "_array" || dName == "_map" || dName == "_string" || dName == "_math") {
                return d;
            }

            js += "class " + d.Name.Lexeme + " {\n";

            js += "\tconstructor(";
            for (int i = 0, size = d.Members.Count; i < size; ++i) {
                string nam = d.Members[i].Name.Lexeme;
                if (nam == "delete") {
                    nam = "_delete__";
                }
                js += nam;
                if (i < size - 1) {
                    js += ", ";
                }
            }
            js += ") {\n";

            for (int i = 0, size = d.Members.Count; i < size; ++i) {
                string nam = d.Members[i].Name.Lexeme;
                if (nam == "delete") {
                    nam = "_delete__";
                }
                js += "\t\tthis." + d.Members[i].Name.Lexeme + " = ";

                js += "(typeof " + nam + " === \"undefined\")? ";
                if (d.Members[i].Initializer != null) {
                    d.Members[i].Initializer.Accept(this);
                }
                else {
                    js += Types.GetDefualtValueAsString(d.Members[i].Type.GetPath());
                }
                js += " : " + nam + ";\n";
            }

            js += "\t}\n"; // End constructor

            js += "}\n";

            js += "\n";
            return d;
        }
        public object Visit(ParseTree.Statement.Block s) {
            js += "{\n";
            Indent++;
            for (int i = 0, size = s.Body.Count; i < size; ++i) {
                ApplyIndent();
                s.Body[i].Accept(this);
            }
            Indent--;
            ApplyIndent();
            js += "}\n";

            return s;
        }
        public object Visit(ParseTree.Statement.Variable s) {
            js += "let " + s.Name.Lexeme + " = ";

            if (s.Initializer != null) {
                s.Initializer.Accept(this);
            }
            else {
                js += Types.GetDefualtValueAsString(s.Type.GetPath());
            }
            js += ";\n";

            return s;
        }
        public object Visit(ParseTree.Statement.Expression s) {
            s.Target.Accept(this);
            js += ";\n";
            return s;
        }
        public object Visit(ParseTree.Statement.Control s) {
            js += s.Keyword.Lexeme;

            if (s.Value != null) {
                js += " ";
                s.Value.Accept(this);
            }
            js += ";\n";

            return s;
        }
        public object Visit(ParseTree.Statement.If s) {
            ParseTree.Statement.If iter = s;
            while (iter != null) {
                if (iter.Condition != null) {
                    js += "if (";
                    iter.Condition.Accept(this);
                    js += ") ";
                }
                if (iter.Body != null) {
                    iter.Body.Accept(this);
                }

                iter = iter.Next;
                if (iter != null) {
                    ApplyIndent();
                    js += "else ";
                }
            }
            return s;
        }
        public object Visit(ParseTree.Statement.While s) {
            js += "while (";
            if (s.Condition != null) {
                object condition = s.Condition.Accept(this);
                if (condition == null) {
                    return null;
                }
                s.Condition = condition as ParseTree.Expression;
            }
            js += ")";
            if (s.Body != null) {
                js += " ";
                s.Body.Accept(this);
            }
            else {
                js += ";";
            }

            return s;
        }
        public object Visit(ParseTree.Statement.For s) {
            js += "for (";
            if (s.Initializers != null) {
                if (s.Initializers.Count > 0) {
                    js += "let ";
                }
                for (int i = 0, size = s.Initializers.Count; i < size; ++i) {
                    js += s.Initializers[i].Name.Lexeme + " = ";
                    if (s.Initializers[i].Initializer != null) {
                        s.Initializers[i].Initializer.Accept(this);
                    }
                    else {
                        js += Types.GetDefualtValueAsString(s.Initializers[i].Type.GetPath());
                    }

                    if (i < size - 1) {
                        js += ", ";
                    }
                }
            }
            js += "; ";
            if (s.Condition != null) {
                s.Condition.Accept(this);
            }
            js += "; ";
            if (s.Iterators != null) {
                for (int i = 0, size = s.Iterators.Count; i < size; ++i) {
                    s.Iterators[i].Accept(this);
                }
            }
            js += ")";
            if (s.Body != null) {
                js += " ";
                s.Body.Accept(this);
            }
            else {
                js += ";\n";
            }

            return s;
        }
        public object Visit(ParseTree.Expression.Group e) {
            js += "(";
            e.Target.Accept(this);
            js += ")";
            return e;
        }
        public object Visit(ParseTree.Expression.Literal e) {
            js += e.Value.Lexeme;
            return e;
        }
        public object Visit(ParseTree.Expression.Get e) {
            if (e.Object != null) {
                e.Object.Accept(this);
                js += ".";
            }
            js += e.Property.Lexeme;

            return e;
        }
        public object Visit(ParseTree.Expression.Set e) {
            if (e.Object != null) {
                e.Object.Accept(this);
                js += ".";
            }
            js += e.Property.Lexeme;

            if (e.Value != null) {
                js += " ";
                js += e.Operator.Lexeme;
                js += " ";
                e.Value.Accept(this);
            }
            return e;
        }
        public object Visit(ParseTree.Expression.Unary e) {
            if (e.Precedence == ParseTree.Expression.Unary.OperatorType.PREFIX) {
                js += e.Operator.Lexeme;
            }
            e.Object.Accept(this);
            if (e.Precedence == ParseTree.Expression.Unary.OperatorType.POSTFIX) {
                js += e.Operator.Lexeme;
            }

            return e;
        }
        public object Visit(ParseTree.Expression.Binary e) {
            e.Left.Accept(this);

            js += " " + e.Operator.Lexeme + " ";

            e.Right.Accept(this);

            return e;
        }
        public object Visit(ParseTree.Expression.Call e) {
            string arrayFunction = e.ArrayFunctionName;
            string mapFunction = e.MapFunctionName;
            string stringFunction = e.StringFunctionName;
            string mathFunctionName = e.MathFunctionName;
            
            if (arrayFunction != null) {
                if (arrayFunction == "length") {
                    e.Arguments[0].Accept(this);
                    js += ".length";
                    return e;
                }
                else if (arrayFunction == "first" || arrayFunction == "last") {
                    e.Arguments[0].Accept(this);
                    js += ".";
                    if (arrayFunction == "first") {
                        js += "findIndex";
                    }
                    else {
                        js += "findLastIndex";
                    }
                    js += "((_js__array_find_target_) => _js__array_find_target_ === (";
                    e.Arguments[1].Accept(this);
                    js += "))";
                    return e;
                }
                else if (arrayFunction == "at" || arrayFunction == "concat" || arrayFunction == "join"
                    ) {
                    e.Arguments[0].Accept(this);
                    js += ".";
                    js += arrayFunction;
                    js += "(";
                    e.Arguments[1].Accept(this);
                    js += ")";
                    return e;
                }
                else if (arrayFunction == "pop" || arrayFunction == "shift" || arrayFunction == "reverse") {
                    e.Arguments[0].Accept(this);
                    js += ".";
                    js += arrayFunction;
                    js += "()";
                    return e;
                }
                else if (arrayFunction == "slice") {
                    e.Arguments[0].Accept(this);
                    js += ".";
                    js += arrayFunction;
                    js += "(";
                    e.Arguments[1].Accept(this);
                    js += ", ";
                    {
                        js += "(";
                        e.Arguments[1].Accept(this);
                        js += ") + (";
                        e.Arguments[2].Accept(this);
                        js += ")";
                    }
                    js += ")";
                    return e;
                }
                else if (arrayFunction == "sort") {
                    e.Arguments[0].Accept(this);
                    js += ".";
                    js += arrayFunction;
                    js += "(";

                    // Skip if arg is null
                    if (e.Arguments[1] is ParseTree.Expression.Literal) {
                        ParseTree.Expression.Literal lit = e.Arguments[1] as ParseTree.Expression.Literal;
                        if (lit.Value.Symbol != Symbol.LIT_NULL) {
                            e.Arguments[1].Accept(this);
                        }
                    }
                    else {
                        e.Arguments[1].Accept(this);
                    }
                    js += ")";
                    return e;
                }
                else if (arrayFunction == "remove") {
                    e.Arguments[0].Accept(this);
                    js += ".splice(";
                    e.Arguments[1].Accept(this);
                    js += ", ";
                    e.Arguments[2].Accept(this);
                    js += ")";
                    return e;
                }
                else if (arrayFunction == "add") {
                    e.Arguments[0].Accept(this);
                    js += ".push(";
                    e.Arguments[1].Accept(this);
                    js += ")";
                    return e;
                }
                else if (arrayFunction == "insert") {
                    e.Arguments[0].Accept(this);
                    js += ".splice(";
                    e.Arguments[1].Accept(this); // Index
                    js += ", 0, "; // Remove none
                    e.Arguments[2].Accept(this); // Insert item
                    js += ")";
                    return e;
                }
                else if (arrayFunction == "copy") {
                    e.Arguments[0].Accept(this);
                    js += ".slice(0, ";
                    e.Arguments[0].Accept(this);
                    js += ".length)";
                    return e;
                }
                else if (arrayFunction == "clear") {
                    e.Arguments[0].Accept(this);
                    js += ".length = 0";
                    return e;
                }
                else {
                    Compiler.Error("JS Compiler", "Unknown array function: " + arrayFunction, e.CallSite.Location);
                }
            }
            else if (mapFunction != null) {
                if (mapFunction == "clear") {
                    e.Arguments[0].Accept(this);
                    js += "." + mapFunction + "()";

                    return e;
                }
                if (mapFunction == "keys" || mapFunction == "values") {
                    js += "Array.from(";
                    e.Arguments[0].Accept(this);
                    js += "." + mapFunction;
                    js += "())";

                    return e;
                }
                else if (mapFunction == "set") {
                    e.Arguments[0].Accept(this);
                    js += ".set(";
                    e.Arguments[1].Accept(this);
                    js += ", ";
                    e.Arguments[2].Accept(this);
                    js += ")";
                    return e;
                }
                else if (mapFunction == "remove" || mapFunction == "get" || mapFunction == "has") {
                    e.Arguments[0].Accept(this);
                    if (mapFunction == "remove") {
                        js += ".delete(";
                    }
                    else {
                        js += "." + mapFunction + "(";
                    }
                    e.Arguments[1].Accept(this);
                    js += ")";
                    return e;
                }
                else {
                    Compiler.Error("JS Compiler", "Unknown map function: " + mapFunction, e.CallSite.Location);
                }
            }
            else if (stringFunction != null) {
                if (stringFunction == "length") {
                    e.Arguments[0].Accept(this);
                    js += "." + stringFunction;
                    return e;
                }
                else if (stringFunction == "at" || stringFunction == "first" || stringFunction == "last"
                    || stringFunction == "starts" || stringFunction == "ends" || stringFunction == "concat"
                    || stringFunction == "split" || stringFunction == "at") {
                    e.Arguments[0].Accept(this);
                    if (stringFunction == "first") {
                        js += ".indexOf(";
                    }
                    else if (stringFunction == "last") {
                        js += ".lastIndexOf(";
                    }
                    else if (stringFunction == "starts" || stringFunction == "ends") {
                        js += "." + stringFunction + "With(";
                    }
                    else {
                        js += "." + stringFunction + "(";
                    }
                    e.Arguments[1].Accept(this);
                    js += ")";
                    return e;
                }
                else if (stringFunction == "lower" || stringFunction == "upper") {
                    e.Arguments[0].Accept(this);
                    if (stringFunction == "upper") {
                        js += ".toUpperCase()";
                    }
                    else {
                        js += ".toLowerCase()";
                    }
                    return e;
                }
                else if (stringFunction == "replace") {
                    e.Arguments[0].Accept(this);
                    js += ".replace(";
                    e.Arguments[1].Accept(this);
                    js += ", ";
                    e.Arguments[2].Accept(this);
                    js += ")";
                    return e;
                }
                else if (stringFunction == "substring") {
                    e.Arguments[0].Accept(this);
                        js += ".substring(";
                    e.Arguments[1].Accept(this);
                    js += ", (";
                    e.Arguments[2].Accept(this);
                    js += ") + (";
                    e.Arguments[1].Accept(this);
                    js += ") )";

                    return e;
                }
                

            }
            else if (mathFunctionName != null) {
                js += "Math." + mathFunctionName + "(";
                for (int i = 0, size = e.Arguments.Count; i < size; ++i) {
                    e.Arguments[i].Accept(this);
                    if (i < size - 1) {
                        js += ", ";
                    }
                }
                js += ")";
                return e;
            }

            if (e.IsPrint) {
                js += "console.log";
            }
            else {
                e.Object.Accept(this);
            }

            if (e.CallSite.Symbol == Symbol.LBRACKET) {
                if (Types.IsMap(ExpressionTypes[e.Object])) {
                    js += ".get(";
                }
                else {
                    js += "[";
                }
            }
            else {
                js += "(";
            }


            for (int i = 0, size = e.Arguments.Count; i < size; ++i) {
                e.Arguments[i].Accept(this);
                if (i < size - 1) {
                    js += ", ";
                }
            }


            if (e.CallSite.Symbol == Symbol.LBRACKET) {
                if (Types.IsMap(ExpressionTypes[e.Object])) {
                    js += ")";
                }
                else {
                    js += "]";
                }
            }
            else {
                js += ")";
            }

            return e;
        }
        public object Visit(ParseTree.Expression.Cast e) {
            if (e.Target.GetPath() == "string") {
                js += "(\"\"+";
            }
            e.Object.Accept(this);
            if (e.Target.GetPath() == "string") {
                js += ")";
            }

            return e;
        }
        public object Visit(ParseTree.Expression.New e) {
            string targetType = e.Target.GetPath();

            if (Types.IsArray(targetType)) {
                js += "[";
            }
            else if (Types.IsMap(targetType)) {
                js += "new Map(";
                if (e.Arguments.Count != 0) {
                    js += "[";
                }
            }
            else {
                js += "new ";
                js += e.Target.GetPath() + "(";
            }
            for (int i = 0, size = e.Arguments.Count; i < size; ++i) {
                if (Types.IsMap(targetType) && i % 2 == 0) {
                    js += "[";
                }
                e.Arguments[i].Accept(this);
                if (Types.IsMap(targetType) && i % 2 != 0) {
                    js += "]";
                }
                if (i < size - 1) {
                    js += ", ";
                }
            }

            if (Types.IsArray(targetType)) {
                js += "]";
            }
            else if (Types.IsMap(targetType)) {
                if (e.Arguments.Count != 0) {
                    js += "]";
                }
                js += ")";
            }
            else {
                js += ")";
            }

            return e;
        }
    }
}
