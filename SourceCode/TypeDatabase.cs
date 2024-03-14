using System;
using System.Collections.Generic;
using System.Text;
using static CScript.ParseTree.Declaration;
using static CScript.ParseTree.Type;

namespace CScript {
    public class TypeDatabase {
        public List<string> StructNames     { get; protected set; }
        public List<string> EnumNames       { get; protected set; }
        public List<string> DelegateNames   { get; protected set; }
        public List<string> VariableNames   { get; protected set; }
        public List<string> FunctionNames   { get; protected set; }
        public Dictionary<string, ParseTree.Type.Map> Maps { get; protected set; }
        public Dictionary<string, ParseTree.Type.Array> Arrays { get; protected set; }
        public Dictionary<string, ParseTree.Declaration.Struct> Structs { get; protected set; }
        public Dictionary<string, ParseTree.Declaration.Enum> Enums { get; protected set; }
        public Dictionary<string, ParseTree.Declaration.Delegate> Delegates { get; protected set; }
        public Dictionary<string, ParseTree.Declaration.Variable> Variables { get; protected set; }
        public Dictionary<string, ParseTree.Declaration.Function> Functions { get; protected set; }

        public void AddTokens(List<Token> tokens) {
            if (tokens == null) {
                tokens = new List<Token>();
            }

            int i = 0;
            while (i < tokens.Count) {
                Token token = tokens[i++];
                if (token.Symbol == Symbol.STRUCT) {
                    Token name = tokens[i++];
                    if (!StructNames.Contains(name.Lexeme)) {
                        // Always succeed for now, will fail duplicates in parser
                        StructNames.Add(name.Lexeme);
                    }
                    Assert(name, Symbol.IDENTIFIER);

                    Token lbracket = tokens[i++];
                    Assert(lbracket, Symbol.LBRACE);

                    int skip = 0;
                    while (i < tokens.Count) {
                        if (tokens[i].Symbol == Symbol.LBRACE) {
                            skip += 1;
                        }
                        else if (tokens[i].Symbol == Symbol.RBRACE) {
                            if (skip == 0) {
                                break;
                            }
                            skip -= 1;
                        }
                        i += 1;
                    }
                    Token rbracket = tokens[i++];
                    Assert(rbracket, Symbol.RBRACE);

                    continue;
                }
                else if (token.Symbol == Symbol.ENUM) {
                    Token name = tokens[i++];
                    if (!EnumNames.Contains(name.Lexeme)) {
                        // Always succeed for now, will fail duplicates in parser
                        EnumNames.Add(name.Lexeme);
                    }
                    Assert(name, Symbol.IDENTIFIER);

                    Token lbracket = tokens[i++];
                    Assert(lbracket, Symbol.LBRACE);
                    /*while (i < tokens.Count && tokens[i].Symbol != Symbol.RBRACE) {
                        i += 1;
                    }*/
                    int skip = 0;
                    while (i < tokens.Count) {
                        if (tokens[i].Symbol == Symbol.LBRACE) {
                            skip += 1;
                        }
                        else if (tokens[i].Symbol == Symbol.RBRACE) {
                            if (skip == 0) {
                                break;
                            }
                            skip -= 1;
                        }
                        i += 1;
                    }
                    Token rbracket = tokens[i++];
                    Assert(rbracket, Symbol.RBRACE);

                    continue;
                }
                else if (token.Symbol == Symbol.DELEGATE) {
                    Token _type = tokens[i++];
                    Assert(_type, Symbol.TYPE_BOOL, Symbol.TYPE_CHAR, Symbol.TYPE_NUM, Symbol.TYPE_OBJECT, Symbol.TYPE_STRING, Symbol.TYPE_VOID, Symbol.IDENTIFIER);

                    if (tokens[i].Symbol == Symbol.LBRACKET) {
                        int skip = 0;
                        Token lbracket = tokens[i++];
                        Assert(lbracket, Symbol.LBRACKET);
                        while (i < tokens.Count && tokens[i].Symbol != Symbol.RBRACKET) {
                            if (tokens[i].Symbol == Symbol.LBRACKET) {
                                skip += 1;
                                i += 1;
                            }
                            else if (tokens[i].Symbol == Symbol.RBRACKET) {
                                if (skip == 0) {
                                    i += 1;
                                    break;
                                }
                                else {
                                    skip -= 1;
                                    i += 1;
                                }
                            }
                            else {
                                i += 1;
                            }
                        }
                        Token rbracket = tokens[i++];
                        Assert(rbracket, Symbol.RBRACKET);
                    }

                    Token name = tokens[i++];
                    if (!DelegateNames.Contains(name.Lexeme)) {
                        // Always succeed for now, will fail duplicates in parser
                        DelegateNames.Add(name.Lexeme);
                    }

                    while (i < tokens.Count && tokens[i].Symbol != Symbol.SEMICOLON) {
                        i += 1;
                    }

                    Token semi = tokens[i++];
                    Assert(semi, Symbol.SEMICOLON);

                    continue;
                }

                // Variable or function
                Token type = token;
                if (!Is(type, Symbol.TYPE_BOOL, Symbol.TYPE_CHAR, Symbol.TYPE_NUM, Symbol.TYPE_OBJECT, Symbol.TYPE_STRING, Symbol.TYPE_VOID, Symbol.IDENTIFIER)) {
                    continue;
                }

                // Skip array or map
                if (tokens[i].Symbol == Symbol.LBRACKET) {
                    int c = i;

                    int skip = 0;
                    Token lbracket = tokens[c++];
                    if (!Is(lbracket, Symbol.LBRACKET)) {
                        continue;
                    }
                    while (c < tokens.Count && tokens[c].Symbol != Symbol.RBRACKET) {
                        if (tokens[c].Symbol == Symbol.LBRACKET) {
                            skip += 1;
                            c += 1;
                        }
                        else if (tokens[c].Symbol == Symbol.RBRACKET) {
                            if (skip == 0) {
                                c += 1;
                                break;
                            }
                            else {
                                skip -= 1;
                                c += 1;
                            }
                        }
                        else {
                            c += 1;
                        }
                    }
                    Token rbracket = tokens[c++];
                    if (!Is(rbracket, Symbol.RBRACKET)) {
                        continue;
                    }

                    i = c;
                }

                Token _name = tokens[i++];
                if (!Is(_name, Symbol.IDENTIFIER)) {
                    continue;
                }

                if (tokens[i].Symbol == Symbol.LPAREN) {
                    if (!FunctionNames.Contains(_name.Lexeme)) {
                        FunctionNames.Add(_name.Lexeme);
                    }

                    while (i < tokens.Count && tokens[i].Symbol != Symbol.LBRACE) {
                        i += 1;
                    }

                    Token lbrace = tokens[i++];
                    Assert(lbrace, Symbol.LBRACE);

                    /*while (i < tokens.Count && tokens[i].Symbol != Symbol.RBRACE) {
                        i += 1;
                    }*/
                    int skip = 0;
                    while (i < tokens.Count) {
                        if (tokens[i].Symbol == Symbol.LBRACE) {
                            skip += 1;
                        }
                        else if (tokens[i].Symbol == Symbol.RBRACE) {
                            if (skip == 0) {
                                break;
                            }
                            skip -= 1;
                        }
                        i += 1;
                    }

                    Token rbrace = tokens[i++];
                    Assert(rbrace, Symbol.RBRACE);

                }
                else if (tokens[i].Symbol == Symbol.EQUAL || tokens[i].Symbol == Symbol.SEMICOLON) {
                    if (!VariableNames.Contains(_name.Lexeme)) {
                        VariableNames.Add(_name.Lexeme);
                    }

                    while (i < tokens.Count && tokens[i].Symbol != Symbol.SEMICOLON) {
                        i += 1;
                    }

                    Token semi = tokens[i++];
                    Assert(semi, Symbol.SEMICOLON);
                }
                else {
                    Compiler.Error("Type Database Pre Parser", "Unknown declaration type. Keyword: " + type.Lexeme, type.Location);
                }
            }
        }
        public TypeDatabase(List<Token> tokens) {
            StructNames = new List<string>();
            EnumNames = new List<string>();
            DelegateNames = new List<string>();
            VariableNames = new List<string>();
            FunctionNames = new List<string>();
            Arrays = new Dictionary<string, ParseTree.Type.Array>();
            Maps = new Dictionary<string, ParseTree.Type.Map>();

            Structs = new Dictionary<string, ParseTree.Declaration.Struct>();
            Enums = new Dictionary<string, ParseTree.Declaration.Enum>();
            Delegates = new Dictionary<string, ParseTree.Declaration.Delegate>();
            Variables = new Dictionary<string, ParseTree.Declaration.Variable>();
            Functions = new Dictionary<string, ParseTree.Declaration.Function>();

            if (tokens != null) {
                AddTokens(tokens);
            }
        }

