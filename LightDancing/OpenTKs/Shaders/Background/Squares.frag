float BG_Square(vec2 _st, vec2 _size){
    float x = step(0.5 - _size.x, _st.x) - step(0.5 + _size.x, _st.x);
    float y = step(0.5 - _size.y, _st.y) - step(0.5 + _size.y, _st.y);
    
    return x * y;
}
mat2 BG_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}
vec4 Squares(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    
    int index = int(mod(low_u_time, 4.0));
    if(index == 2){
        st -= 0.5;
        st = BG_rotate2d(u_time) * st;
        st += 0.5;
    }

	for(int i = 0; i < int((low_intensity+0.1) *10.0); i++){
        float size = 0.5 / float(int((low_intensity+0.1) *10.0)) * float(i) * low_intensity;
        if(mod(float(int((low_intensity+0.1) *10.0)), 2.0) == 0.0){
            if(mod(float(i), 2.0) == 1.0){
                color += BG_Square(st, vec2(size)) * low_displayColor;
            }
            else{
                color -= BG_Square(st, vec2(size)) * low_displayColor;
            }
        }
        else{
            if(mod(float(i), 2.0) == 0.0){
                color += BG_Square(st, vec2(size)) * low_displayColor;
            }
            else{
                color -= BG_Square(st, vec2(size)) * low_displayColor;
            }
        }
    }
    

    return vec4(color,1.0);
}