float high_random (in float x) {
    return fract(sin(x)*1e4);
}

mat2 high_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float high_Line(vec2 _st, vec2 center){
    vec2 pos = _st - center;
    pos = high_rotate2d(u_time * center.x) * pos;
    pos += center;
    float x = step(center.x - 0.07 * high_intensity, pos.x) - step(center.x + 0.07 * high_intensity, pos.x);
    float y = step(center.y - 0.02 * high_intensity, pos.y) - step(center.y + 0.02 * high_intensity, pos.y);
    
    return x * y;
}
vec4 CartoonProtagonist3(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    for(int i = 0; i < int(high_u_time); i++){
        vec2 position = vec2(high_random(float(i)), fract(1.0 - u_time - sin(float(i))));
        float draw = high_Line(st, position);
        if(mod(float(i), 4.0) == 0.0){
            if(draw != 0.0)
                color = draw * high_displayColor * high_intensity;
        }
        else if(mod(float(i), 4.0) == 1.0){
            if(draw != 0.0)
                color = draw * high_displayColor.zxy * high_intensity;
        }
        else if(mod(float(i), 4.0) == 2.0){
            if(draw != 0.0)
                color = draw * high_displayColor.xzy * high_intensity;
        }
        else if(mod(float(i), 4.0) == 3.0){
            if(draw != 0.0)
                color = draw * high_displayColor.yzz * high_intensity;
        }
    }
    
    return vec4(color,1.0);
}