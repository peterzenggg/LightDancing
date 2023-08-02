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

float high_circle(vec2 _st, vec2 center, float size){
    vec2 pos = _st - center;
    float length = length(pos);
    
    return step(length, size * high_intensity);
}

vec3 high_pop_square(vec2 _st, vec2 center, float size){
    
    //circle
    float draw = high_circle(_st, center, size * 0.5 / 2.0);
    vec3 result;
    if(draw != 0.0)
    	result = draw * vec3(0.0001);
    
    draw = high_circle(_st, center, size * 0.35 / 2.0);
    if(draw != 0.0){
        if(_st.y > center.y){
            result = draw * high_displayColor.zyx;
        }
        else{
            result = draw * high_displayColor.xxz;
        }
    }
    
    draw = high_square(_st, center, vec2(0.35, 0.1) * high_intensity  * size);
    if(draw != 0.0)
    	result = draw * vec3(0.001);
    
    	
    
    return result;
}


vec4 TwoSideCircle(){
    
    float freq = u_time;
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.0);
    
    
    int i = int(floor(high_u_time));
    vec2 position = high_random(float(i));
    float size = high_random1(float(i));
    vec3 draw = high_pop_square(st, position, high_intensity);
    if(draw != vec3(0.0))
        color = draw;
    
    
    return vec4(color,1.0);
}