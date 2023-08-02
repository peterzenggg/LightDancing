mat2 low_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec4 CartoonBG5() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	
    vec2 rotateSt = st - 0.5;
    rotateSt *= low_rotate2d(u_time/5.0);
    rotateSt += 0.5;
    
    vec2 counterclockSt = st - 0.5;
    counterclockSt *= low_rotate2d(-u_time/5.0);
    counterclockSt += 0.5;
    
    vec3 color = vec3(0.);
    
    vec2 coordinate = rotateSt;
    
    if(st.x < 0.5){
        coordinate = counterclockSt;
    }
    vec2 pos = coordinate - 0.5;
    float length = length(pos);
    float a = acos(pos.x/length);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    
    float index = a / (2.0 * PI) * 100.0;
    
    float y = floor(sin(index)) * 0.01 * low_intensity + 0.1 * low_intensity;
        float fill = step(length, y);
        color = vec3(fill) * vec3(0.980,0.504,0.216) * low_intensity;
        fill = step(length, y+0.005);
        color += vec3(fill) * vec3(0.165,0.000,0.000);
        
        y =  floor(sin(index)) * 0.02 * low_intensity + 0.2 * low_intensity;
        fill = step(length, y);
        if(color == vec3(0.0)){
            color = vec3(fill) * vec3(0.980,0.889,0.218) * low_intensity;
            fill = step(length, y+0.005);
            color += vec3(fill) * vec3(0.165,0.000,0.000);
        }
        
        y = floor(sin(index)) * 0.03 * low_intensity + 0.3 * low_intensity;
        fill = step(length, y);
        if(color == vec3(0.0)){
            color = vec3(fill) * vec3(0.980,0.430,0.796) * low_intensity;
            fill = step(length, y+0.005);
            color += vec3(fill) * vec3(0.165,0.000,0.000);
        }
        
        y = floor(sin(index)) * 0.04 * low_intensity + 0.4 * low_intensity;
        fill = step(length, y);
        if(color == vec3(0.0)){
            color = vec3(fill) * vec3(0.561,0.719,0.980) * low_intensity;
            fill = step(length, y+0.005);
            color += vec3(fill) * vec3(0.165,0.000,0.000);
        }
        
        y = floor(sin(index)) * 0.05 * low_intensity + 0.5 * low_intensity;
        fill = step(length, y);
        if(color == vec3(0.0)){
            color = vec3(fill) * vec3(0.980,0.402,0.411) * low_intensity;
            fill = step(length, y+0.005);
            color += vec3(fill) * vec3(0.165,0.000,0.000);
        }     

    

    if(st.x < 0.5){
        color = color.zyx;
    }
    
    return vec4(color,1.0);
}