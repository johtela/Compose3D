vec3 hsv(float h) {
    if(h==0.0) return vec3(0.0,0.0,0.0);
    float h6 = h*6.0;
    float x = 1.0 - abs(mod(h6,2.0)-1.0);
    if(h6<1.0) return vec3(1.0,x,0.0);
    if(h6<2.0) return vec3(x,1.0,0.0);
    if(h6<3.0) return vec3(0.0,1.0,x);
    if(h6<4.0) return vec3(0.0,x,1.0);
    if(h6<5.0) return vec3(x,0.0,1.0);
    return vec3(1.0,0.0,x);
}

