vec2 high_random (in float x) {
    return vec2(fract(sin(x)*1e4 * 5.0), fract(cos(x)*1e4));
}

float high_random1 (in float x) {
    return fract(sin(x)*1e4);
}

float plot(vec2 _st, vec2 center, float wave){
    float y = step(wave - 0.05 + center.y, _st.y) - step(wave + 0.05 + center.y, _st.y);
    float x = step(center.x - 0.15 * high_intensity, _st.x) - step(center.x + 0.15 * high_intensity, _st.x);
    
    return x * y;
}

mat2 high_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec4 RandomCurve(){
    
    float freq = u_time;
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.0);
    
    
    
    for(int i = 0; i <int(high_u_time); i++){
        vec2 position = high_random(float(i));
        float wave = 30.0 * high_random1(float(i));
    	float y = mix(sin(floor(st.x * wave) * PI / 2.0), sin(floor(st.x * wave + 1.0) * PI / 2.0), fract(st.x * wave)) * 0.05;
        
        vec2 rotate_st = st - 0.5;
        rotate_st *= high_rotate2d(high_random1(float(i)) * 2.0 * PI);
        rotate_st += 0.5;
        float draw = plot(rotate_st, position, y);
        vec3 displayColor;
        if(mod(float(i), 3.0) == 0.0)
            displayColor = high_displayColor.zyx;
        else if(mod(float(i), 3.0) == 1.0)
            displayColor = high_displayColor.xxz;
        else if(mod(float(i), 3.0) == 2.0)
            displayColor = high_displayColor.yxz;
        
        if(draw != 0.0)
            color = draw * displayColor;
    }
    
    
    return vec4(color,1.0);
}