        public List<string> GetAllTypeNames() {
            List<string> result = new List<string>();
            result.AddRange(StructNames    );
            result.AddRange(EnumNames      );
            result.AddRange(DelegateNames  );
            result.AddRange(VariableNames  );
            result.AddRange(FunctionNames);
            return result;
        }
        protected void Assert(Token t, params Symbol[] _s) {
            foreach (Symbol s in _s) {
                if (t.Symbol == s) {
                    return;
                }
            }
            Compiler.Error("Type Database Pre Parser", "Token '" + t.Lexeme + "' has unexpected symbol", t.Location);
        }
        protected bool Is(Token t, params Symbol[] _s) {
            foreach (Symbol s in _s) {
                if (t.Symbol == s) {
                    return true;
                }
            }
            return false;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();

            sb.Append("Structs:\n");
            foreach (string s in StructNames) {
                sb.Append('\t');
                sb.Append(s);
                sb.Append('\n');
            }

            sb.Append("Enums:\n");
            foreach (string s in EnumNames) {
                sb.Append('\t');
                sb.Append(s);
                sb.Append('\n');
            }

            sb.Append("Delegates:\n");
            foreach (string s in DelegateNames) {
                sb.Append('\t');
                sb.Append(s);
                sb.Append('\n');
            }

            sb.Append("Variables:\n");
            foreach (string s in VariableNames) {
                sb.Append('\t');
                sb.Append(s);
                sb.Append('\n');
            }

            sb.Append("Functions:\n");
            foreach (string s in FunctionNames) {
                sb.Append('\t');
                sb.Append(s);
                sb.Append('\n');
            }

            sb.Append("Arrays:\n");
            foreach (KeyValuePair<string, ParseTree.Type.Array> s in Arrays) {
                sb.Append('\t');
                sb.Append(s.Key);
                sb.Append('\n');
            }

            sb.Append("Maps:\n");
            foreach (KeyValuePair<string, ParseTree.Type.Map> s in Maps) {
                sb.Append('\t');
                sb.Append(s.Key);
                sb.Append('\n');
            }
            return sb.ToString();
        }

