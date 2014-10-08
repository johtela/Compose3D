varying vec2 texCoord;
varying float scale;
vec3 hsv(float h);
void main() {
    vec2 z = vec2(0.0);
    vec2 c = texCoord;
    float iter = 0.0;
    float step = pow(scale-0.00004,-0.3)/800.0 ;
    for(iter=0.0; iter<1.0; iter+=step) {
        float t = z.x;
        z.x = z.x*z.x - z.y*z.y + c.x;
        z.y = 2.0*t*z.y + c.y;
        if(z.x*z.x+z.y*z.y>4.0) break;
    }
 float mixfactor = pow(iter,pow(scale+0.01,0.1)/0.9);
    gl_FragColor = vec4(hsv(mixfactor),1);
}

