mat2 high_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float high_line(float _st, float position, float size){
    return step(position - size / 2.0, _st) - step(position + size / 2.0, _st);
}

vec4 HighGeoLines() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    st = fract(st + (u_time + high_u_time) / 5.0 );
    vec3 color = vec3(0.);
    

    vec2 pos = st * 10.0;
    
    color = high_line(fract(pos.y), 0.5, 0.5) * high_displayColor;
    
    pos = st - 0.5;
    pos *= high_rotate2d(PI / 4.0);
    pos += 0.5;
    
    if(pos.x > 0.4 && pos.x < 0.6)
        color = high_line(fract(st.x * 10.0), 0.5, 0.5) * high_displayColor;
        
    return vec4(color,1.0);
}