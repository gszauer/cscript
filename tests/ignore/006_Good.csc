struct Vector3 {
    num x;
    num y = 1.0;
    num z;

    string debug = "none";
}

Vector3 Vector3Cross(Vector3 a, Vector3 b) {
    Vector3 result = new Vector3();
    result.x = a.y * b.z - a.z * b.y;
    result.y = a.z * b.x - a.x * b.z;
    result.z = a.x * b.y - a.y * b.x;
    return result;
}

object AAAA () {
    return null;
}

Vector3 bBBBB() {
    return null;
}

void main(string[] args) {
    Vector3 up = new Vector3(0, 1, 0);
    Vector3 right = new Vector3(0, 0, 1);

    Vector3Cross(null, Vector3Cross(up, right));

    num[] numbers = new num[]();

    Vector3[] allVecs = new Vector3[](
        new Vector3(0, 1, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, 1, 0),
        up
    );
    
    array.add(allVecs, new Vector3(9, 5, 9));
    array.add(allVecs, new Vector3(9, 5, 9));

    Vector3[] allVecs1 = new Vector3[](
        up,
        up,
        null,
        up
    );
}