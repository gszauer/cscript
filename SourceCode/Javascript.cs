using System.Linq.Expressions;

namespace CScript {
    internal class Javascript : ParseTree.Visitor {
        protected string js = null;
        protected TypeDatabase Types = null;
        protected Dictionary<ParseTree.Expression, string> ExpressionTypes = null;
        protected int Indent = 0;
        public string Result { get { return js; } }
        protected static List<string> Vec2Functions = new List<string>() {
            "_vec2_add",
            "_vec2_sub",
            "_vec2_mul",
            "_vec2_div",
            "_vec2_scale",
            "_vec2_dot",
            "_vec2_len",
            "_vec2_lenSq",
            "_vec2_normalized",
            "_vec2_angle",
            "_vec2_project",
            "_vec2_reject",
            "_vec2_reflect",
            "_vec2_lerp",
            "_vec2_slerp",
            "_vec2_nlerp",
            "_vec2_compare"
        };
        protected static List<string> Vec3Functions = new List<string>() {
            "_vec3_add",
            "_vec3_sub",
            "_vec3_mul",
            "_vec3_div",
            "_vec3_scale",
            "_vec3_dot",
            "_vec3_len",
            "_vec3_lenSq",
            "_vec3_normalized",
            "_vec3_angle",
            "_vec3_project",
            "_vec3_reject",
            "_vec3_reflect",
            "_vec3_cross",
            "_vec3_lerp",
            "_vec3_slerp",
            "_vec3_nlerp",
            "_vec3_compare"
        };
        protected static List<string> Vec4Functions = new List<string>() {
            "_vec4_add",
            "_vec4_sub",
            "_vec4_mul",
            "_vec4_div",
            "_vec4_scale",
            "_vec4_dot",
            "_vec4_len",
            "_vec4_lenSq",
            "_vec4_normalized",
            "_vec4_angle",
            "_vec4_project",
            "_vec4_reject",
            "_vec4_reflect",
            "_vec4_cross",
            "_vec4_lerp",
            "_vec4_slerp",
            "_vec4_nlerp",
            "_vec4_compare"
        };
        protected static List<string> QuatFunctions = new List<string>() {
            "_quat_angleAxis",
            "_quat_fromTo",
            "_quat_lookRotation",
            "_quat_getAxis",
            "_quat_getAngle",
            "_quat_add",
            "_quat_sub",
            "_quat_mul",
            "_quat_rotate",
            "_quat_scale",
            "_quat_negate",
            "_quat_compare",
            "_quat_compare_orientation",
            "_quat_dot",
            "_quat_lenSq",
            "_quat_len",
            "_quat_normalized",
            "_quat_conjugate",
            "_quat_inverse",
            "_quat_mix",
            "_quat_nlerp",
            "_quat_slerp",
            "_quat_pow"
        };
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
            if (dName == "array" || dName == "map" || dName == "string" || dName == "print" || 
                dName == "Math" || dName == "math" || dName == "vec3" || dName == "vec2" || 
                dName == "vec4" || dName == "quat") {
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
            if (Vec2Functions.Contains(dName) ||
                Vec3Functions.Contains(dName) ||
                Vec4Functions.Contains(dName) ||
                QuatFunctions.Contains(dName)) {
                return d;
            }

            return VisitForReal(d);
        }
        protected object VisitForReal(ParseTree.Declaration.Function d) {  // Done
            string dName = d.Name.Lexeme;

            if (Vec3Functions.Contains(dName) || Vec2Functions.Contains(dName) || Vec4Functions.Contains(dName) || QuatFunctions.Contains(dName)) {
                js += "function (";
            }
            else {
                js += "function " + dName + "(";
            }

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
            if (dName == "_array" || dName == "_map" || dName == "_string" || dName == "_math" || 
                dName == "vec3" || dName == "vec2" || dName == "vec4" || dName == "quat") {
                return d;
            }

            if (dName == "_vec2") {
                dName = "vec2";
            }
            else if (dName == "_vec3") {
                dName = "vec3";
            }
            else if (dName == "_vec4") {
                dName = "vec4";
            }
            else if (dName == "_quat") {
                dName = "quat";
            }

            js += "class " + dName + " {\n";

