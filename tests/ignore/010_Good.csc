// This document tests expressions

struct Game {
    num level;
    num lives;
}


num main(string[] args) {
    // Grouping
    num seven = (2 * (3 - 1)) + 3;

    // Getter
    seven = 9;
    Game g = new Game(1, 7);
    print ("Level: " + g.level + ", Lives: " + g.lives);

    return 0;
}