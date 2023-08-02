vec2 high_random (in float x) {
    return vec2(fract(sin(x)*1e4 + 0.2), fract(cos(x)*1e4));
}

float high_circle(vec2 _st, vec2 center, float radius){
    vec2 pos = _st - center;
    float length = length(pos);
    
    return step(length, radius * high_intensity);
    
}

vec3 high_rip(vec2 _st, vec2 center){
    vec3 result;
    result = (high_circle(_st, center, 0.07) - high_circle(_st, center, 0.04)) * high_displayColor * high_intensity;
    if(high_circle(_st, center, 0.02) != 0.0)
        	result = high_circle(_st, center, 0.02) * high_displayColor.zyx * high_intensity;
    
    return result;
}

vec4 CartoonProtagonist4(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    for(int i = 0; i < int(high_u_time); i++){
        vec2 position = high_random(float(i));

        if(high_rip(st, position) != vec3(0.0))
        	color = high_rip(st, position);
    }
    
    return vec4(color,1.0);
}