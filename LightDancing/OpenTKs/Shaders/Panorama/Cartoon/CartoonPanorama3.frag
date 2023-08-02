mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}
float mid_Line(vec2 _st, float heigh){
    float y = step(heigh - 0.01, _st.y) - step(heigh + 0.01, _st.y);
    
    return y;
}

vec4 CartoonPanorama3(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(.0);

    
    for(int i = 0; i < int(mid_u_time); i++){
    	vec3 displayColor;   
        float heigh = 0.35 - 0.5 *(1.0 - mid_intensity) / (mid_u_time) * float(i);
        float angle = 0.800 / mid_u_time * float(i) * mid_intensity;
        vec2 _st = st - vec2(0.5, 0.2);
   	 	_st = mid_rotate2d(0.0) * _st;
   	 	_st += vec2(0.5, 0.2);
        
        if(mod(float(i), 3.0) == 0.0){
            displayColor = mid_displayColor;
        }
        else if(mod(float(i), 3.0) == 1.0){
         	displayColor = mid_displayColor.zyx;
        }
        else if(mod(float(i), 3.0) == 2.0){
            displayColor = mid_displayColor.yxz;
        }
        
        float draw = mid_Line(_st, heigh);
        if(draw != 0.0)
        	color = mid_Line(_st, heigh) * displayColor * mid_intensity;
        
        float angle2 = 0.800 / mid_u_time * float(i) * mid_intensity;
        vec2 _st2 = st - vec2(0.5, 0.5);
   	 	_st2 = mid_rotate2d(2.208) * _st2;
   	 	_st2 += vec2(0.5, 0.5);
        draw = mid_Line(_st2, heigh);
        if(draw != 0.0)
        	color = mid_Line(_st2, heigh) * displayColor * mid_intensity;
        
        float angle3 = 0.800 / mid_u_time * float(i) * mid_intensity;
        vec2 _st3 = st - vec2(0.5, 0.5);
   	 	_st3 = mid_rotate2d(4.096) * _st3;
   	 	_st3 += vec2(0.5, 0.5);
        draw = mid_Line(_st3, heigh);
        if(draw != 0.0)
            color = mid_Line(_st3, heigh) * displayColor * mid_intensity;
        
        
    }
    
    return  vec4(color,1.0);
}