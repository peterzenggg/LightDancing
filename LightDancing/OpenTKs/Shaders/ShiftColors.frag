#version 330 
#ifdef GL_FRAGMENT_PRECISION_HIGH
    precision highp float;
#else
    precision mediump float;
#endif

#define PI 3.14159265358979323846
uniform vec2 u_resolution;
varying float pass;


void main(void){
    vec2 st = vec2(gl_FragCoord.x / u_resolution.x, gl_FragCoord.y / u_resolution.y);
    
    st.x *= 3.0;
    st.y *= 2.0;
    
    float index = mod(floor(st.x) + floor(st.y) * 6.0 - (floor(st.x)*2.0+1.0) * floor(st.y) + floor(pass), 6.0);
    
    vec3 color = vec3(0.0);
    if(index == 0.0){
        color += vec3(1.0, 1.0, 0.0);
    }
    else if(index == 1.0){
        color += vec3(0.0, 0.0, 1.0);
    }
    else if(index == 2.0){
        color += vec3(1.0, 0.5, 0.0);
    }
    else if(index == 3.0){
        color += vec3(1.0, 0.0, 0.0);
    }
    else if(index == 4.0){
        color += vec3(0.0, 1.0, 0.0);
    }
    else if(index == 5.0){
        color += vec3(1.0, 0.0, 1.0);
    }
    
    gl_FragColor = vec4(color,1.0);
}