        public string GetDefualtValueAsString(string type) {
            if (IsNullable(type)) {
                return "null";
            }
            else if (type == "num") {
                return "0";
            }
            else if (type == "bool") {
                return "false";
            }
            else if (type == "char") {
                return "'\\0'";
            }

            Compiler.Error("Type Database", "Can't determine default of " + type, new Location("generated", 0, 0));
            return null;
        }
        public void RegisterStruct(ParseTree.Declaration.Struct _struct) {
            if (!StructNames.Contains(_struct.Name.Lexeme)) {
                StructNames.Add(_struct.Name.Lexeme);
            }
            Structs.Add(_struct.Name.Lexeme, _struct);
        }
        public void RegisterEnum(ParseTree.Declaration.Enum _enum) {
            if (!EnumNames.Contains(_enum.Name.Lexeme)) {
                EnumNames.Add(_enum.Name.Lexeme);
            }
            Enums.Add(_enum.Name.Lexeme, _enum);
        }
        public void RegisterDelegate(ParseTree.Declaration.Delegate _delegate) {
            if (!DelegateNames.Contains(_delegate.Name.Lexeme)) {
                DelegateNames.Add(_delegate.Name.Lexeme);
            }
            Delegates.Add(_delegate.Name.Lexeme, _delegate);
        }
        public void RegisterVariable(ParseTree.Declaration.Variable _variable) {
            if (!VariableNames.Contains(_variable.Name.Lexeme)) {
                VariableNames.Add(_variable.Name.Lexeme);
            }
            Variables.Add(_variable.Name.Lexeme, _variable);
        }
        public void RegisterFunction(ParseTree.Declaration.Function _function) {
            Functions.Add(_function.Name.Lexeme, _function);
            if (!DelegateNames.Contains(_function.GetDelegateName())) {
                DelegateNames.Add(_function.GetDelegateName());
                Delegates.Add(_function.GetDelegateName(), _function.GetDelegate());
            }
        }
        public void RegisterType(ParseTree.Type type) {
            if (type is ParseTree.Type.Primitive) {
                RegisterType(type as ParseTree.Type.Primitive);
            }
            else if (type is ParseTree.Type.Array) {
                RegisterType(type as ParseTree.Type.Array);
            }
            else if (type is ParseTree.Type.Map) {
                RegisterType(type as ParseTree.Type.Map);
            }
            else {
                Compiler.Error("Type database", "Unknown type", type.GetLocation());
            }
        }
        protected void RegisterType(ParseTree.Type.Primitive type) {
            Symbol s = type.Name.Symbol;

            if (s == Symbol.TYPE_BOOL || s == Symbol.TYPE_CHAR || s == Symbol.TYPE_STRING ||
                s == Symbol.TYPE_NUM || s == Symbol.TYPE_OBJECT || s == Symbol.TYPE_VOID) {
                return;
            }

            string n = type.Name.Lexeme;
            if (s == Symbol.IDENTIFIER) {
                if (StructNames.Contains(n) || EnumNames.Contains(n) ||
                    DelegateNames.Contains(n) || FunctionNames.Contains(n)) {
                    return;
                }
            }

            Location l = type.Name.Location;
            Compiler.Error("Type Database", "Parsing invalid type: " + n, l);
        }

