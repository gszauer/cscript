
namespace CScript {
    class CompilerException : Exception {
        public Location Location { get; protected set; }
        public string Error {get; protected set;}
        public ExceptionSource Source { get; protected set; }

        public CompilerException(ExceptionSource source, Location location, string error) :
            base(source.ToString() + " error on line: " + location.Line + ", in file: " + location.File +
                "\nError: " + error) {
            Source = source;
            Location = location;
            Error = error;
        }

        public override string ToString() {
            return Message;
        }
    }
}
