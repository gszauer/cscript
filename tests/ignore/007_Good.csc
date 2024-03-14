struct Vector3 {
    num x;
    num y;
    num z;
}

struct Quaternion {
    num w;
    num x;
    num y;
    num z;
}

struct Transform {
    Vector3 position;
    Quaternion rotation;
    Vector3 scale;
    string name;
}

Transform[Transform] Transform_Parent       = new Transform[Transform]();
Transform[Transform] Transform_FirstChild   = new Transform[Transform]();
Transform[Transform] Transform_NextSibling  = new Transform[Transform]();

void main(string[] args) {
    Transform rootTransform = new Transform();
    Transform rootTransform1 = new Transform(null);
    Transform aTransform = new Transform(new Vector3(0, 1, 0));
    Transform bTransform = new Transform(new Vector3(), new Quaternion());
    Transform cTransform = new Transform(new Vector3(0), new Quaternion(), new Vector3(1, 1, 1));
    rootTransform.name = "Root";

    Transform[string] allTransforms = new Transform[string](
        "Root", rootTransform,
        "A", aTransform,
    );
}