        static Random random = new Random();
        public static string GetRandomHexNumber(int digits) {
            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X");
        }

        protected string Shuffle(string name) {
            string result = name + "_" + GetRandomHexNumber(8);
            return result;
        }
        public string GenerateFunctionName(string _name, bool shuffle = true) {
            string name = _name;
            if (shuffle) {
                name = Shuffle(_name);
            }
            while (true) {
                if (StructNames.Contains(name) || EnumNames.Contains(name) ||
                    DelegateNames.Contains(name) || VariableNames.Contains(name) ||
                    FunctionNames.Contains(name)) {
                    name = Shuffle(_name);
                    continue;
                }
                break;
            }
            return name;
        }

        protected void RegisterType(ParseTree.Type.Array type) {
            RegisterType(type.Content);

            string name = type.GetPath();
            if (!Arrays.ContainsKey(name)) {
                Arrays.Add(name, type);
            }
        }
        protected void RegisterType(ParseTree.Type.Map type) {
            RegisterType(type.Value);
            RegisterType(type.Key);

            string name = type.GetPath();
            if (!Maps.ContainsKey(name)) {
                Maps.Add(name, type);
            }

            if (!Arrays.ContainsKey(type.Value.GetPath() + "[]")) {
                Arrays.Add(type.Value.GetPath() + "[]", new ParseTree.Type.Array(type.Value));
            }

            if (!Arrays.ContainsKey(type.Key.GetPath() + "[]")) {
                Arrays.Add(type.Key.GetPath() + "[]", new ParseTree.Type.Array(type.Key));
            }
        }

        public bool IsNullable(string type) {
            bool isNotNullable = type == "bool" || type == "num" || type == "char" || type == "$null";
            return !isNotNullable;
        }

        public bool IsPrimitive(string type) {
            return type == "bool" || type == "num" || type == "string" || type == "char" || type == "object" || type == "$null";
        }
        public bool IsStruct(string type) {
            return StructNames.Contains(type);
        }

        public bool IsEnum(string type) {
            return EnumNames.Contains(type);
        }

        public bool IsArray(string type) {
            return Arrays.ContainsKey(type);
        }
        
