mat2 BG_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float BG_Line(vec2 _st){
    return step(1.0 - low_intensity, _st.x);
}

vec4 Arrow(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec2 triple_x_st = vec2(st.x * 4.0 - u_time , st.y );
	vec2 top_st = triple_x_st - 0.5;

    float index = mod(low_u_time, 2.0);
    vec3 color = vec3(0.);
    if(int(index) == 0){
        
        top_st = BG_rotate2d(1.000) * top_st;
        top_st += 0.5;
    
        vec2 bottom_st = triple_x_st - 0.5;
        bottom_st = BG_rotate2d(-1.0) * bottom_st;
        bottom_st += 0.5;
    
        if(fract(triple_x_st.y) > 0.5){
        
            color = (BG_Line(fract(top_st))) * low_displayColor * low_intensity;
        }
        else{
            color = (BG_Line(fract(bottom_st))) * low_displayColor * low_intensity;
        }
    }
    else{

        top_st = BG_rotate2d(-1.000) * top_st;
        top_st += 0.5;
    
        vec2 bottom_st = triple_x_st - 0.5;
        bottom_st = BG_rotate2d(1.0) * bottom_st;
        bottom_st += 0.5;
    
        if(fract(triple_x_st.y) > 0.5){
        
            color = (BG_Line(fract(top_st))) * low_displayColor * low_intensity;
        }
        else{
            color = (BG_Line(fract(bottom_st))) * low_displayColor * low_intensity;
        }
    }

    return vec4(color,1.0);
}