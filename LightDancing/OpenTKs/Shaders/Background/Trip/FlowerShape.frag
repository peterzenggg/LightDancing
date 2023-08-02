mat2 low_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}


vec4 FlowerShape() {
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
        //coordinate = counterclockSt;
    }
    vec2 pos = coordinate - 0.5;
    float length = length(pos);
    float a = acos(pos.x/length);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    
    float index = a / (2.0 * PI) * 3.14 * 60.0 * low_intensity;

    for(int i = 0; i < 8; i++){
        vec3 displayColor;
        if(mod(float(i), 2.0) == 0.0)
            displayColor = low_displayColor;
        else
            displayColor = vec3(0.00001);
        
        float size = 1.0 / 8.0 * float(8 - i);
        float wave = 0.02 + 0.05 * float(8 - i);
        float y = sin(index) * wave * low_intensity + size;
        float fill = step(length, y);
        if(fill != 0.0)
            color = displayColor;
        
    }
    
    return vec4(color,1.0);
}