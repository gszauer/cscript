// This document tests map fuctions

num main(string[] args) {
    num[string] inventory = new num[string](
        "bannana", 1,
        "apple", 2,
        "strawberry", 0
    );
    print(PrintMap(inventory));
    map.clear(inventory);
    print(PrintMap(inventory));

    num[string] durability = new num[string]();

    map.set(inventory, "sword", 1);
    map.set(inventory, "shield", 2);
    map.set(inventory, "boots", 1);
    print(PrintMap(inventory));

    if (map.has(inventory, "shield")) {
        print("Num shields: " + map.get(inventory, "shield"));
    }
    else {
        print ("No shields");
    }

    print ("remove shield");
    map.remove(inventory, "shield");
    if (map.has(inventory, "shield")) {
        print("Num shields: " + map.get(inventory, "shield"));
    }
    else {
        print ("No shields");
    }

    return 0;
}

string PrintMap(num[string] info) {
    string result = "";
    string[] keys_ = map.keys(info);
    for (num i = 0, size = array.length(keys_); i < size; ++i) {
        string k = keys_[i];
        result += array.at(keys_, i) + ": " + info[k];
        if (i < size - 1) {
            result += "\n";
        }
    }
    if (result == "") {
        return "Empty";
    }
    return result;
}