float High_Line(vec2 _st, vec2 center){
    float x = smoothstep(center.x-0.015, center.x, _st.x) - smoothstep(center.x, center.x + 0.015, _st.x);
    float y = 1.0 - step(high_intensity * 1.5, _st.y);
    return x * y;
}

mat2 High_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec4 Laser(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    
    vec2 pos = st - 0.5;
    float length = length(pos);
    
    for(int i = 0; i < 4; i++){
        vec2 center = vec2(1.0, 0.0);
        vec2 pos = st - center;
        pos = High_rotate2d(PI / 8.0/ 4.0 * float(i)+fract(high_intensity)) * pos;
        pos += center;
        color += vec3(High_Line(pos, center)) * high_displayColor;
    }
    
	for(int i = 0; i < 4; i++){
        vec2 center = vec2(0.0, 0.0);
        vec2 pos = st - center;
        pos = High_rotate2d(-PI / 8.0/ 4.0 * (float(i))-fract(high_intensity)) * pos;
        pos += center;
        color += vec3(High_Line(pos, center)) * high_displayColor;
    }
    
    
    return vec4(color,1.0);
}