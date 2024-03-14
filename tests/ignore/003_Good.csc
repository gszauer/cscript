
struct Foo {
    num bar = 8;
}

enum Direction {
    NORTH, SOUTH, EAST, WEST = 6
}


num main(string[] args) {
    print("Direction = " + Direction.NORTH);
    char bling = ',';
    num eight = 8 + 5;
    num seven = eight - 1;
    return 0;
}

char Error() {
    return null as char;
}

num main2() {
    return 8;
}