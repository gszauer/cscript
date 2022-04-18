namespace CScript {
    class AbstractSyntaxTree {
        public List<Pass1.Statement> Program { get; protected set; }
        public TypeTable Types { get; protected set; }
        public AbstractSyntaxTree(List<Pass1.Statement> program, TypeTable types) {
            Program = program;
            Types = types;
        }
    }
}
