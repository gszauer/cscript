
namespace CScript {
    namespace Runtime {
        class InterpreterException : Exception {
            public Location Location { get; protected set; }
            public string Error { get; protected set; }

            public InterpreterException(Location location, string error) :
                base("Runtime error on line: " + location.Line + ", in file: " + location.File +
                    "\nError: " + error) {
                Location = location;
                Error = error;
            }

            public override string ToString() {
                return Message;
            }
        }
    }
}
