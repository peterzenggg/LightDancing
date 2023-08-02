float mid_square(vec2 _st, vec2 center, float size){
    vec2 square_st = step(center - size / 2.0, _st) - step(center + size / 2.0, _st);
    
    return square_st.x * square_st.y;
}

float mid_circle(vec2 _st, vec2 center, float size){
    vec2 pos = _st - center;
    float length = length(pos);
    
    return step(length, size);
}

mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec3 mid_pop_square(vec2 _st, vec2 center){
    vec3 result;
    float draw = mid_square(_st, center, 2.0 * mid_intensity);
    result = draw * vec3(0.001);
    draw = mid_square(_st, center, 1.6 * mid_intensity);
    if(draw != 0.0)
    	result = draw * mid_displayColor;
    
    //Circles
    for(int i = -2; i <= 2; i++){
        for(int j = -2; j <= 2; j++){
            vec2 position = center + vec2(i, j) * 0.3 * mid_intensity;
            draw = mid_circle(_st, position, 0.1 * mid_intensity);
            if(draw != 0.0 && result != vec3(0.0))
            	result = draw * vec3(0.001);   
        }
    }
    return result;
}

vec4 CornerDot(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.0);
    
    float index = floor(mod(mid_u_time, 4.0));
    vec2 position = vec2(mod(index, 2.0), floor(index / 2.0));
    vec2 rotate_st = st - position;
    rotate_st *= mid_rotate2d(PI / 4.0);
    rotate_st += position;
    vec3 draw = mid_pop_square(rotate_st, position);
    if(draw != vec3(0.0)){
        color = draw;
    }
    
    return vec4(color,1.0);
}