mat2 low_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float low_square(vec2 _st, float size){
    float x = step(0.5 - size * low_intensity / 2.0, _st.x) - step(0.5 + size * low_intensity / 2.0, _st.x);
    float y = step(0.5 - size * low_intensity / 2.0, _st.y) - step(0.5 + size * low_intensity / 2.0, _st.y);
    
    return x * y;
}

vec4 CartoonBG4() {
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
    

    float fill = low_square(coordinate, 0.2);
    color = vec3(fill) * vec3(0.980,0.504,0.216) * low_intensity;
    fill = low_square(coordinate, 0.2 + 0.045);
    color += vec3(fill) * vec3(0.165,0.000,0.000);
        
    
    fill = low_square(coordinate, 0.4);
    if(color == vec3(0.0)){
        color = vec3(fill) * vec3(0.980,0.889,0.218) * low_intensity;
        fill = low_square(coordinate, 0.4 + 0.045);
        color += vec3(fill) * vec3(0.165,0.000,0.000);
    }
    
    fill = low_square(coordinate, 0.6);
    if(color == vec3(0.0)){
        color = vec3(fill) * vec3(0.980,0.430,0.796) * low_intensity;
        fill = low_square(coordinate, 0.6 + 0.045);
        color += vec3(fill) * vec3(0.165,0.000,0.000);
    }
    
    fill = low_square(coordinate, 0.8);
    if(color == vec3(0.0)){
        color = vec3(fill) * vec3(0.561,0.719,0.980) * low_intensity;
        fill = low_square(coordinate, 0.8 + 0.045);
        color += vec3(fill) * vec3(0.165,0.000,0.000);
    }
    
    fill = low_square(coordinate, 1.0);
    if(color == vec3(0.0)){
        color = vec3(fill) * vec3(0.980,0.402,0.411) * low_intensity;
        fill = low_square(coordinate, 1.0 + 0.045);
        color += vec3(fill) * vec3(0.165,0.000,0.000);
    }
        

    if(st.x < 0.5){
        color = color.zyx;
    }
    
    return vec4(color,1.0);
}