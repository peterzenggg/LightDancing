vec2 mid_random (in float x) {
    return vec2(fract(sin(x)*1e4), fract(cos(x)*1e4));
}

float mid_circle(vec2 _st, vec2 _center, float radius){
    vec2 pos = _st - _center;
    float length = length(pos);
    return step(radius, length);
}
float mid_ripple(vec2 _st, vec2 _center){
    float result;
    
        
    
    for(int i = 0; i < int(mid_intensity * 10.0); i++){
        float radius = 0.3 * mid_intensity / (mid_intensity * 10.0) * float(i);
        if(mod(float(i), 2.0) == 0.0){
            result += mid_circle(_st, _center, radius);
        }
        else{
            result -= mid_circle(_st, _center, radius);
        }
    }

    if(int(mod(mid_intensity * 10.0, 2.0)) == 1){
        result -= mid_circle(_st, _center, 0.3 * mid_intensity);
    }
        
    
    return result;
}

vec4 Ripple(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(.0);
	
    for(int i = int(mid_u_time); i < 5 + int(mid_u_time); i++){
        vec2 position = mid_random(float(i));
    	color += mid_ripple(st, position) * mid_displayColor;
    }
    
    return vec4(color,1.0);
}