using static CScript.ParseTree.Declaration;
using static CScript.ParseTree.Declaration.Function;

namespace CScript {
    public interface ParseTree {
        public object Accept(ParseTree.Visitor visitor);
        public interface Visitor {
            public object Visit(ParseTree.Type.Primitive t);
            public object Visit(ParseTree.Type.Array t);
            public object Visit(ParseTree.Type.Map t);
            public object Visit(ParseTree.Declaration.File d);
            public object Visit(ParseTree.Declaration.Variable d);
            public object Visit(ParseTree.Declaration.Function d);
            public object Visit(ParseTree.Declaration.Enum d);
            public object Visit(ParseTree.Declaration.Delegate d);
            public object Visit(ParseTree.Declaration.Struct d);
            public object Visit(ParseTree.Statement.Block s);
            public object Visit(ParseTree.Statement.Variable s);
            public object Visit(ParseTree.Statement.Expression s);
            public object Visit(ParseTree.Statement.Control s);
            public object Visit(ParseTree.Statement.If s);
            public object Visit(ParseTree.Statement.While s);
            public object Visit(ParseTree.Statement.For s);
            public object Visit(ParseTree.Expression.Group e);
            public object Visit(ParseTree.Expression.Literal e);
            public object Visit(ParseTree.Expression.Get e);
            public object Visit(ParseTree.Expression.Set e);
            public object Visit(ParseTree.Expression.Unary e);
            public object Visit(ParseTree.Expression.Binary e);
            public object Visit(ParseTree.Expression.Call e);
            public object Visit(ParseTree.Expression.Cast e);
            public object Visit(ParseTree.Expression.New e);
        }
        public interface Type : ParseTree {
            public Type MakeCopy();
            public string GetPath();
            public Location GetLocation();
            public Type GetBase();
            public class Primitive : Type {
                public Token Name;
                public Primitive(Token name) {
                    Name = name;
                }
                public Type MakeCopy() {
                    return new Primitive(new Token(Name.Symbol, Name.Location, Name.Lexeme));
                }
                public string GetPath() {
                    return Name.Lexeme;
                }
                public Location GetLocation() {
                    return Name.Location;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
                public Type GetBase() {
                    return this;
                }

            }
            public class Array : Type {
                public Type Content { get; set; }
                public Array(Type content) {
                    Content = content;
                }
                public Type MakeCopy() {
                    Type content = Content.MakeCopy();
                    return new Array(content);
                }
                public string GetPath() {
                    return Content.GetPath() + "[]";
                }
                public Location GetLocation() {
                    return Content.GetLocation();
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
                public Type GetBase() {
                    return Content.GetBase();
                }
            }
            public class Map : Type {
                public Type Key { get; set; }
                public Type Value { get; set; }
                public Map(Type key, Type value) {
                    Key = key;
                    Value = value;
                }
                public Type MakeCopy() {
                    Type key = Key.MakeCopy();
                    Type value = Value.MakeCopy();
                    return new Map(key, value);
                }
                public string GetPath() {
                    return Value.GetPath() + "[" + Key.GetPath() + "]";
                }
                public Location GetLocation() {
                    return Value.GetLocation();
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
                public Type GetBase() {
                    return Value.GetBase();
                }
            }
        }
        public interface Declaration : ParseTree {
            public class File : Declaration {
                public string Path { get; protected set; }
                public List<Declaration> Content { get; protected set; }
                public File(string path, List<Declaration> content) {
                    Path = path;
                    Content = content;
                    if (Content == null) {
                        Content = new List<Declaration>();
                    }
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Variable : Declaration {
                public ParseTree.Type Type { get; set; }
                public Token Name { get; set; }
                public Expression Initializer { get; set; }
                public Variable(ParseTree.Type type, Token name, Expression initializer) {
                    Type = type;
                    Initializer = initializer;
                    Name = name;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Function : Declaration {
                public class Paramater {
                    public ParseTree.Type Type { get; set; }
                    public Token Name { get; protected set; }
                    public Paramater(ParseTree.Type type, Token name) {
                        Type = type;
                        Name = name;
                    }

                    public Paramater MakeCopy() {
                        return new Paramater(Type.MakeCopy(), Name.MakeCopy());
                    }
                }
                public ParseTree.Type Return { get; set; }
                public Token Name { get; protected set; }
                public List<Paramater> Paramaters { get; set; }
                public Statement.Block Body { get; set; }

                protected static Dictionary<string, ParseTree.Declaration.Delegate> Delegate = null;
                public Function(ParseTree.Type _return, Token name, List<Paramater> _params, Statement.Block body) {
                    Return = _return;
                    Name = name;
                    Paramaters = _params;
                    if (Paramaters == null) {
                        Paramaters = new List<Paramater>();
                    }
                    Body = body;

                    if (Delegate == null) {
                        Delegate = new Dictionary<string, Delegate>();
                    }

                    string delegateName = GetDelegateName();
                    if (!Delegate.ContainsKey(delegateName)) {
                        Delegate.Add(delegateName, new Delegate(
                            _return.MakeCopy(),
                            new Token(Symbol.IDENTIFIER, name.Location, delegateName),
                            CopyParamaters()
                        ));
                    }
                }
                public string GetDelegateName() {
                    string result = "$delegate<" + Return.GetPath() + ">(";
                    for (int i = 0, size = Paramaters.Count; i < size; ++i) {
                        result += Paramaters[i].Type.GetPath();
                        if (i < size - 1) {
                            result += ",";
                        }

                    }
                    result += ")";

                    return result;
                }
                public Delegate GetDelegate() {
                    return Delegate[GetDelegateName()];
                }
                protected List<Paramater> CopyParamaters() {
                    List<Paramater> copy = new List<Paramater>();
                    foreach (Paramater param in Paramaters) {
                        copy.Add(param.MakeCopy());
                    }
                    return copy;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Enum : Declaration {
                public class Member {
                    public Token Name { get; protected set; }
                    public int Value { get; protected set; }
                    public Member(Token name, int value) {
                        Name = name;
                        Value = value;
                    }
                }
                public Token Name { get; protected set; }
                public List<Member> Members { get; protected set; }
                public Enum(Token name, List<Member> members) {
                    this.Name = name;
                    this.Members = members;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Delegate : Declaration {
                public ParseTree.Type Return { get; set; }
                public Token Name { get; protected set; }
                public List<Function.Paramater> Paramaters { get; protected set; }
                public Delegate(Type _return, Token name, List<ParseTree.Declaration.Function.Paramater> paramaters) {
                    Return = _return;
                    Name = name;
                    Paramaters = paramaters;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Struct : Declaration {
                public Token Name { get; protected set; }
                public List<Statement.Variable> Members { get; protected set; }
                public Struct(Token name, List<Statement.Variable> members) {
                    Name = name;
                    Members = members;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
                public Statement.Variable Get(string name) {
                    foreach (Statement.Variable v in Members) {
                        if (v.Name.Lexeme == name) {
                            return v;
                        }
                    }
                    return null;
                }
            }
        }
        public interface Statement : ParseTree {
            public class Block : Statement {
                protected Token Brace { get; set; }
                public List<Statement> Body { get; protected set; }
                public Block(Token brace, List<Statement> body) {
                    Brace = brace;
                    Body = body;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Variable : Statement {
                public ParseTree.Type Type { get; set; }
                public Token Name { get; protected set; }
                public ParseTree.Expression Initializer { get; set; }
                public Variable(ParseTree.Type type, Token name, ParseTree.Expression initializer) {
                    Type = type;
                    Initializer = initializer;
                    Name = name;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Expression : Statement {
                public ParseTree.Expression Target { get; set; }
                public Expression(ParseTree.Expression e) {
                    Target = e;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Control : Statement {
                public Token Keyword { get; protected set; }
                public ParseTree.Expression Value { get; set; }
                public Control(Token keyword, ParseTree.Expression value) {
                    Keyword = keyword;
                    Value = value;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class If : Statement {
                protected Token Keyword { get; set; }
                public ParseTree.Expression Condition { get; set; }
                public Block Body { get; set; }
                public If Next = null;
                public If(Token keyword, ParseTree.Expression condition, Block body) {
                    Keyword = keyword;
                    Condition = condition;
                    Body = body;
                    Next = null;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class While : Statement {
                protected Token Keyword { get; set; }
                public ParseTree.Expression Condition { get; set; }
                public Block Body { get; set; }
                public While(Token keyword, ParseTree.Expression condition, Block body) {
                    Keyword = keyword;
                    Condition = condition;
                    Body = body;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class For : Statement {
                protected Token Keyword { get; set; }
                public List<CScript.ParseTree.Statement.Variable> Initializers { get; set; }
                public ParseTree.Expression Condition { get; set; }
                public List<ParseTree.Expression> Iterators { get; set; }
                public Block Body { get; set; }
                public For(Token keyword, List<Variable> initializers, ParseTree.Expression condition, List<ParseTree.Expression> iterators, Block body) {
                    Keyword = keyword;
                    Initializers = initializers;
                    Condition = condition;
                    Iterators = iterators;
                    Body = body;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
        }
        public interface Expression : ParseTree {
            public class Group : Expression {
                public Expression Target { get; set; }
                public Group(Expression target) {
                    Target = target;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Literal : Expression {
                public Token Value { get; protected set; }
                public Literal(Token value) {
                    Value = value;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Get : Expression {
                public Expression Object { get; set; }
                public Token Property { get; set; }
                public Get(Expression _object, Token property) {
                    Object = _object;
                    Property = property;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Set : Expression {
                public Expression Object { get; set; }
                public Token Property { get; protected set; }
                public Token Operator { get; protected set; }
                public Expression Value { get; set; }
                public Set(Expression _object, Token property, Token _operator, Expression value) {
                    Object = _object;
                    Property = property;
                    Operator = _operator;
                    Value = value;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Unary : Expression {
                public enum OperatorType {
                    PREFIX,
                    POSTFIX
                }
                public Token Operator { get; protected set; }
                public Expression Object { get; set; }
                public OperatorType Precedence { get; protected set; }
                public Unary(Token _operator, Expression _object, OperatorType precedence) {
                    Operator = _operator;
                    Object = _object;
                    Precedence = precedence;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Binary : Expression {
                public Token Operator { get; protected set; }
                public Expression Left { get; set; }
                public Expression Right { get; set; }
                public Binary(Token _operator, Expression left, Expression right) {
                    Operator = _operator;
                    Left = left;
                    Right = right;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class Call : Expression {
                public Token CallSite { get; protected set; }
                public Expression Object { get; set; }
                public List<Expression> Arguments { get; protected set; }
                public Call(Token callSite, Expression _object, List<Expression> aRguments) {
                    CallSite = callSite;
                    Object = _object;
                    Arguments = aRguments;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }

                protected string GetFunctionName(string[] functions, string target) {
                    if (Object != null && Object is Expression.Get) {
                        Expression.Get _get = Object as Expression.Get;
                        string propName = _get.Property.Lexeme;
                        if (functions.Contains(propName)) {
                            if (_get.Object != null && _get.Object is Expression.Get) {
                                _get = _get.Object as Expression.Get;
                                if (_get.Object == null) {
                                    if (_get.Property.Lexeme == target) {
                                        return propName;
                                    }
                                }
                            }
                        }
                    }
                    return null;
                }
                public string MapFunctionName {
                    get {
                        string[] functions = new string[] {
                            "clear", "remove", "get", "set", "has", "keys", "values"
                        };
                        return GetFunctionName(functions, "map");
                    }
                }
                public string ArrayFunctionName {
                    get {
                        string[] functions = new string[] {
                            "clear", "length", "first", "last", "at", "concat", "join", "pop", "shift", "insert", "slice", "sort", "reverse", "remove", "add", "copy"
                        };
                        return GetFunctionName(functions, "array");
                    }
                }

                public string StringFunctionName {
                    get {
                        string[] functions = new string[] {
                            "length", "at", "concat", "ends", "starts", "first", "last", "replace", "split", "substring", "lower", "upper"
                        };
                        return GetFunctionName(functions, "string");
                    }
                }

                public string MathFunctionName {
                    get {
                        string[] functions = new string[] {
                            "abs", "cos", "sin","tan", "sqrt", "exp","pow", "random", "acos","asin" , "atan"  , "atan2" ,"imul" , "max"   , "min","log"  , "ceil"  , "floor","round", "sign"  , "trunc"
                        };
                        string result = GetFunctionName(functions, "math");
                        if (result == null) {
                            result = GetFunctionName(functions, "Math");
                        }

                        return result;
                    }
                }
                public bool IsPrint {
                    get {
                        if (Object != null) {
                            if (Object is ParseTree.Expression.Get) {
                                ParseTree.Expression.Get gObject = Object as ParseTree.Expression.Get;
                                if (gObject.Object == null) {
                                    if (gObject.Property.Lexeme == "print") {
                                        return true;
                                    }
                                }
                            }
                        }
                        return false;
                    }
                }
            }
            public class Cast : Expression {
                public Expression Object { get; set; }
                public Token Operator { get; protected set; }
                public Type Target { get; set; }
                public Cast(Expression _object, Token _operator, Type target) {
                    Object = _object;
                    Operator = _operator;
                    Target = target;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
            public class New : Expression {
                public Token Keyword { get; protected set; }
                public Type Target { get; set; }
                public List<Expression> Arguments { get; set; }
                public New(Token _operator, Type target, List<Expression> arguments) {
                    Keyword = _operator;
                    Target = target;
                    Arguments = arguments;
                }
                public object Accept(ParseTree.Visitor visitor) {
                    return visitor.Visit(this);
                }
            }
        }
    }
}
