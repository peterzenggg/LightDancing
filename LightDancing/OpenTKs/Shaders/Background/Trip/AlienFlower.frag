mat2 low_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float low_head(vec2 _st, vec2 center, float size){
    float index = _st.y * 5.0;
    float x = smoothstep(center.x - 0.12 * size * index, center.x - 0.0 * size * index, _st.x) - smoothstep(center.x + 0.0 * size * index, center.x + 0.12 * size * index, _st.x);
    float y = smoothstep(center.y - 0.3  * size, center.y - 0.0 * size, _st.y) - smoothstep(center.y + 0.0 * size, center.y + 0.3 * size, _st.y);
    
    return x * y;
}

float eye(vec2 _st, vec2 center, float size){
    float index = _st.y * 5.0;
    float x = smoothstep(center.x - 0.12 * size * index, center.x - 0.0 * size * index, _st.x) - smoothstep(center.x + 0.0 * size * index, center.x + 0.12 * size * index, _st.x);
    float y = smoothstep(center.y - 0.46  * size, center.y - 0.0 * size, _st.y) - smoothstep(center.y + 0.0 * size, center.y + 0.4 * size, _st.y);
    
    return x * y;
}

float smile(vec2 _st, vec2 center){
    vec2 pos = _st - center;
    float length = length(pos);
    float fill = 0.0;
    
    if(pos.y < 0.0)
        fill = step(0.03, length) - step(0.04, length);
    
    return fill;
}

vec3 alien(vec2 _st, vec2 center){
    vec3 displayColor;
    float draw = low_head(_st, center, 1.0);
    
    if(draw > 0.5)
        displayColor = vec3(0.130,0.130,0.130);
    draw = low_head(_st, center, 0.9);
    if(draw > 0.5)
        displayColor = vec3(0.406,0.995,0.142);
    
    draw = eye(_st, center - vec2(0.060,-0.030), 0.2);
    if(draw > 0.5)
        displayColor = vec3(0.00001);
    
    draw = eye(_st, center - vec2(-0.060,-0.03), 0.2);
    if(draw > 0.5)
        displayColor = vec3(0.00001);
    draw = smile(_st, center - vec2(0.0, 0.06));
    if(draw > 0.5)
        displayColor = vec3(0.00001);
    return displayColor;
}

vec4 AlienFlower() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	
    vec2 rotateSt = st - 0.5;
    rotateSt *= low_rotate2d(u_time/5.0);
    rotateSt += 0.5;
    
    vec2 counterclockSt = st - 0.5;
    counterclockSt *= low_rotate2d(-u_time/5.0);
    counterclockSt += 0.5;
    
    vec3 color = vec3(0.);
    
    vec2 coordinate = rotateSt;
    
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
        if(i == 7 || i == 6)
            displayColor = vec3(0.00001, st.x, st.y);
        
        float size = 1.0 / 8.0 * float(8 - i);
        float wave = 0.02 + 0.05 * float(8 - i);
        float y = sin(index) * wave * low_intensity + size;
        float fill = step(length, y);
        if(fill != 0.0)
            color = displayColor;
    }
    
    vec3 et = alien(rotateSt, vec2(0.5));
    if(et != vec3(0.0))
        color = et;
    
    
    return vec4(color,1.0);
}