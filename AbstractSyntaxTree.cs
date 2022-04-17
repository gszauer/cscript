namespace CScript {
    class AbstractSyntaxTree {
        public List<Pass0.Statement> Program { get; protected set; }
        public AbstractSyntaxTree(List<Pass0.Statement> program) {
            Program = program;
        }
    }
}
