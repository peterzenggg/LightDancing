float mid_random (in float x) {
    return fract(sin(x)*1e4);
}

float mid_Line(vec2 _st, float hight){
    return smoothstep(hight - 0.1, hight, _st.y) - smoothstep(hight, hight + 0.1, _st.y);
}

vec4 HorizontalLines(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(.0);

    
    for(int i = 0; i < int(mid_u_time); i++){
        float hight =  mid_random(float(i));
        color += mid_Line(st, hight) * mid_displayColor * mid_intensity;
    }
    
    
    return  vec4(color,1.0);
}