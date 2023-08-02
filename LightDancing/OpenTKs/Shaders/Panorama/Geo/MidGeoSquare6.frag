mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float mid_square(vec2 _st, float size, vec2 center){
    float fill = 0.0;
    vec2 result = step(center - size / 2.0, _st) - step(center + size / 2.0, _st);
    vec2 pos = _st - center;
    float length = length(pos);
    float angle = acos(pos.x/length);
    if(pos.y < .0){
        angle = 2.0 * PI - angle;
    }
    
    if(result.x * result.y == 1.0 && angle >= 0.0 && angle <= PI / 4.0)
        fill = 1.0;
    if(result.x * result.y == 1.0 && angle >= PI * 0.75 && angle <= PI * 1.25)
        fill = 1.0;
    
    return fill;
}

float mid_circle(vec2 _st, float size){
    vec2 pos = _st - 0.5;
    float length = length(pos);
    
    return step(length, size / 2.0);
}

vec4 MidGeoSquare6(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	st -= 0.5;
    st *= mid_rotate2d(u_time);
    st += 0.5;
    
    vec3 color  = vec3(0.0);
    for(int x = 0; x < 4; x++){
        vec2 rotate_st = st - 0.5;
        rotate_st *= mid_rotate2d(0.5 * PI * float(x));
        rotate_st += 0.5;
        
        for(int i = 0; i < 8; i++){
            float size = pow(0.5 , float(i))  / 2.0 * mid_intensity;
            vec2 center = vec2(0.5, 0.5) + vec2(-1.0, 1.0) * size / 2.0;

            float draw = mid_square(rotate_st, size, center);
            if(draw != 0.0)
                color = draw * mid_displayColor;
        }
    }
    
    return vec4(color, 1.0);
}