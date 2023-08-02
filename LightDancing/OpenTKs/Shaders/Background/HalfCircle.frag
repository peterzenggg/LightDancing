float half_circle_circle(vec2 _st, vec2 position, float size){
    vec2 pos = _st - position;
    float length = length(pos);
    
    return step(length, size);
}

vec4 HalfCircle() {

    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    
    int mode = int(mod(low_u_time, 6.0));

    if(mode == 0){
        float circle = half_circle_circle(st, vec2(0.5, 0.0), low_intensity * 0.5);
        color = low_displayColor * circle;
    }
    else if(mode == 1){
        float circle = half_circle_circle(st, vec2(0.5, 1.0), low_intensity * 0.5);
        color = low_displayColor * circle;
    }
    else if(mode == 2){
        float circle = half_circle_circle(st, vec2(0.0, 0.5), low_intensity * 0.5);
        color = low_displayColor * circle;
    }
    else if(mode == 3){
        float circle = half_circle_circle(st, vec2(1.0, 0.5), low_intensity * 0.5);
        color = low_displayColor * circle;
    }
    else if(mode == 4){
        float circle = half_circle_circle(st, vec2(0.5, 1.0), low_intensity * 0.5);
        circle += half_circle_circle(st, vec2(0.5, 0.0), low_intensity * 0.5);
        color = low_displayColor * circle;
    }
    else if(mode == 5){
        float circle = half_circle_circle(st, vec2(0.0, 0.5), low_intensity * 0.5);
        circle += half_circle_circle(st, vec2(1.0, 0.5), low_intensity * 0.5);
        color = low_displayColor * circle;
    }
    
    
    return vec4(color,1.0);
}