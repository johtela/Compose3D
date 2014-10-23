#version 330

out vec4 outputColor;
smooth in vec4 theColor;

void main()
{
    outputColor = theColor;
}