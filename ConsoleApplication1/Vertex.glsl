varying vec2 texCoord;
varying float scale;
void main() {
    gl_Position = ftransform();
    texCoord = gl_Vertex.xy;
    scale = gl_ModelViewMatrix[2][2]/4000;
}

