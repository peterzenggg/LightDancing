vec2 high_random (in float x) {
    return vec2(fract(sin(x)*1e4 + 0.2), fract(cos(x)*1e4));
}

float high_circle(vec2 _st, vec2 center, float radius){
    vec2 pos = _st - center;
    float length = length(pos);
    
    return step(length, radius * high_intensity);
    
}

float high_rip(vec2 _st, vec2 center){
    return high_circle(_st, center, 0.07) - high_circle(_st, center, 0.05) + high_circle(_st, center, 0.02);
  
}

vec4 RandomTarget() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    for(int i = 0; i < int(high_u_time); i++){
        vec2 position = high_random(float(i));

        float ifDraw = high_rip(st, position);
        color += ifDraw * high_displayColor;
    }
    
    return vec4(color,1.0);
}