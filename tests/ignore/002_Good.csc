struct Foo {
    num bar = 8;
}

enum Direction {
    NORTH, SOUTH, EAST, WEST = 6
}

delegate void Bar(Foo f, Direction d);

num seven = 7;
num eight = 8.0;
char cc = 'd';
string x = "yyyy";
object oo = null;
num[num[]] mappy = null;

string y;

num main(string[] args) {
    print("Hello world");
    return 0;
}

void Sub(string[] args) {

}

void Mul(num x, num y) {

}

void Dance(Foo f, Bar b) {
    num[char[]] lefsDoIt = new num[char[]](new char[]());
}
