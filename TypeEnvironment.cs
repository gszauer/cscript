using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CScript {
    class TypeEnvironment {
        protected Dictionary<string, TypeId> mDeclaredTypes;
        public TypeEnvironment mParent;

        public TypeEnvironment(TypeEnvironment parent) {
            mParent = parent;
            mDeclaredTypes = new Dictionary<string, TypeId>();
        }

        public void DeclareVariableType(string name, TypeId type, Location location) {
            if (mDeclaredTypes.ContainsKey(name)) {
                throw new CompilerException(ExceptionSource.TYPECHECKER, location, "Trying to re-declare type: " + type.DebugName);
            }
            mDeclaredTypes[name] = type;
        }

        public TypeId GetVariableType(string name, Location location) {
            if (!mDeclaredTypes.ContainsKey(name)) {
                if (mParent != null) {
                    return mParent.GetVariableType(name, location);
                }
                throw new CompilerException(ExceptionSource.TYPECHECKER, location, "Trying to access undeclared type: " + name);
            }
            return mDeclaredTypes[name];
        }
    }
}
