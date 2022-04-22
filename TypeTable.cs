using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CScript {

    class CallableDeclaration {
        public Location Location { get; protected set; }
        public TypeId ReturnType { get; protected set; }
        public string Name { get; protected set; }
        public List<string> ArgumentNames { get; protected set; }
        public List<TypeId> ArgumentTypes { get; protected set; }
        //public Dictionary<string, TypeId> Arguments { get; protected set; }

        public CallableDeclaration(TypeId rType, string name, List<string> argNames, List<TypeId> argVals, Location location) {
            Location = location;
            ReturnType = rType;
            Name = name;
            ArgumentNames = argNames;
            ArgumentTypes = argVals;
            /*Arguments = new Dictionary<string, TypeId>();
            for (int i = 0; i < argNames.Count; i++) {
                Arguments[argNames[i]] = argVals[i];
            }*/
        }
    }

    class StructDeclaration {
        public Location Location { get; protected set; }
        public string Name { get; protected set; }
        public TypeId StructType { get; protected set; }
        public List<string> VariableNames { get; protected set; }
        public List<TypeId> VariableTypes { get; protected set; }

        public StructDeclaration(Token name, TypeId type, List<string> varNames, List<TypeId> varTypes) {
            Name = name.Lexeme;
            Location = name.Location;
            StructType = type;
            VariableNames = varNames;
            VariableTypes = varTypes;
        }
    }

    class TypeTable {
        protected List<string> DeclaredTypes;
        protected Dictionary<string, CallableDeclaration> Functions;
        protected Dictionary<string, StructDeclaration> Structures;
        protected Dictionary<string, CallableDeclaration> StructureConstructors;
        public TypeId VoidID { get; protected set; }
        public TypeId IntID { get; protected set; }
        public TypeId FloatID { get; protected set; }
        public TypeId CharID { get; protected set; }
        public TypeId BoolID { get; protected set; }
        public TypeId FunctionID { get; protected set; }
        public TypeId TypeID { get; protected set; }
        public TypeId StructID { get; protected set; }
        public TypeId ObjectID { get; protected set; }
        public TypeId NullID { get; protected set; }
        public TypeTable(List<Pass0.Statement> statements) { // Will ALWAYS BE PASS0
            DeclaredTypes = new List<string>();
            Functions = new Dictionary<string, CallableDeclaration>();
            Structures = new Dictionary<string, StructDeclaration>();
            StructureConstructors = new Dictionary<string, CallableDeclaration>();

            DeclaredTypes.Add("type");
            DeclaredTypes.Add("object");
            DeclaredTypes.Add("null");
            DeclaredTypes.Add("void");
            DeclaredTypes.Add("int");
            DeclaredTypes.Add("float");
            DeclaredTypes.Add("char");
            DeclaredTypes.Add("bool");

            DeclaredTypes.Add("$struct"); // For later so i don't forget
            // Register all struct types
            foreach (Pass0.Statement statement in statements) {
                if (statement is Pass0.StructDeclStatement) {
                    Pass0.StructDeclStatement structDecl = (Pass0.StructDeclStatement)statement;
                    string structName = structDecl.Name.Lexeme;
                    if (DeclaredTypes.Contains(structName)) {
                        throw new CompilerException(ExceptionSource.TYPETABLE, statement.Location, "Trying to re-declare type with struct: " + structName);
                    }
                    DeclaredTypes.Add(structName);
                }
            }
            // Collect struct type info
            foreach (Pass0.Statement statement in statements) {
                if (statement is Pass0.StructDeclStatement) {
                    Pass0.StructDeclStatement structDecl = (Pass0.StructDeclStatement)statement;
                    string structName = structDecl.Name.Lexeme;

                    List<string> varNames = new List<string>();
                    List<TypeId> varTypes = new List<TypeId>();

                    foreach(Pass0.VarDeclStatement var in structDecl.Variables) {
                        if (varNames.Contains(var.Name.Lexeme)) {
                            throw new CompilerException(ExceptionSource.TYPETABLE, var.Name.Location, "Duplicate variable " + var.Name.Lexeme + " in struct " + structName);
                        }
                        varTypes.Add(GetTypeId(var.Type.Lexeme, var.Type.Location));
                        varNames.Add(var.Name.Lexeme); 
                    }

                    StructDeclaration decl = new StructDeclaration(structDecl.Name, new TypeId(DeclaredTypes.IndexOf(structName), structName), varNames, varTypes);
                    Structures[structName] = decl;

                    CallableDeclaration construct = new CallableDeclaration(decl.StructType, structName, new List<string>(), new List<TypeId>(), statement.Location);
                    StructureConstructors[structName] = construct;
                }
            }

            // This way you can query a struct if typeid is between $struct and $function
            // Similarly, anything past $function will be a user function. Pretty clever.
            DeclaredTypes.Add("$function");

            IntID = new TypeId(DeclaredTypes.IndexOf("int"), "int");
            FloatID = new TypeId(DeclaredTypes.IndexOf("float"), "float");
            CharID = new TypeId(DeclaredTypes.IndexOf("char"), "char");
            BoolID = new TypeId(DeclaredTypes.IndexOf("bool"), "bool");
            FunctionID = new TypeId(DeclaredTypes.IndexOf("$function"), "$function");
            VoidID = new TypeId(DeclaredTypes.IndexOf("void"), "void");
            TypeID = new TypeId(DeclaredTypes.IndexOf("type"), "type");
            StructID = new TypeId(DeclaredTypes.IndexOf("$struct"), "$struct");
            ObjectID = new TypeId(DeclaredTypes.IndexOf("object"), "object");
            NullID = new TypeId(DeclaredTypes.IndexOf("null"), "null");

            // Loop trough all functions and register their unique names here
            foreach (Pass0.Statement statement in statements) {
                if (statement is Pass0.FunDeclStatement) {
                    Pass0.FunDeclStatement funDecl = (Pass0.FunDeclStatement)statement;
                    TypeId returnType = GetTypeId(funDecl.ReturnType.Lexeme, funDecl.ReturnType.Location);

                    List<string> argNames = new List<string>();
                    List<TypeId> argTypes = new List<TypeId>();

                    foreach (Pass0.FunParamater param in funDecl.Paramaters) {
                        argNames.Add(param.Name.Lexeme);
                        argTypes.Add(GetTypeId(param.Type.Lexeme, param.Type.Location));
                    }

                    CallableDeclaration function = new CallableDeclaration(returnType, funDecl.Name.Lexeme, argNames, argTypes, funDecl.Location);
                    Functions.Add(funDecl.Name.Lexeme, function);
                }
            }
        }

        public bool ContainsType(string typeName) {
            return DeclaredTypes.Contains(typeName);
        }
        public TypeId GetTypeId(string typeName, Location location) {
            if (Functions.ContainsKey(typeName)) {
                return FunctionID;
            }

            int index = DeclaredTypes.IndexOf(typeName);
            if (index == -1) {
                throw new CompilerException(ExceptionSource.TYPETABLE, location, "Trying to access undeclared type: " + typeName);
            }

            return new TypeId(index, DeclaredTypes[index]);
        }

        public bool IsValidVariableType(TypeId type) {
            if (type.Index > StructID.Index && type.Index < FunctionID.Index) {
                return true;
            }

            if (type == IntID || type == FloatID || type == CharID || type == BoolID || type == TypeID || type == ObjectID) {
                return true;
            }

            return false;
        }

        public bool CanAssign(TypeId leftType, TypeId rightType) {
            if (rightType == NullID) {
                return CanBeNull(leftType);
            }
            if (leftType == ObjectID) {
                if (rightType == TypeID) { // Can't assign type to object. I don't want to cast.
                    return true;
                }
                if (rightType == ObjectID) {
                    return true;
                }
                return IsValidVariableType(rightType);
            }
            return leftType == rightType;
        }
        public bool IsCallable(TypeId type) {
            return type.Index == FunctionID.Index || type.Index == StructID.Index;
        }
        public bool IsStruct(TypeId type) {
            return type.Index > StructID.Index && type.Index < FunctionID.Index;
        }

        public bool CanBeNull(TypeId type) {
            if (IsStruct(type)) {
                return true;
            }
            if (type == ObjectID) {
                return true;
            }
            return false;
        }
        public List<string> GetBuiltInTypeNames() {
            List<string> result = new List<string>();
            bool skip = false;
            foreach(string type in DeclaredTypes) {
                if (type == "$struct") {
                    break;
                }
                result.Add(type);
            }
            result.Add("$struct");
            result.Add("$function");
            return result;
        }

        public List<string> GetFunctionNames() {
            List<string> result = new List<string>();
            foreach (var kvp in Functions) {
                result.Add(kvp.Key);
            }
            return result;
        }

        public List<string> GetStructureNames() {
            List<string> result = new List<string>();
            foreach (var kvp in Structures) {
                result.Add(kvp.Key);
            }
            return result;
        }
        public StructDeclaration GetStruct(TypeId type, Location location) {
            if (!Structures.ContainsKey(type.DebugName)) {
                throw new CompilerException(ExceptionSource.TYPETABLE, location, "Trying to access undeclared struct type: " + type.DebugName);
            }
            return Structures[type.DebugName];
        }

        public CallableDeclaration GetCallable(string name, TypeId type, Location location) {
            if (type.Index != FunctionID.Index && type.Index != StructID.Index) {
                throw new CompilerException(ExceptionSource.TYPETABLE, location, "Only functions and structs are callable, not: " + type.DebugName);
            }
            if (Functions.ContainsKey(name)) {
                return Functions[name];
            }
            if (StructureConstructors.ContainsKey(name)) {
                return StructureConstructors[name];
            }
            throw new CompilerException(ExceptionSource.TYPETABLE, location, "Invalid callable: " + type.DebugName);
        }
    }
}
