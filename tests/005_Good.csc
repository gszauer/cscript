struct Vector3 {
    num x;
    num y;
    num z;
}

Vector3 Vector3Cross(Vector3 a, Vector3 b) {
    Vector3 result = new Vector3();
    result.x = a.y * b.z - a.z * b.y;
    result.y = a.z * b.x - a.x * b.z;
    result.z = a.x * b.y - a.y * b.x;
    return result;
}

void main(string[] args) {
    Vector3 right   = new Vector3(1.0, 0.0, 0.0);
    Vector3 up      = new Vector3(0.0, 1.0, 0.0);
    Vector3 forward = new Vector3(0.0, 0.0, 1.0);
    Vector3 zero    = new Vector3();
    Vector3 one     = new Vector3(1.0);

    Vector3 left = Vector3Cross(up, forward);

    if (up.x < 7.0) {
        left = Vector3Cross(forward, up);
    }
    else if (up.y < 7.0) {
        left = zero;
    }
    else {
        right = left;
    }

    for (num x = 0; x < 5; ++x);
    for (num x = 5; x >= 0; --x) {
        left.x -= x;
    }
    while (true) {
        break;
    }
    while() {
        if (left.x > 5) {
            break;
        }
        else {
            continue;
        }
    }
    {
        {
            {
                num z = 5.0;
            }
        }
    }
}