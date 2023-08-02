mat2 BG_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float BG_line(float _st, float position, float size){
    return step(position - size / 2.0, _st) - step(position + size / 2.0, _st);
}

vec4 GeoLines() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    st = fract(st + (u_time + low_u_time) / 5.0 );
    vec3 color = vec3(0.);
    

    vec2 pos = st * 10.0;
    
    color = BG_line(fract(pos.y), 0.5, 0.5) * low_displayColor;
    
    pos = st - 0.5;
    pos *= BG_rotate2d(PI / 4.0);
    pos += 0.5;
    
    if(pos.x > 0.4 && pos.x < 0.6)
        color = BG_line(fract(st.x * 10.0), 0.5, 0.5) * low_displayColor;
        
    return vec4(color,1.0);
}