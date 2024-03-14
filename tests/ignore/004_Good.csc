num[] vertices = new num[](
  // Front face
  -1.0, -1.0, 1.0, 1.0, -1.0, 1.0, 1.0, 1.0, 1.0, -1.0, 1.0, 1.0,

  // Back face
  -1.0, -1.0, -1.0, -1.0, 1.0, -1.0, 1.0, 1.0, -1.0, 1.0, -1.0, -1.0,

  // Top face
  -1.0, 1.0, -1.0, -1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, -1.0,

  // Bottom face
  -1.0, -1.0, -1.0, 1.0, -1.0, -1.0, 1.0, -1.0, 1.0, -1.0, -1.0, 1.0,

  // Right face
  1.0, -1.0, -1.0, 1.0, 1.0, -1.0, 1.0, 1.0, 1.0, 1.0, -1.0, 1.0,

  // Left face
  -1.0, -1.0, -1.0, -1.0, -1.0, 1.0, -1.0, 1.0, 1.0, -1.0, 1.0, -1.0,
);

num numComponents = 3;

num[][] faceColors = new num[][](
  new num[](1.0, 1.0, 1.0, 1.0), // Front face: white
  new num[](1.0, 0.0, 0.0, 1.0), // Back face: red
  new num[](0.0, 1.0, 0.0, 1.0), // Top face: green
  new num[](0.0, 0.0, 1.0, 1.0), // Bottom face: blue
  new num[](1.0, 1.0, 0.0, 1.0), // Right face: yellow
  new num[](1.0, 0.0, 1.0, 1.0), // Left face: purple
);

void main() {
  num vert0 = vertices[0];

  num seventeen = lookup["seventeen"];
}

num[string] lookup = new num[string](
  "seven", 7.0, "t", 6.0, "string" 3.0
);