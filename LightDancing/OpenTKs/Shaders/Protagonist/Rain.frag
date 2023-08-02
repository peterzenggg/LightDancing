float random (in float x) {
    return fract(sin(x)*1e4);
}

float Raining(vec2 _st, vec2 position){
    float x = step(position.x-0.025 * high_intensity, _st.x) - step(position.x+0.025 * high_intensity, _st.x);
    float y = step(position.y-0.04 * high_intensity, _st.y) - step(position.y+0.04 * high_intensity, _st.y);
    
    return x*y;
 
}

vec4 Rain(){
    
    float freq = u_time;
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    
    if(high_u_time > 0){
        for(int i = 0; i < int(high_u_time); i++){
            float isRain = Raining(st, vec2(random(float(i)), fract(1.0 - u_time - sin(float(i)))));
    	    color += vec3(0.0, isRain * 0.2, isRain * 0.2);
        }
    }
    if(high_u_time > 30){
        for(int i = 30; i < high_u_time; i++){
            float isRain = Raining(st, vec2(random(float(i)), fract(1.0 - u_time - sin(float(i)))));
    	    color += vec3(0.0, isRain * 0.2, isRain * 0.648);
        }
    }
    if(high_u_time > 50){
        for(int i = 50; i < high_u_time; i++){
            float isRain = Raining(st, vec2(random(float(i)), fract(1.0 - u_time - sin(float(i)))));
    	    color += vec3(0.0, isRain * 0.048, isRain * -0.304);
        }
    }
    
    return vec4(color,1.0);
}