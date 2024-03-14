// This document tests string fuctions

num main(string[] args) {
    num four = math.sqrt(16) * -1.0;
    four = Math.abs(four);
    print("Four: " + four);

    string display = "Hello, world";
    num firstO = string.first(display, 'o');
    num lastO = string.last(display, "o");
    num comma = string.first(display, ",");
    print("" + string.at(display, firstO) + display[comma] + display[lastO]);

    if (string.starts(display, 'H')) {
        print ("true 1");
    }

    if (string.starts(display, "Hel")) {
        print ("true 2");
    }

    if (!string.starts(display, "He3")) {
        print ("true 3");
    }

    display = "hello my love";
    string[] exploded = string.split(display, " ");
    display = array.join(exploded, "-");
    print(display);

    string x1 = "hello";
    string x2 = " world";
    display = string.concat(x1, x2);
    print(display);

    display = string.upper(display);
    print(display);
    display = string.lower(display);
    print(display);

    display = "hello my love";
	string lova = string.substring(display, 9, 4);
    print(lova);
    lova = string.replace(display, "hello", "hola");
    print(lova);


    return 0;    
}
