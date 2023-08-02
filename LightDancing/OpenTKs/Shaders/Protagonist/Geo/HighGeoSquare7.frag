mat2 high_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec3 high_square(vec2 _st, float size, vec2 center, bool left){
    float fill = 0.0;
    vec3 display = vec3(0.0);
    vec2 result = step(center - size / 2.0, _st) - step(center + size / 2.0, _st);
    vec2 pos = _st - center;
    float length = length(pos);
    float angle = acos(pos.x/length);
    if(pos.y < .0){
        angle = 2.0 * PI - angle;
    }
    
    if(result.x * result.y == 1.0 && (pos.x + pos.y) <= 0.0 && left)
        display = high_displayColor;
    else if(result.x * result.y == 1.0 && (pos.x + pos.y) >= 0.0 && !left)
        display = high_displayColor;
	else if(result.x * result.y == 1.0)
        display = vec3(0.000001);
    
    
    return display;
}

vec3 high_quarter(vec2 _st, float size, vec2 center){
    vec3 display = vec3(0.0);
    vec3 result = high_square(_st, size, center, true);
    if(result != vec3(0.0)){
        display = result;
    }
    
    result = high_square(_st, size * 0.5, center, false);
    if(result != vec3(0.0)){
        display = result;
    }
    
    return display;    
}


vec4 HighGeoSquare7(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    st -= 0.5;
    st *= high_rotate2d(u_time);
    st += 0.5;
    
	vec2 pos = st;
    
    vec3 color  = vec3(0.0);
    for(int x = 0; x < 4; x++){
        vec2 rotate_st = pos - 0.5;
        rotate_st *= high_rotate2d(0.5 * PI * float(x));
        rotate_st += 0.5;
        
        for(int i = 1; i < 4; i++){
            float size = pow(0.5 , float(i)) * 2.0 * high_intensity;
            vec2 center = vec2(0.5, 0.5) + vec2(-1.0, 1.0) * size / 2.0;

            vec3 draw = high_quarter(rotate_st, size, center);
            if(draw != vec3(0.0))
                color = draw;
                
        }
    }
    
    float size = pow(0.5 , float(1))  / 2.0;
            vec2 center = vec2(0.5, 0.5) + vec2(-1.0, 1.0) * size;

            vec3 draw = high_quarter(st, size, center);
            //color = draw;
    return vec4(color, 1.0);
}