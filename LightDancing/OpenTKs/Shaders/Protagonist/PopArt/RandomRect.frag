vec2 high_random (in float x) {
    return vec2(fract(sin(x)*1e4 * 5.0), fract(cos(x)*1e4));
}

float high_random1 (in float x) {
    return fract(sin(x)*1e4);
}

float high_square(vec2 _st, vec2 center, vec2 size){
    vec2 square_st = step(center - size / 2.0, _st) - step(center + size / 2.0, _st);
    
    return square_st.x * square_st.y;
}


vec3 high_pop_square(vec2 _st, vec2 center, vec2 size){
    
    //circle
    float draw = high_square(_st, center, size * high_intensity);
    vec3 result;
    if(draw != 0.0)
    	result = draw * vec3(0.0001);
    
    draw = high_square(_st, center, size * 0.8 * high_intensity);
    if(draw != 0.0){
        result = draw * high_displayColor;
    }
    
    return result;
}


vec4 RandomRect(){
    
    float freq = u_time;
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.0);
    
    
    for(int i = 0; i < int(high_u_time); i++){
        vec2 position = high_random(float(i));
        vec2 size = vec2(high_random1(float(i)), high_random1(float(i + 5)));
        vec3 draw = high_pop_square(st, position, size);
        if(draw != vec3(0.0))
            color = draw;
    }
    
    return vec4(color,1.0);
}