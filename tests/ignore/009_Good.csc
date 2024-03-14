struct Vector3 {
    num x;
    num y;
    num z;
}

enum Directions {
    NORTH,
    SOUTH = 4,
    EAST,
    WEST
}

delegate void Del();

num globalSeven = 8;
num globalZero;

Directions defDir = Directions.NORTH;
Vector3 worldUp = new Vector3(0, 1, 0);

num main(string[] args) {
    num xxx = 666;

    if (xxx > 5.0) {
        worldUp = new Vector3(0, -1, 0);
    }

    if (5 > 6) {
        num seven = 8;
    }
    else if (8 < 7) {
        bool never = true;
    }

    if ('c' as num < 36) {
        num seven = 8;
    }
    else if (false) { }
    else {
        string hello = "world";
    }

    while (true) {
        print("TrUe");
    }

    for (;;) {

    }

    for (num x = 0;;);

    for (num y = 0, z = 9; y < 3; ++y) {
        print(y as string);
    }
    return 0;
}