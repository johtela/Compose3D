#version 330

in vec3 position;
in vec3 color;

smooth out vec4 theColor;

uniform mat4 worldMatrix;
uniform mat4 perspectiveMatrix;

void main()
{
    gl_Position = perspectiveMatrix * (worldMatrix * vec4(position, 1.0));
    theColor = vec4(color, 1.0);
}