mat2 BG_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float BG_square(vec2 _st, float size, vec2 center, bool left){
    float fill = 0.0;
    vec2 result = step(center - size / 2.0, _st) - step(center + size / 2.0, _st);
    vec2 pos = _st - center;
    float length = length(pos);
    float angle = acos(pos.x/length);
    if(pos.y < .0){
        angle = 2.0 * PI - angle;
    }
    
    if(result.x * result.y == 1.0 && (pos.x + pos.y) <= 0.0 && left)
        fill = 1.0;
    else if(result.x * result.y == 1.0 && (pos.x + pos.y) >= 0.0 && !left)
        fill = 1.0;

    return fill;
}

float BG_quarter(vec2 _st, float size, vec2 center){
    vec2 position = center + vec2(0.5) * size;
    float fill = BG_square(_st, size, position, true);
    position = center + vec2(0.5, -0.5) * size;
    fill += BG_square(_st, size, position, false);
    position = center + vec2(-0.5, -0.5) * size;
    fill += BG_square(_st, size, position, true);
    position = center + vec2(-0.5, 0.5) * size;
    fill += BG_square(_st, size, position, true);
    return fill;    
}

float BG_circle(vec2 _st, float size){
    vec2 pos = _st - 0.5;
    float length = length(pos);
    
    return step(length, size / 2.0);
}

vec4 RotateGeo(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    st -= 0.5;
    st *= BG_rotate2d(u_time);
    st += 0.5;
    
	vec2 pos = st;
    
    vec3 color  = vec3(0.0);
    for(int x = 0; x < 4; x++){
        vec2 rotate_st = fract(pos) - 0.5;
        rotate_st *= BG_rotate2d(0.5 * PI * float(x));
        rotate_st += 0.5;
        
        for(int i = 1; i < 2; i++){
            float size = pow(0.5 , float(i))  / 2.0 * (low_intensity + 1.0);
            vec2 center = vec2(0.5, 0.5) + vec2(-1.0, 1.0) * size;

            float draw = BG_quarter(rotate_st, size, center);
            if(draw != 0.0)
                color = draw * low_displayColor;
                
        }
    }
    

    return vec4(color, 1.0);
}