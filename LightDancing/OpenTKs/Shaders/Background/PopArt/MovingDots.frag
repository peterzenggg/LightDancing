float low_circle(vec2 _st, vec2 center){
    vec2 pos = _st - center;
    float length = length(pos);
    return step(length, 0.03);
}

float low_square(vec2 _st, float size){
    vec2 square_st = step(0.5 - size / 2.0, _st) - step(0.5 + size / 2.0, _st);
    
    return square_st.x * square_st.y;
}

vec4 MovingDots() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	vec3 color = low_square(st, 0.9) * low_displayColor;
    float index = mod(low_u_time, 4.0);
    vec2 positionMove;
    if(index >= 0.0 && index < 1.0)
        positionMove.x += u_time;
    else if(index >= 1.0 && index < 2.0)
        positionMove.x -= u_time;
    else if(index >= 2.0 && index < 3.0)
        positionMove.y += u_time;
    else if(index >= 3.0 && index < 4.0)
        positionMove.y -= u_time;
    
    float result = 1.0;
    for(int i = 0; i < 10; i++){
        for(int j = 0; j < 10; j++){
            vec2 bias = vec2(0.05, (mod(float(i), 2.0) + 1.0) * 0.05 - 0.025);
            vec2 position = fract(vec2(1.0 / 10.0 * float(i), 1.0 / 10.0 * float(j)) + bias + positionMove);
            result -= low_circle(st, position);
        }
    }
    color = result * color;
    
    return vec4(color,1.0);
}