        public bool IsMap(string type) {
            return Maps.ContainsKey(type);
        }
        public bool CanAssign(string targetType, string valueType, List<string> delegateOverides = null) {
            if (valueType == "void" && targetType != "void") {
                return false;
            }

            if (targetType == "object") {
                return valueType != "void";
            }

            if (valueType == "$null") {
                return IsNullable(targetType);
            }

            if (targetType == "string" && valueType == "char") {
                return true;
            }

            if (IsDelegate(targetType) && IsDelegate(valueType)) {
                ParseTree.Declaration.Delegate t = Delegates[targetType];
                ParseTree.Declaration.Delegate v = Delegates[valueType];

                if (t.Paramaters.Count != v.Paramaters.Count) {
                    return false;
                }

                for (int i = 0, size = t.Paramaters.Count; i < size; ++i) {
                    string typeLeft = t.Paramaters[i].Type.GetPath();
                    if (delegateOverides != null) {
                        typeLeft = delegateOverides[i];
                    }
                    string typeRight = v.Paramaters[i].Type.GetPath();
                    if (typeLeft != typeRight) {
                        return false;
                    }
                }

                if (t.Return.GetPath() != v.Return.GetPath()) {
                    return false;
                }

                return true;
            }

            return targetType == valueType;
        }
        public ParseTree.Type.Array GetArray(string type) {
            return Arrays[type];
        }

        public ParseTree.Declaration.Struct GetStruct(string type) {
            return Structs[type];
        }

        public ParseTree.Declaration.Function GetFunction(string type) {
            return Functions[type];
        }

        public ParseTree.Type.Map GetMap(string type) {
            return Maps[type];
        }

        public string Indexed(string type) {
            if (IsArray(type)) {
                ParseTree.Type.Array arr = GetArray(type);
                return arr.Content.GetPath();
            }
            else if (IsMap(type)) {
                ParseTree.Type.Map map = GetMap(type);
                return map.Value.GetPath();
            }
            else if (type == "string") {
                return "char";
            }

            // Return null would be graceful here
            // but i'm thinking about making indexing an
            // overloadable operator!
            throw new NotImplementedException();
        }

        public bool IsDelegate(string type) {
            return DelegateNames.Contains(type);
        }
        public bool ContainsType(string type) {
            if (Structs.ContainsKey(type)) {
                return true;
            }
            else if (Enums.ContainsKey(type)) {
                return true;
            }
            else if (Delegates.ContainsKey(type)) {
                return true;
            }

            return false;
        }
        public bool ContainsName(string type) {
            if (Variables.ContainsKey(type)) {
                return true;
            }
            if (Functions.ContainsKey(type)) {
                return true;
            }
            return false;
        }
        public void ValidateNewTypeName(Token name, bool allowStruct = false) {
            string type = name.Lexeme;
            string error = null;

            if (Structs.ContainsKey(type)) {
                if (!allowStruct) {
                    error = "Type '" + type + "' is already defined as a struct in file: " + Structs[type].Name.Location.File;
                }
            }
            else if (Enums.ContainsKey(type)) {
                error = "Type '" + type + "' is already defined as an enum in file: " + Enums[type].Name.Location.File;
            }
            else if (Delegates.ContainsKey(type)) {
                error = "Type '" + type + "' is already defined as a delegate in file: " + Delegates[type].Name.Location.File;
            }
            else if (Variables.ContainsKey(type)) {
                error = "Type '" + type + "' is already defined as a global variable name in file: " + Variables[type].Name.Location.File;
            }
            else if (Functions.ContainsKey(type)) {
                error = "Type '" + type + "' is already defined as a function name in file: " + Functions[type].Name.Location.File;
            }

            if (error != null) {
                Compiler.Error("Type Database", error, name.Location);
            }
        }
        public void ValidateNewLocalVariableName(Token name) {
            string type = name.Lexeme;
            string error = null;

            if (Structs.ContainsKey(type)) {
                error = "Variable name '" + type + "' is already defined as a struct in file: " + Structs[type].Name.Location.File;
            }
            else if (Enums.ContainsKey(type)) {
                error = "Variable name '" + type + "' is already defined as an enum in file: " + Enums[type].Name.Location.File;
            }
            else if (Delegates.ContainsKey(type)) {
                error = "Variable name '" + type + "' is already defined as a delegate in file: " + Delegates[type].Name.Location.File;
            }
            else if (Variables.ContainsKey(type)) {
                error = "Variable name '" + type + "' is already defined as a global variable name in file: " + Variables[type].Name.Location.File;
            }
            else if (Functions.ContainsKey(type)) {
                error = "Variable name '" + type + "' is already defined as a function name in file: " + Functions[type].Name.Location.File;
            }

            if (error != null) {
                Compiler.Error("Type Database", error, name.Location);
            }
        }
    }
}
