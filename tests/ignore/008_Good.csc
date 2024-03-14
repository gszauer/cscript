delegate void _fnPrint(string arg);
_fnPrint print_ = null;

void main(string[] args) {
    string input = "HEllo World";
    print_(string.lower(input));
    char x = string.at(input, 6);
}