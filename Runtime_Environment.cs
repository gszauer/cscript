using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CScript {

    namespace Runtime {
        class Environment {
            Environment Parent;
            Dictionary<string, object> Variables;
            public Environment(Environment parent) {
                Parent = parent;
                Variables = new Dictionary<string, object>();
            }
            public object Get(string name, Location location) {
                if (!Variables.ContainsKey(name)) {
                    if (Parent != null) {
                        return Parent.Get(name, location);
                    }
                    throw new InterpreterException(location, "Trying to access undeclared variable: " + name);
                }
                return Variables[name];
            }

            public void Declare(string name, Location location) {
                if (Variables.ContainsKey(name)) {
                    throw new InterpreterException(location, "Trying to re-declare variable: " + name);
                }
                Variables[name] = null;
            }

            public void Set(string name, object value, Location location) {
                if (!Variables.ContainsKey(name)) {
                    if (Parent != null) {
                        Parent.Set(name, value, location);
                        return;
                    }
                    throw new InterpreterException(location, "Trying to set an undeclared declare variable: " + name);
                }
                Variables[name] = value;
            }
        }
    }
}
