float mid_square(vec2 _st, vec2 center, float size){
    vec2 square_st = step(center - size / 2.0, _st) - step(center + size / 2.0, _st);
    
    return square_st.x * square_st.y;
}

float mid_circle(vec2 _st, vec2 center){
    vec2 pos = _st - center;
    float length = length(pos);
    return step(length, 0.03 * mid_intensity);
}

vec3 mid_pop_square(vec2 _st, vec2 center){
    vec3 result;
    float draw = mid_square(_st, center, 0.5 * mid_intensity);
    result = draw * vec3(0.001);
    draw = mid_square(_st, center, 0.3 * mid_intensity);
    if(draw != 0.0)
    	result = draw * mid_displayColor;
    
    if(mid_intensity < 0.1){
        for(int i = 0; i < int(10.0); i++){
            for(int j = 0; j < int(10.0); j++){
                vec2 bias = vec2(0.05 * mid_intensity, (mod(float(i), 2.0) + 1.0) * 0.05 - 0.025) * mid_intensity;
                vec2 position = vec2(1.0 / 10.0 * float(i) * mid_intensity, 1.0 / 10.0 * float(j) * mid_intensity) + bias;
                if(result == mid_displayColor && mid_circle(_st, position) == 1.0)
            	    result = vec3(0.001);
            }
        }
    }
    else{
        for(int i = 0; i < int(10.0 / (mid_intensity)); i++){
            for(int j = 0; j < int(10.0 / (mid_intensity)); j++){
                vec2 bias = vec2(0.05 * mid_intensity, (mod(float(i), 2.0) + 1.0) * 0.05 - 0.025) * mid_intensity;
                vec2 position = vec2(1.0 / 10.0 * float(i) * mid_intensity, 1.0 / 10.0 * float(j) * mid_intensity) + bias;
                if(result == mid_displayColor && mid_circle(_st, position) == 1.0)
            	    result = vec3(0.001);
            }
        }
    }
    return result;
}

vec4 DotSquare(){
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