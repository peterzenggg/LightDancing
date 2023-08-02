mat2 BG_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float BG_square(vec2 _st, float angle, float size){
    vec2 pos = _st - 0.5;
    pos *= BG_rotate2d(angle);
    pos += 0.5;
    
    vec2 result = step(0.5 - size / 2.0, pos) - step(0.5 + size / 2.0, pos);
    return result.x * result.y;
}

vec4 GeoLines4(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color  = vec3(0.0);
    vec3 fillcolor;
    
    st *= 2.0;
    st.x *= 3.0;
    st.x += u_time + low_u_time / 4.0;
    if(st.y > 1.0){
        if(fract(st.x) < 0.5)
            color = low_displayColor;
    }
    else{
        if(fract(st.x) > 0.5)
            color = low_displayColor;
    }
        
    return vec4(color, 1.0);
}