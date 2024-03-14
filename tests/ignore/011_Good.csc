// This document tests array fuctions

struct Game {
    num level;
    num lives;
}

num main(string[] args) {
    Game[] saves = new Game[](new Game(1, 2), new Game(2, 3));

    print("Num saves: " + array.length(saves));
    print("Save 0 level: " + saves[0].level + ", Save 0 lives: " + saves[0].lives);
    print("Save 1 level: " + saves[1].level + ", Save 1 lives: " + saves[1].lives);

    string[] fruits = new string[]("apple", "bannana", "orange", "bannana", "grape");
    string out = "Fruits: ";
    for (num i = 0, size = array.length(fruits); i < size; ++i) {
        out += fruits[i];
        if (i < size - 1) {
            out += ", ";
        }
    }
    print("\n" + out);    
    print ("First instance of \"bannana\": " + array.first(fruits, "bannana"));
    print ("Last instance of \"bannana\": " + array.last(fruits, "bannana"));
    print("[0] = " + array.at(fruits, 0) + ", [1] = " + array.at(fruits, 1));
    print("[2] = " + fruits[2] + ", [3] = " + fruits[3]);


    string[] veggies = new string[]("Carrots", "Broccoli", "ASparagus");

    string[] food = array.concat(fruits, veggies);
    out = "\nFood: \n\t";
    for (num i = 0, size = array.length(food); i < size; ++i) {
        out += food[i];
        if (i < size - 1) {
            out += ", ";
        }
    }
    print(out);    

	out = "\nFruits: \n\t" + array.join(fruits, ", ");
    print(out);    

    print ("removing grape: " + array.pop(fruits));
    out = "\nFruits: \n\t" + array.join(fruits, ", ");
    print(out);  

    print ("removing apple: " + array.shift(fruits));
    out = "\nFruits: \n\t" + array.join(fruits, ", ");
    print(out);  

	string[] names = new string[]("john", "kate", "marry", "sue", "jim", "bob");
    out = "\nNames: " + array.join(names, ", ");
    print(out);  
    string[] boyNames = array.concat(array.slice(names, 0, 1), array.slice(names, 4, 2));
    out = "\nBoys: " + array.join(boyNames, ", ");
    print(out); 

    array.sort(names, null);
    out = "\nSorted: " + array.join(names, ", ");
    print(out);  

    array.sort(names, ReverseSort);
    out = "\nReversed: " + array.join(names, ", ");
    print(out);  

    array.reverse(names);
    out = "\nSorted: " + array.join(names, ", ");
    print(out);  

    array.remove(names, 1, 2);
    out = "\nBye jim and john: " + array.join(names, ", ");
    print(out);  

    array.add(names, "jim");
    array.insert(names, 1, "john");
    out = "\nWelcome back: " + array.join(names, ", ");
    print(out); 

    string[] names2 = array.copy(names);
    array.sort(names2, null);
    out = "\nOriginal: " + array.join(names, ", ");
    out += "\nCopy: " + array.join(names2, ", ");
    print(out); 

    print("names1: " + array.length(names) + ", names2: " + array.length(names2));
    array.clear(names2);
    print("names1: " + array.length(names) + ", names2: " + array.length(names2));
    
    return 0;
}

num ReverseSort(string left, string right) {
    return 1;
}