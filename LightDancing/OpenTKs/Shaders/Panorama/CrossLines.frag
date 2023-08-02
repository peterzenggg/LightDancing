float mid_random (in float x) {
    return fract(sin(x)*1e4);
}
mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}
float mid_Line(vec2 _st, vec2 position){
    float y = smoothstep(position.y - 0.1, position.y, _st.y) - smoothstep(position.y, position.y + 0.1, _st.y);
    float x = step(position.x, _st.x);
    
    return x * y;
}

vec4 CrossLines(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(.0);

    
    for(int i = 0; i < 4; i++){
        vec2 position = vec2(0.1 - 0.2 / 4.0 * float(i), (0.3 - 0.2 / 4.0 * float(i)) * -2.5 + 0.95);
        float angle = 0.800 / 4.0 * float(i) * mid_intensity;
        float hight =  mid_random(float(i));
        vec2 _st = st - position;
   	 	_st = mid_rotate2d(0.528 - angle) * _st;
   	 	_st += position;
        color += mid_Line(_st, position) * mid_displayColor * mid_intensity;
        
        vec2 position2 = vec2(0.9 + 0.2 / 4.0 * float(i), (0.7 + 0.2 / 4.0 * float(i)) * 2.5 - 1.558);
        float angle2 = 0.800 / 4.0 * float(i) * mid_intensity;
        float hight2 =  mid_random(float(i));
        vec2 _st2 = st - position2;
   	 	_st2 = mid_rotate2d(2.624 + angle) * _st2;
   	 	_st2 += position2;
        color += mid_Line(_st2, position2) * mid_displayColor * mid_intensity;
    }
    
    
    return  vec4(color,1.0);
}