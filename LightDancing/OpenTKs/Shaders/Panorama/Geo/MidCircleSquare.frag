mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float mid_square(vec2 _st, float size){
    vec2 result = step(0.5 - size / 2.0, _st) - step(0.5 + size / 2.0, _st);
    return result.x * result.y;
}

float mid_circle(vec2 _st, float size){
    vec2 pos = _st - 0.5;
    float length = length(pos);
    
    return step(length, size / 2.0);
}

vec4 MidCircleSquare(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    st -= 0.5;
    st *= mid_rotate2d(u_time + mid_u_time);
    st += 0.5;
    
    vec3 color  = vec3(0.0);
    for(int i = 0; i < 10; i++){
        float size = float(20 - i * 2) / 20.0 * pow(mid_intensity, float(i * 2)) * mid_intensity * 2.0;
        float draw = mid_circle(st, size);
        if(draw != 0.0)
            color = draw * mid_displayColor * mid_intensity;
        size = 1.0 * float(20 - i * 2 - 1) / 20.0 * pow(mid_intensity, float(i * 2 + 1)) * mid_intensity * 2.0;
        draw = mid_square(st, size);
        if(draw != 0.0)
            color = draw * vec3(0.00001) * mid_intensity;
    }

    return vec4(color, 1.0);
}