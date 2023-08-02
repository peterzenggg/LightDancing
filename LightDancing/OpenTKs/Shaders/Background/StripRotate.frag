float BG_Line(vec2 _st){
    return step(_st.x, low_intensity);
}
mat2 BG_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}
vec4 StripRotate(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    vec2 pos = st * (low_intensity + 0.1) * 10.0;
    
    pos -= 0.5;
    pos = BG_rotate2d(u_time + low_u_time) * pos;
    pos += 0.5;
    
    color = BG_Line(fract(pos)) * low_displayColor;
    
    

    return vec4(color,1.0);
}