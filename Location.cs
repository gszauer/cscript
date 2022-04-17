namespace CScript {
    struct Location {
        public int Line;
        public string File;

        public Location() {
            Line = -2;
            File = "Default location";
        }
        public Location(int line, string file) {
            Line = line;
            File = file;
        }
    }
}
