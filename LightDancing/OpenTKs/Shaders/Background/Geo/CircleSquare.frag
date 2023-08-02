mat2 BG_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float BG_square(vec2 _st, float size){
    vec2 result = step(0.5 - size / 2.0, _st) - step(0.5 + size / 2.0, _st);
    return result.x * result.y;
}

float BG_circle(vec2 _st, float size){
    vec2 pos = _st - 0.5;
    float length = length(pos);
    
    return step(length, size / 2.0);
}

vec4 CircleSquare(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    st -= 0.5;
    st *= BG_rotate2d((u_time + low_u_time) / 3.0);
    st += 0.5;
    
    vec3 color  = vec3(0.0);
    for(int i = 0; i < 10; i++){
        float size = float(20 - i * 2) / 20.0 * pow(low_intensity, float(i * 4)) * low_intensity * 2.0;
        float draw = BG_circle(st, size);
        if(draw != 0.0)
            color = draw * low_displayColor * low_intensity;
        size = 1.0 * float(20 - i * 2 - 1) / 20.0 * pow(low_intensity, float(i * 4 + 1)) * low_intensity * 2.0;
        draw = BG_square(st, size);
        if(draw != 0.0)
            color = draw * vec3(0.00001) * low_intensity;
    }

    return vec4(color, 1.0);
}