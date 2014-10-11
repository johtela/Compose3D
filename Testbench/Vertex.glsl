#version 330

in vec3 position;
in vec3 color;

smooth out vec4 theColor;

uniform float loopDuration;
uniform float time;

void main()
{
    float timeScale = 3.14159f * 2.0f / loopDuration;
    
    float currTime = mod(time, loopDuration);
    vec4 totalOffset = vec4(
        cos(currTime * timeScale) * 0.5f,
        sin(currTime * timeScale) * 0.5f,
        0.0f,
        0.0f);
    gl_Position = vec4(position, 1.0) + totalOffset;
    theColor = vec4(color, 1.0);
}