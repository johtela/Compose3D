#version 330

in vec3 position;
in vec3 color;

smooth out vec4 theColor;

uniform float loopDuration;
uniform float time;
uniform mat4 perspectiveMatrix;

void main()
{
    float timeScale = 3.14159f * 2.0f / loopDuration;
    
    float currTime = mod(time, loopDuration);
    vec4 totalOffset = vec4(
        cos(currTime * timeScale),
        sin(currTime * timeScale),
        sin(currTime * timeScale),
        0.0f);
    gl_Position = perspectiveMatrix * (vec4(position, 1.0) + totalOffset);
    theColor = vec4(color, 1.0);
}