vec2 mid_random (in float x) {
    return vec2(fract(sin(x)*1e4), fract(cos(x)*1e4));
}

float mid_circle(vec2 _st, vec2 _center, float radius){
    vec2 pos = _st - _center;
    float length = length(pos);
    return step(length, radius);
}

vec3 mid_ripple(vec2 _st, vec2 _center){
    vec3 color = vec3(0.0);
    float result;

    for(int i = int(mid_intensity * 10.0); i > 0 ; i--){
        
        float radius = 0.2 * mid_intensity / (mid_intensity * 10.0) * float(i);
        
        if(mod(float(i), 2.0) == 0.0){
            result = mid_circle(_st, _center, radius + 0.005);
            if(result != 0.0)
            	color = result * vec3(0.010,0.010,0.010);
            result = mid_circle(_st, _center, radius);
            if(result != 0.0)
            	color = result * mid_displayColor;
            
        }
        else{
            result = mid_circle(_st, _center, radius + 0.005);
            if(result != 0.0)
            	color = result * vec3(0.010,0.010,0.010);
            result = mid_circle(_st, _center, radius);
            if(result != 0.0)
            	color = result * mid_displayColor.zyx;
        }
    }
    return color;
}

vec4 CartoonPanorama2() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(.0);
	
    for(int i = int(mid_u_time); i < 5 + int(mid_u_time); i++){
        vec2 position = mid_random(float(i));
        
        if(mid_ripple(st, position) != vec3(0.0))
    		color = mid_ripple(st, position);
    }
    
    return vec4(color,1.0);
}