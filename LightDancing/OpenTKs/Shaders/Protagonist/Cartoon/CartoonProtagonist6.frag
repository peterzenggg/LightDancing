float high_random (in float x) {
    return fract(sin(x)*1e4);
}

float high_Raining(vec2 _st, vec2 position, float size){
    float x = step(position.x-0.025 * high_intensity * size, _st.x) - step(position.x+0.025 * high_intensity * size, _st.x);
    float y = step(position.y-0.04 * high_intensity * size, _st.y) - step(position.y+0.04 * high_intensity * size, _st.y);
    
    return x*y;
 
}

vec4 CartoonProtagonist6(){
    
    float freq = u_time;
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    
    
    for(int i = 0; i < int(high_u_time); i++){
        float isRain = high_Raining(st, vec2(high_random(float(i)), fract(1.0 - u_time - sin(float(i)))), 1.0);
        
        if(isRain != 0.0){
            color = isRain * vec3(0.065,0.065,0.065);
            isRain = high_Raining(st, vec2(high_random(float(i)), fract(1.0 - u_time - sin(float(i)))), 1.0 - 0.04);
            if(mod(float(i), 3.0) == 0.0){
                color = isRain * high_displayColor;
                isRain = high_Raining(st, vec2(high_random(float(i)), fract(1.0 - u_time - sin(float(i)))), 0.45 + 0.04);
                if(isRain != 0.0)
                    color = isRain * vec3(0.065,0.065,0.065);
                isRain = high_Raining(st, vec2(high_random(float(i)), fract(1.0 - u_time - sin(float(i)))), 0.45);
                if(isRain != 0.0)
                    color = isRain * high_displayColor.zyx;
            }
            else if(mod(float(i), 3.0) == 1.0){
                color = isRain * high_displayColor.zyx;
                isRain = high_Raining(st, vec2(high_random(float(i)), fract(1.0 - u_time - sin(float(i)))), 0.45 + 0.04);
                if(isRain != 0.0)
                    color = isRain * vec3(0.065,0.065,0.065);
                isRain = high_Raining(st, vec2(high_random(float(i)), fract(1.0 - u_time - sin(float(i)))), 0.45);
                if(isRain != 0.0)
                    color = isRain * high_displayColor.zxy;
            }
            else if(mod(float(i), 3.0) == 2.0){
                color = isRain * high_displayColor.yzx;
                isRain = high_Raining(st, vec2(high_random(float(i)), fract(1.0 - u_time - sin(float(i)))), 0.45 + 0.04);
                if(isRain != 0.0)
                    color = isRain * vec3(0.065,0.065,0.065);
                isRain = high_Raining(st, vec2(high_random(float(i)), fract(1.0 - u_time - sin(float(i)))), 0.45);
                if(isRain != 0.0)
                    color = isRain * high_displayColor;
            }
            
        }
    }
    return vec4(color,1.0);
}