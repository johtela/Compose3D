#version 330

in vec3 position;
in vec4 color;
in vec3 normal;

smooth out vec4 theColor;

uniform mat4 worldMatrix;
uniform mat4 perspectiveMatrix;
uniform mat3 normalMatrix;
uniform vec3 dirToLight;

void main()
{
    gl_Position = perspectiveMatrix * worldMatrix * vec4(position, 1);
	vec3 norm = normalize(normalMatrix * normal);
	float angle = dot(norm, dirToLight);
    theColor = color * angle;
}