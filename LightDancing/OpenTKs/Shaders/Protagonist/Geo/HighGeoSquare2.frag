﻿mat2 high_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float high_square(vec2 _st, float angle, float size){
    vec2 pos = _st - 0.5;
    pos *= high_rotate2d(angle);
    pos += 0.5;
    
    vec2 result = step(0.5 - size / 2.0, pos) - step(0.5 + size / 2.0, pos);
    return result.x * result.y;
}

vec4 HighGeoSquare2(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color  = vec3(0.0);
    vec3 fillcolor;
    for(int i = 0; i < 8; i++){
        if(mod(float(i), 2.0) == 0.0)
            fillcolor = high_displayColor;
        else
            fillcolor = vec3(0.001);
        float angle = 2.0 * PI / 8.0 * float(i - 8) * high_intensity;
        float size = 1.0 / 8.0 * float(i - 8) * high_intensity;
        float draw = high_square(st, angle, size);
        if(draw != 0.0)
            color = draw * fillcolor;
    }
    return vec4(color, 1.0);
}