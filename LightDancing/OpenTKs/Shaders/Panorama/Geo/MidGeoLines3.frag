mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float mid_square(vec2 _st, float angle, float size){
    vec2 pos = _st - 0.5;
    pos *= mid_rotate2d(angle);
    pos += 0.5;
    
    vec2 result = step(0.5 - size / 2.0, pos) - step(0.5 + size / 2.0, pos);
    return result.x * result.y;
}

vec4 MidGeoLines3(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color  = vec3(0.0);
    vec3 fillcolor;
    
    st *= 2.0;
    st.y *= 3.0;
    st.y += u_time;
    if(st.x > 1.0){
        if(fract(st.y) < 0.5)
            color = mid_displayColor;
    }
    else{
        if(fract(st.y) > 0.5)
            color = mid_displayColor;
    }
        
    return vec4(color, 1.0);
}