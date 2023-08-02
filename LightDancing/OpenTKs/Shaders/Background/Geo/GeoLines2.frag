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

vec4 GeoLines2(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color  = vec3(0.0);
    vec3 fillcolor;
    
    st *= 2.0;
    st = mod(st + u_time + low_u_time, 2.0);
    float index = floor(st.x) * 2.0 + floor(st.y);
    if(index == 0.0 || index == 3.0){
        st.x *= 3.0;
        if(fract(st.x) <= 0.5)
            color = low_displayColor;
    }
    else{
        st.y *= 3.0;
        if(fract(st.y) > 0.5)
            color = low_displayColor;
    }
        
    return vec4(color, 1.0);
}