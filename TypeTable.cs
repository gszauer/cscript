using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CScript {
    class TypeTable {
        public List<string> DeclaredTypes { get; protected set; }

        public TypeId IntID { get; protected set; }
        public TypeId FloatID { get; protected set; }
        public TypeId CharID { get; protected set; }
        public TypeId BoolID { get; protected set; }
        public TypeTable(List<Pass0.Statement> statements) {
            DeclaredTypes = new List<string>();

            DeclaredTypes.Add("int");
            DeclaredTypes.Add("float");
            DeclaredTypes.Add("char");
            DeclaredTypes.Add("bool");

            IntID = new TypeId(DeclaredTypes.IndexOf("int"), "int");
            FloatID = new TypeId(DeclaredTypes.IndexOf("float"), "float");
            CharID = new TypeId(DeclaredTypes.IndexOf("char"), "char");
            BoolID = new TypeId(DeclaredTypes.IndexOf("bool"), "bool");
        }

        public bool ContainsType(string typeName) {
            return DeclaredTypes.Contains(typeName);
        }

        public TypeId GetTypeId(string typeName, Location location) {
            int index = DeclaredTypes.IndexOf(typeName);
            if (index == -1) {
                throw new CompilerException(ExceptionSource.TYPETABLE, location, "Trying to access undeclared type: " + typeName);
            }

            return new TypeId(index, DeclaredTypes[index]);
        }
    }
}
