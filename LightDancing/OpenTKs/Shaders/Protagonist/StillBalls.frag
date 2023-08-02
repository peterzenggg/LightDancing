vec2 high_random (in float x) {
    return vec2(fract(sin(x)*1e4), fract(cos(x)*1e4));
}


float DrawCircle(vec2 _st, vec2 center, float size){
    vec2 pos = _st - center;
    float length = length(pos);
    
    return 1.0 - smoothstep(size, size + 0.02, length);
}

vec4 StillBalls() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    
    for(int i = 0; i < int(high_u_time); i++){
        vec2 position = high_random(float(i));

        float ifDraw = DrawCircle(st, position, 0.1 * high_intensity);
        color += ifDraw * high_displayColor;
    }
    return vec4(color,1.0);
}