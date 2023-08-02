mat2 rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}


float Line(vec2 _st, vec2 size, vec2 position){
    float x = smoothstep(position.x - size.x / 2.0, position.x, _st.x) - smoothstep(position.x, position.x + size.x / 2.0, _st.x);
    float y = smoothstep(position.y - size.y / 2.0, position.y, _st.y) - smoothstep(position.y, position.y + size.y / 2.0, _st.y);
    
    return x*y;
}

vec4 LineRotate(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    
    
    for(int i = 0; i < 10; i++){
        st -= 0.5;
    	st = rotate2d(mid_u_time) * st;
    	st += 0.5;
        float ifFill = Line(st, vec2(0.520 * mid_intensity, 0.070), vec2(mod(0.7 , 0.5), 0.5));
        
        color += ifFill * mid_displayColor;
    }
    
	
    
    return vec4(color,1.0);
}