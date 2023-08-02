float mid_circle(vec2 _st, vec2 position, float size){
    vec2 pos = _st - position;
    float length = length(pos);
    
    return step(length, size / 2.0);
}

vec4 MidGeoCircle(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    
	vec2 pos = st;
    
    vec3 color  = vec3(0.0);
    
    for(int i = 0; i < 5; i++){
        vec3 displayColor = mid_displayColor;
        if(mod(float(i), 2.0) == 1.0)
            displayColor = vec3(0.00001);
    	float size = 1.0 / 5.0 * float( 5 - i) * mid_intensity;
        vec2 position = vec2(0.500,0.510);
        float fill = mid_circle(st, position, size);
        
        if(fill != 0.0)
        color = fill * displayColor;
    }
    

    return vec4(color, 1.0);
}