float BG_Row(vec2 _st, float size){
    return step(_st.y, size);
}

float BG_Column(vec2 _st, float size){
    return step(_st.x, size);
}

vec4 Grid(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec2 triple_st = st * (low_intensity + 0.1) * 10.0;
    vec3 color = vec3(0.);
	
    if(int(mod(floor(triple_st.y), 3.0)) == 0){
		color += BG_Row(fract(triple_st), 0.2 * (low_intensity - 0.3)) * vec3(1.0, 0.2, 0.0);
    }
    else if(int(mod(floor(triple_st.y), 3.0)) == 1){
        color += BG_Row(fract(triple_st), 1.0 - 0.9 * (low_intensity - 0.3)) * vec3(0.420,0.298,0.091);
    }
    else{
        color += BG_Row(fract(triple_st), 0.6* (low_intensity - 0.3)) * vec3(0.400,0.132,0.147);
    	
    }
    if(int(mod(floor(triple_st.x), 4.0)) == 0){
        color += BG_Column(fract(triple_st), 0.6 * (low_intensity - 0.3)) * vec3(0.480,0.238,0.131);
    }
    else if(int(mod(floor(triple_st.x), 3.0)) == 1){
        color += BG_Column(fract(triple_st), 0.2 * (low_intensity - 0.3)) * vec3(0.540,0.362,0.245);
    }
    else if(int(mod(floor(triple_st.x), 3.0)) == 2){
        color += BG_Column(fract(triple_st), 1.0 - 0.6 * (low_intensity - 0.3)) * vec3(1.000,0.727,0.232);
    	
    }else{
        color += BG_Column(fract(triple_st), 1.0 - 0.7 * (low_intensity - 0.3)) * vec3(0.405,0.337,0.121);
    	
    }
    
    return vec4(color,1.0);
}