            js += "\tconstructor(";
            if (dName == "vec2") {
                js += "x, y";
            }
            else if (dName == "vec3") {
                js += "x, y, z";
            }
            else if (dName == "vec4" || dName == "quat") {
                js += "x, y, z, w";
            }
            else {
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
            }
            js += ") {\n";

            if (dName == "vec2") {
                js += "\t\tthis.x = (typeof x === \"undefined\")? 0.0 : x;\n";
                js += "\t\tthis.y = (typeof y === \"undefined\")? 0.0 : y;\n";
            }
            else if (dName == "vec3") {
                js += "\t\tthis.x = (typeof x === \"undefined\")? 0.0 : x;\n";
                js += "\t\tthis.y = (typeof y === \"undefined\")? 0.0 : y;\n";
                js += "\t\tthis.z = (typeof z === \"undefined\")? 0.0 : z;\n";
            }
            else if (dName == "vec4") {
                js += "\t\tthis.x = (typeof x === \"undefined\")? 0.0 : x;\n";
                js += "\t\tthis.y = (typeof y === \"undefined\")? 0.0 : y;\n";
                js += "\t\tthis.z = (typeof z === \"undefined\")? 0.0 : z;\n";
                js += "\t\tthis.w = (typeof w === \"undefined\")? 0.0 : w;\n";
            }
            else if (dName == "quat") {
                js += "\t\tthis.x = (typeof x === \"undefined\")? 0.0 : x;\n";
                js += "\t\tthis.y = (typeof y === \"undefined\")? 0.0 : y;\n";
                js += "\t\tthis.z = (typeof z === \"undefined\")? 0.0 : z;\n";
                js += "\t\tthis.w = (typeof w === \"undefined\")? 1.0 : w;\n";
            }
            else {
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
            }

            js += "\t}\n"; // End constructor

