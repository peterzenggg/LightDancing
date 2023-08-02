float random (in float x) {
    return fract(sin(x)*1e4);
}

float DrawCircle(vec2 _st, vec2 center, float size){
    vec2 pos = _st - center;
    float length = length(pos);
    
    return 1.0 - smoothstep(size, size + 0.02, length);
}

vec4 BouncingBall() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    
    for(int i = 0; i < int(high_u_time); i++){
        vec2 position = vec2(random(float(i)));
        float angle = random(float(i) + floor(high_u_time));

        position.x = mod(position.x + high_u_time * cos(angle), 2.0) > 1.0? abs(mod(position.x + high_u_time * cos(angle), 2.0) - 2.0): mod(position.x + high_u_time * cos(angle), 2.0);
        position.y = mod(position.y + high_u_time * sin(angle), 2.0) > 1.0? abs(mod(position.y + high_u_time * sin(angle), 2.0) - 2.0) : mod(fract(position.y + high_u_time * sin(angle)), 2.0);

        float ifDraw = DrawCircle(st, position, 0.1 * high_intensity);
        color += ifDraw * high_displayColor;
    }
    return vec4(color,1.0);
}