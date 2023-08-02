mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float mid_circle(vec2 _st, vec2 _center, float radius){
    vec2 pos = _st - _center;
    float length = length(pos);
    return step(length, radius);
}

vec3 mid_ripple(vec2 _st, vec2 _center){
    vec3 result;
    float draw;
    vec2 pos = _st - _center;
    float length = length(pos);
    float a = acos(pos.x/length);
    

    for(int i = 0; i < 8; i++){
        float angle = 2.0 * PI / 8.0 * float(i);
        vec2 position = _center + vec2(cos(angle), sin(angle)) * vec2(0.4 * mid_intensity);
        if(mod(float(i), 4.0) == 0.0){
            draw = mid_circle(_st, position, (0.1 + 0.01) * mid_intensity);
            if(draw != 0.0)
            	result = mid_circle(_st, position, (0.1 + 0.01) * mid_intensity) * vec3(0.25);
            draw = mid_circle(_st, position, 0.1 * mid_intensity);
            if(draw != 0.0)
        		result = mid_circle(_st, position, 0.1 * mid_intensity) * mid_displayColor;
        }
        else if(mod(float(i), 4.0) == 1.0){
            draw = mid_circle(_st, position, (0.1 + 0.01) * mid_intensity);
            if(draw != 0.0)
            	result = mid_circle(_st, position, (0.1 + 0.01) * mid_intensity) * vec3(0.25);
            draw = mid_circle(_st, position, 0.1 * mid_intensity);
            if(draw != 0.0)
        		result = mid_circle(_st, position, 0.1 * mid_intensity) * mid_displayColor.zyx;
        }
        else if(mod(float(i), 4.0) == 2.0){
            draw = mid_circle(_st, position, (0.1 + 0.01) * mid_intensity);
            if(draw != 0.0)
            	result = mid_circle(_st, position, (0.1 + 0.01) * mid_intensity) * vec3(0.25);
            draw = mid_circle(_st, position, 0.1 * mid_intensity);
            if(draw != 0.0)
        		result = mid_circle(_st, position, 0.1 * mid_intensity) * mid_displayColor.yzx;
        }
        else if(mod(float(i), 4.0) == 3.0){
            draw = mid_circle(_st, position, (0.1 + 0.01) * mid_intensity);
            if(draw != 0.0)
            	result = mid_circle(_st, position, (0.1 + 0.01) * mid_intensity) * vec3(0.25);
            draw = mid_circle(_st, position, 0.1 * mid_intensity);
            if(draw != 0.0)
        		result = mid_circle(_st, position, 0.1 * mid_intensity) * mid_displayColor.yxz;
        }
    }

    
    return result;
}

vec4 CartoonPanorama4(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(.0);
    
    st -= 0.5;
    st = mid_rotate2d(u_time + mid_u_time) * st;
    st += 0.5;
    
	color = mid_ripple(st, vec2(0.5)) * mid_displayColor;
    
    return vec4(color,1.0);
}