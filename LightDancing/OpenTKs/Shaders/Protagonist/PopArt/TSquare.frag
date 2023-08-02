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

vec3 high_pop_square(vec2 _st, vec2 center, float size){
    vec3 result = vec3(0.0);
    float draw = high_square(_st, center, vec2(1.0)  * size);
    if(draw != 0.0)
    	result = draw * vec3(0.001);
    
    draw = high_square(_st, center, vec2(0.8)  * size);
    if(draw != 0.0)
    	result = draw * high_displayColor;
    // -
    vec2 position = center + vec2(0.0, center.y * 0.4) * size;
    draw = high_square(_st, position, vec2(0.6, 0.2) * size);
    if(draw != 0.0)
        result = draw * vec3(0.001);
    draw = high_square(_st, position, vec2(0.5, 0.1) * size);
    if(draw != 0.0)
        result = draw * high_displayColor.zyx;
    // |
    position = center - vec2(0.0, center.y * 0.2) * size;
    draw = high_square(_st, position, vec2(0.2, 0.5) * size);
    if(draw != 0.0)
        result = draw * vec3(0.001);
    draw = high_square(_st, position, vec2(0.1, 0.4) * size);
    if(draw != 0.0)
        result = draw * high_displayColor.xxz;
    return result;
}


vec4 TSquare(){
    
    float freq = u_time;
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    
    
    int i = int(floor(high_u_time));
    vec2 position = high_random(float(i));
    float size = high_random1(float(i));
    vec3 draw = high_pop_square(st, position, high_intensity);
    if(draw != vec3(0.0))
        color = draw;
    
    
    return vec4(color,1.0);
}