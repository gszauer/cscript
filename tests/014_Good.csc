// This document tests vec3 fuctions

num main(string[] args) {
    vec3 right = new vec3(1, 0, 0);
    vec3 up = new vec3(0, 1, 0);
    vec3 forward = new vec3(0, 0, 1);

    vec3 half = vec3.lerp(right, up, 0.5);

    print("right: " + right);
    print ("up: " + (up as string));
    print ("forward: " + forward);
    print ("half: " + half as string);
    print ("dot: " + vec3.dot(right, up));

    vec3 a = up + right;
    vec3 b = up - right;
    vec3 c = up * right;
    vec3 e = up / right;
    vec3 d = up * 0.5;

    bool same = up == right;
    bool thisTime = up == new vec3(0, 1, 0);
    bool hammerTime = up != new vec3(0, 1, 0);
    if (same) {
        print("wrong");
    }
    else if (thisTime) {
        print("right!");
    }
    else {
        print("wrong");
    }

    return 0;    
}
