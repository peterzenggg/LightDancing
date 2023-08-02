float mid_square(vec2 _st, vec2 center, float size){
    vec2 square_st = step(center - size / 2.0, _st) - step(center + size / 2.0, _st);
    
    return square_st.x * square_st.y;
}

vec3 mid_pop_square(vec2 _st, vec2 center){
    vec3 result;
    float draw = mid_square(_st, center, 0.5 * mid_intensity);
    result = draw * vec3(0.001);
    draw = mid_square(_st, center, 0.3 * mid_intensity);
    if(draw != 0.0)
    	result = draw * mid_displayColor;
    return result;
}

vec4 FourSquare(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.0);
    
    float index = floor(mod(mid_u_time, 4.0));
    vec2 position = vec2(mod(index, 2.0) * 2.0, floor(index / 2.0) * 2.0) * 0.25 + 0.25;
    
    vec3 draw = mid_pop_square(st, position);
    if(draw != vec3(0.0)){
        color = draw;
    }
    
    return vec4(color,1.0);
}