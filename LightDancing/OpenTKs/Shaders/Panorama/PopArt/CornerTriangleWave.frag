float mid_square(vec2 _st, vec2 center, float size){
    vec2 square_st = step(center - size / 2.0, _st) - step(center + size / 2.0, _st);
    
    return square_st.x * square_st.y;
}

float mid_plot(vec2 _st, float y, float height){
    float size = 0.1;
    return step(y - size / 2.0 + height, _st.y) - step(y + size / 2.0 + height, _st.y);
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
    
    //Wave
    float y = mix(sin(floor(_st.x * 10.0) * PI / 2.0), sin(floor(_st.x * 10.0 + 1.0) * PI / 2.0), fract(_st.x * 10.0)) * 0.05;
    
    for(int i = 0; i < 5; i++){
        float height = 1.0 / 5.0 * float(i) + 0.1;
        if(result != vec3(0.0) &&  mid_plot(_st, y, fract(height)) != 0.0)
            result = mid_plot(_st, y, fract(height)) * vec3(0.0001);
    }
    return result;
}

vec4 CornerTriangleWave(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.0);
    
    float index = floor(mod(mid_u_time, 4.0));
    vec2 position = vec2(mod(index, 2.0), floor(index / 2.0));
    vec2 rotate_st = st - position;
    if(mod(position.x + position.y, 2.0) == 0.0)
        rotate_st *= mid_rotate2d(PI / 4.0);
    else
    	rotate_st *= mid_rotate2d(-PI / 4.0);
    rotate_st += position;
    vec3 draw = mid_pop_square(rotate_st, position);
    if(draw != vec3(0.0)){
        color = draw;
    }
    
    return vec4(color,1.0);
}