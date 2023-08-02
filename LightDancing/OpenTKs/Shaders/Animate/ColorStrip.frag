float strip(vec2 _st, float center){
    return step(center - 0.1, _st.x) - step(center + 0.1, _st.x);
}

vec4 ColorStrip() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 displayColor;
    if(mod(floor(u_time), 8.0) == 0.0)
        displayColor = color_1;
    else if(mod(floor(u_time), 8.0) == 1.0)
        displayColor = color_2;
    else if(mod(floor(u_time), 8.0) == 2.0)
        displayColor = color_3;
    else if(mod(floor(u_time), 8.0) == 3.0)
        displayColor = color_4;
    else if(mod(floor(u_time), 8.0) == 4.0)
        displayColor = color_5;
    else if(mod(floor(u_time), 8.0) == 5.0)
        displayColor = color_6;
    else if(mod(floor(u_time), 8.0) == 6.0)
        displayColor = color_7;
    else if(mod(floor(u_time), 8.0) == 7.0)
        displayColor = color_8;
  
    float center = mod(u_time, 2.0);
    vec3 color = strip(st, abs(1.0 - center) * 0.8 + 0.1) * displayColor;
    
    return vec4(color,1.0);
}