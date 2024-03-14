using System.Linq.Expressions;
using static CScript.ParseTree.Declaration;

namespace CScript {
    internal class FunctionGenerator : ParseTree.Visitor {
        protected TypeDatabase Types = null;
        protected TypeChecker Checker = null;
        protected Dictionary<ParseTree.Expression, string> ExpressionTypes = null;
        protected class Environment {
            public Environment Parent { get; protected set; }
            public List<string> Variables { get; protected set; }

            public void Create(string name) {
                if (Variables.Contains(name)) {
                    Compiler.Error("Function Generator", "Duplicate name " + name, new Location("generated", 0, 0));
                    return;
                }
                Variables.Add(name);
            }

            public void Create(List<string> names) {
                Variables.AddRange(names);
            }

            public bool Contains(string name) {
                if (Variables.Contains(name)) {
                    return true;
                }
                if (Parent != null) {
                    return Parent.Contains(name);
                }
                return false;
            }

            public Environment(Environment parent) {
                Parent = parent;
                Variables = new List<string>();
            }
        }

        protected Environment Variables = null;

        void PushEnv() {
            Variables = new Environment(Variables);
        }
        void PopEnv() {
            Variables = Variables.Parent;
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
        public string GenerateVariableName(string _name) {
            string name = Shuffle(_name);
            while (true) {
                if (Variables.Contains(name)) {
                    name = Shuffle(_name);
                    continue;
                }
                break;
            }
            return name;
        }
        public FunctionGenerator(List<ParseTree.Declaration.File> parseTree, TypeChecker check, TypeDatabase types, Dictionary<ParseTree.Expression, string> expressionTypes) {
            Variables = new Environment(null);
            ExpressionTypes = expressionTypes;
            Types = types;
            Checker = check;


            Variables.Create(types.GetAllTypeNames());

            List<ParseTree.Declaration.Function> GeneratedArrayConstructors = new List<ParseTree.Declaration.Function>();
            List<ParseTree.Declaration.Function> GeneratedMapConstructors = new List<ParseTree.Declaration.Function>();
            List<ParseTree.Declaration.Function> GeneratedStructConstructors = new List<ParseTree.Declaration.Function>();
            foreach (ParseTree.Declaration.File file in parseTree) {
                if (file.Accept(this) == null) {
                    Compiler.Error("Type Checker", "Type checker failed", new Location(file.Path, 0, 0));
                    break;
                }
            }
        }
        public object Visit(ParseTree.Type.Primitive t) {
            // Nothing to do
            return t;
        }
        public object Visit(ParseTree.Type.Array t) {
            object content = t.Content.Accept(this);
            if (content == null) {
                return null;
            }
            t.Content = content as ParseTree.Type;

            return t;
        }
        public object Visit(ParseTree.Type.Map t) {
            object key = t.Key.Accept(this);
            if (key == null) {
                return null;
            }
            t.Key = key as ParseTree.Type;

            object value = t.Value.Accept(this);
            if (value == null) {
                return null;
            }
            t.Value = value as ParseTree.Type;

            return t;
        }
        public object Visit(ParseTree.Declaration.File d) {
            for (int i = 0, size = d.Content.Count; i < size; ++i) {
                object declaration = d.Content[i].Accept(this);
                if (declaration == null) {
                    return null;
                }
                d.Content[i] = declaration as ParseTree.Declaration;
            }

            return d;
        }
        public object Visit(ParseTree.Declaration.Variable d) {
            object type = d.Type.Accept(this);
            if (type == null) {
                return null;
            }
            d.Type = type as ParseTree.Type;

            if (d.Initializer != null) {
                object initializer = d.Initializer.Accept(this);
                if (initializer  == null) {
                    return null;
                }
                d.Initializer = initializer as ParseTree.Expression;
            }

            return d;
        }
        public object Visit(ParseTree.Declaration.Function d) {
            PushEnv();
            object _Return = d.Return.Accept(this);
            if (_Return == null) {
                return null;
            }
            d.Return = _Return as ParseTree.Type;

            for (int i = 0, size = d.Paramaters.Count; i < size; ++i) {
                object param = d.Paramaters[i].Type.Accept(this);
                if (param == null) {
                    return null;
                }
                d.Paramaters[i].Type = param as ParseTree.Type;
            }
            if (d.Body != null) {
                object body = d.Body.Accept(this);
                if (body == null) {
                    return null;
                }
                d.Body = body as ParseTree.Statement.Block;
            }
            PopEnv();

            return d;
        }
        public object Visit(ParseTree.Declaration.Enum d) {
            // Nothing to really do
            return d;
        }
        public object Visit(ParseTree.Declaration.Delegate d) {
            PushEnv();
            object _return = d.Return.Accept(this);
            if (_return == null) {
                return null;
            }
            d.Return = _return as ParseTree.Type;
            for (int i = 0, size = d.Paramaters.Count; i < size; ++i) {
                object param = d.Paramaters[i].Type.Accept(this);
                if (param == null) {
                    return null;
                }
                d.Paramaters[i].Type = param as ParseTree.Type;
            }
            PopEnv();
            return d;
        }

        public object Visit(ParseTree.Declaration.Struct d) {
            PushEnv();
            for (int i = 0, size = d.Members.Count; i < size; ++i) {
                object member = d.Members[i].Accept(this);
                if (member == null) {
                    return null;
                }
                d.Members[i] = member as ParseTree.Statement.Variable;
            }
            PopEnv();
            return d;
        }
        public object Visit(ParseTree.Statement.Block s) {
            PushEnv();
            for (int i = 0, size = s.Body.Count; i < size; ++i) {
                object stmt = s.Body[i].Accept(this);
                if (stmt == null) {
                    return null;
                }
                s.Body[i] = stmt as ParseTree.Statement;
            }
            PopEnv();

            return s;
        }
        public object Visit(ParseTree.Statement.Variable s) {
            object type = s.Type.Accept(this);
            if (type == null) {
                return null;
            }
            s.Type = type as ParseTree.Type;

            if (s.Initializer != null) {
                object initializer = s.Initializer.Accept(this);
                if (initializer == null) {
                    return null;
                }
                s.Initializer = initializer as ParseTree.Expression;
            }

            Variables.Create(s.Name.Lexeme);

            return s;
        }
        public object Visit(ParseTree.Statement.Expression s) {
            object target = s.Target.Accept(this);
            if (target == null) {
                return null;
            }
            s.Target = target as ParseTree.Expression;
            return s;
        }
        public object Visit(ParseTree.Statement.Control s) {
            if (s.Value != null) {
                object value = s.Value.Accept(this);
                if (value == null) {
                    return null;
                }
                s.Value = value as ParseTree.Expression;
            }
            return s;
        }
        public object Visit(ParseTree.Statement.If s) {
            ParseTree.Statement.If iter = s;
            while (iter != null) {
                if (iter.Condition != null) {
                    object condition = iter.Condition.Accept(this);
                    if (condition == null) {
                        return null;
                    }
                    iter.Condition = condition as ParseTree.Expression;
                }
                if (iter.Body != null) {
                    object body = iter.Body.Accept(this);
                    if (body == null) {
                        return null;
                    }
                    iter.Body = body as ParseTree.Statement.Block;
                }

                iter = iter.Next;
            }
            return s;
        }
        public object Visit(ParseTree.Statement.While s) {
            if (s.Condition != null) {
                object condition = s.Condition.Accept(this);
                if (condition  == null) {
                    return null;
                }
                s.Condition = condition as ParseTree.Expression;
            }
            if (s.Body != null) {
                object body = s.Body.Accept(this);
                if (body == null) {
                    return null;
                }
                s.Body = body as ParseTree.Statement.Block;
            }

            return s;
        }
        public object Visit(ParseTree.Statement.For s) {
            PushEnv();
            if (s.Initializers != null) {
                for (int i = 0, size = s.Initializers.Count; i < size; ++i) {
                    object initializer = s.Initializers[i].Accept(this);
                    if (initializer == null) {
                        return null;
                    }
                    s.Initializers[i] = initializer as ParseTree.Statement.Variable;
                }
            }
            if (s.Condition != null) {
                object condition = s.Condition.Accept(this);
                if (condition == null) {
                    return null;
                }
                s.Condition = condition as ParseTree.Expression;
            }
            if (s.Iterators != null) {
                for (int i = 0, size = s.Iterators.Count; i < size; ++i) {
                    object iterator = s.Iterators[i].Accept(this);
                    if (iterator == null) {
                        return null;
                    }
                    s.Iterators[i] = iterator as ParseTree.Expression;
                }
            }
            if (s.Body != null) {
                object body = s.Body.Accept(this);
                if (body == null) {
                    return null;
                }
                s.Body = body as ParseTree.Statement.Block;
            }
            PopEnv();

            return s;
        }
        public object Visit(ParseTree.Expression.Group e) {
            object target = e.Target.Accept(this);
            if (target == null) {
                return null;
            }
            e.Target = target as ParseTree.Expression;
            return e;
        }
        public object Visit(ParseTree.Expression.Literal e) {
            // Nothing to do
            return e;
        }
        public object Visit(ParseTree.Expression.Get e) {
            if (e.Object != null) {
                object obj = e.Object.Accept(this);
                if (obj == null) {
                    return null;
                }
                e.Object = obj as ParseTree.Expression;
            }

            return e;
        }
        public object Visit(ParseTree.Expression.Set e) {
            if (e.Object != null) {
                object obj = e.Object.Accept(this);
                if (obj == null) {
                    return null;
                }
                e.Object = obj as ParseTree.Expression;
            }
            if (e.Value != null) {
                object value = e.Value.Accept(this);
                if (value == null) {
                    return null;
                }
                e.Value = value as ParseTree.Expression;
            }
            return e;
        }
        public object Visit(ParseTree.Expression.Unary e) {
            object obj = e.Object.Accept(this);
            if (obj == null) {
                return null;
            }
            e.Object = obj as ParseTree.Expression;

            return e;
        }
        public object Visit(ParseTree.Expression.Binary e) {
            object left = e.Left.Accept(this);
            if (left == null) {
                return null;
            }
            e.Left = left as ParseTree.Expression;

            object right = e.Right.Accept(this);
            if (right == null) {
                return null;
            }
            e.Right = right as ParseTree.Expression;

            return e;
        }
        public object Visit(ParseTree.Expression.Call e) {
            if (e.Object != null) {
                object obj = e.Object.Accept(this);
                if (obj == null) {
                    return null;
                }
                e.Object = obj as ParseTree.Expression;
            }
            for (int i = 0, size = e.Arguments.Count; i < size; ++i) {
                object arg = e.Arguments[i].Accept(this);
                if (arg == null) {
                    return null;
                }
                e.Arguments[i] = arg as ParseTree.Expression;
            }

            return e;
        }
        public object Visit(ParseTree.Expression.Cast e) {
            if (e.Object != null) {
                object obj = e.Object.Accept(this);
                if (obj == null) {
                    return null;
                }
                e.Object = obj as ParseTree.Expression;
            }
            if (e.Target != null) {
                object target = e.Target.Accept(this);
                if (target == null) {
                    return null;
                }
                e.Target = target as ParseTree.Type;
            }
            return e;
        }
        public object Visit(ParseTree.Expression.New e) {
            object target = e.Target.Accept(this);
            if (target == null) {
                return null;
            }
            e.Target = target as ParseTree.Type;

            for (int i = 0, size = e.Arguments.Count; i < size; ++i) {
                object arg = e.Arguments[i].Accept(this);
                if (arg == null) {
                    return null;
                }
                e.Arguments[i] = arg as ParseTree.Expression;
            }

            if (Types.IsArray(e.Target.GetPath())) {
                return e;
            }
            else if (Types.IsMap(e.Target.GetPath())) {
                return e;
            }
            else if (Types.IsStruct(e.Target.GetPath())) {
                return e;
            }

            return e;
        }
    }
}
