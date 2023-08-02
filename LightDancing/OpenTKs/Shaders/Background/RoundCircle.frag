mat2 BG_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float BG_Circle(vec2 _st){
    vec2 pos = _st - 0.5;
    float length = length(pos);
    return step(length, 0.3 * low_intensity);
    
}

vec4 RoundCircle(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    
    vec2 pos = st - 0.5;
    float length = length(pos);
    
    //color = vec3(length - fract(u_time / 3.0));

    
    for(int i = 0; i < 5; i++){
        vec2 center = 0.2 * vec2(cos(2.0*PI/5.0 * float(i)) ,sin(2.0*PI/5.0 * float(i)));
        vec2 pos = st - center;
        pos -= 0.5;
        pos = BG_rotate2d(low_u_time) * pos;
        pos += 0.5;
        pos -= center;
        color += BG_Circle(pos) * 0.5 * low_displayColor;
    }
    
    
    
    return vec4(color,1.0);
}