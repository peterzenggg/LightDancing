mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float mid_line(vec2 _st, vec2 position, float size){
    return step(position.x - size / 2.0, _st.x) - step(position.x + size / 2.0, _st.x);
}

vec4 MidHexagon() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    st -= 0.5;
    st *= mid_rotate2d(mid_u_time + u_time);
    st += 0.5;
    vec3 color = vec3(0.);
    
	vec2 pos = st - 0.5;
    float lengthr = length(pos);
    float a = acos(pos.x/lengthr);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    
    
    float index = floor(a / (2.0 * PI) * 6.0);
    float rotateA = PI / 6.0 + PI / 3.0 * index;
    vec2 rotate_st = st - 0.5;
    rotate_st *= mid_rotate2d(-rotateA);
    rotate_st += 0.5;
    for(int i = 0; i < 10; i++){
        
        float size = 0.5 / 5.0 * float(i);
        if(mod(index, 2.0) == 1.0)
            size = 0.05 + 0.5 / 5.0 * float(i);
        vec2 position = vec2(0.5) + vec2(cos(PI / 6.0) * size, sin(PI / 6.0) * size);
        
    	color += mid_line(rotate_st, position, 0.05) * mid_displayColor;
    }
    
        
    return vec4(color,1.0);
}