            if (dName == "vec3" || dName == "vec2" || dName == "vec4" || dName == "quat") {
                for (int i = 0, size = d.Members.Count; i < size; ++i) {
                    string nam = d.Members[i].Name.Lexeme;
                    if (nam == "delete") {
                        nam = "_delete__";
                    }
                    js += "\tstatic " + d.Members[i].Name.Lexeme + " = ";

                    if (d.Members[i].Initializer != null) {
                        if (d.Members[i].Initializer is ParseTree.Expression.Literal) {
                            d.Members[i].Initializer.Accept(this);
                            js += ";\n\n";
                        }
                        else if (!(d.Members[i].Initializer is ParseTree.Expression.Get)) {
                            Compiler.Error("Javascript Compiler", "vec3 initializer must be function", d.Members[i].Name.Location);
                        }
                        else {
                            ParseTree.Expression.Get g = (ParseTree.Expression.Get)d.Members[i].Initializer;
                            string funName = g.Property.Lexeme;
                            ParseTree.Declaration.Function fun = Types.GetFunction(funName);
                            Indent++;
                            VisitForReal(fun);
                            Indent--;
                        }
                    }
                    else {
                        js += Types.GetDefualtValueAsString(d.Members[i].Type.GetPath());
                        js += ";\n";
                    }
                }
            }

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
            string leftType = ExpressionTypes[e.Left];
            string rightType = ExpressionTypes[e.Right];
            if (e.Operator.Symbol == Symbol.PLUS) {
                if (leftType == "string" && rightType == "vec2") {
                    e.Left.Accept(this);
                    js += " + (\"(\" + ";
                    e.Right.Accept(this);
                    js += ".x + \", \" + ";
                    e.Right.Accept(this);
                    js += ".y + \")\")";
                    return e;
                }
                else if (leftType == "string" && rightType == "vec3") {
                    e.Left.Accept(this);
                    js += " + (\"(\" + ";
                    e.Right.Accept(this);
                    js += ".x + \", \" + ";
                    e.Right.Accept(this);
                    js += ".y + \", \" + ";
                    e.Right.Accept(this);
                    js += ".z + \")\")";
                    return e;
                }
                else if (leftType == "string" && rightType == "vec4") {
                    e.Left.Accept(this);
                    js += " + (\"(\" + ";
                    e.Right.Accept(this);
                    js += ".x + \", \" + ";
                    e.Right.Accept(this);
                    js += ".y + \", \" + ";
                    e.Right.Accept(this);
                    js += ".z + \", \" + ";
                    e.Right.Accept(this);
                    js += ".w + \")\")";
                    return e;
                }
                else if (leftType == "string" && rightType == "quat") {
                    e.Left.Accept(this);
                    js += " + (\"(\" + ";
                    e.Right.Accept(this);
                    js += ".x + \", \" + ";
                    e.Right.Accept(this);
                    js += ".y + \", \" + ";
                    e.Right.Accept(this);
                    js += ".z + \", \" + ";
                    e.Right.Accept(this);
                    js += ".w + \")\")";
                    return e;
                }
                else if (leftType == rightType && (rightType == "vec2" || rightType == "vec3" || rightType == "vec4" || rightType == "quat")) {
                    js += leftType + ".add(";
                    e.Left.Accept(this);
                    js += ", ";
                    e.Right.Accept(this);
                    js += ")";
                    return e;
                }
            }
            else if (e.Operator.Symbol == Symbol.MINUS) {
                if (leftType == rightType && (rightType == "vec2" || rightType == "vec3" || rightType == "vec4" || rightType == "quat")) {
                    js += rightType + ".sub(";
                    e.Left.Accept(this);
                    js += ", ";
                    e.Right.Accept(this);
                    js += ")";
                    return e;
                }
            }
            else if (e.Operator.Symbol == Symbol.SLASH) {
                if (leftType == rightType && (rightType == "vec2" || rightType == "vec3" || rightType == "vec4")) {
                    js += rightType + ".div(";
                    e.Left.Accept(this);
                    js += ", ";
                    e.Right.Accept(this);
                    js += ")";
                    return e;
                }
            }
            else if (e.Operator.Symbol == Symbol.STAR) {
                if (leftType == rightType && (rightType == "vec2" || rightType == "vec3" || rightType == "vec4" || rightType == "quat")) {
                    js += rightType + ".mul(";
                    e.Left.Accept(this);
                    js += ", ";
                    e.Right.Accept(this);
                    js += ")";
                    return e;
                }
                else if ((rightType == "vec2" || rightType == "vec3" || rightType == "vec4" || rightType == "quat") && rightType == "num") {
                    js += rightType + ".scale(";
                    e.Left.Accept(this);
                    js += ", ";
                    e.Right.Accept(this);
                    js += ")";
                    return e;
                }
            }
            else if (e.Operator.Symbol == Symbol.EQUAL_EQUAL) {
                if (leftType == rightType && (rightType == "vec2" || rightType == "vec3" || rightType == "vec4" || rightType == "quat")) {
                    js += rightType + ".compare(";
                    e.Left.Accept(this);
                    js += ", ";
                    e.Right.Accept(this);
                    js += ")";
                    return e;
                }
            }
            else if (e.Operator.Symbol == Symbol.NOT_EQUAL) {
                if (leftType == rightType && (rightType == "vec2" || rightType == "vec3" || rightType == "vec4" || rightType == "quat")) {
                    js += "!" + rightType + ".compare(";
                    e.Left.Accept(this);
                    js += ", ";
                    e.Right.Accept(this);
                    js += ")";
                    return e;
                }
            }

            e.Left.Accept(this);

            if (e.Operator.Symbol == Symbol.OR) {
                js += " || ";
            }
            else if (e.Operator.Symbol == Symbol.AND) {
                js += " && ";
            }
            else {
                js += " " + e.Operator.Lexeme + " ";
            }

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
                string objectType = ExpressionTypes[e.Object];
                if (objectType == "vec2") {
                    js += "(\"(\" + ";
                    e.Object.Accept(this);
                    js += ".x + \", \" + ";
                    e.Object.Accept(this);
                    js += ".y + \")\")";
                    return e;
                }
                else if (objectType == "vec3") {
                    js += "(\"(\" + ";
                    e.Object.Accept(this);
                    js += ".x + \", \" + ";
                    e.Object.Accept(this);
                    js += ".y + \", \" + ";
                    e.Object.Accept(this);
                    js += ".z + \")\")";
                    return e;
                }
                else if (objectType == "vec4" || objectType == "quat") {
                    js += "(\"(\" + ";
                    e.Object.Accept(this);
                    js += ".x + \", \" + ";
                    e.Object.Accept(this);
                    js += ".y + \", \" + ";
                    e.Object.Accept(this);
                    js += ".z + \")\")";
                    return e;
                }

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
