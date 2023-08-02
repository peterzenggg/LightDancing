float low_line(vec2 _st, vec2 center){
    float size = 0.5;
    return step(center.x - size / 2.0, _st.x) - step(center.x + size / 2.0, _st.x);
}

float low_square(vec2 _st, float size){
    vec2 square_st = step(0.5 - size / 2.0, _st) - step(0.5 + size / 2.0, _st);
    
    return square_st.x * square_st.y;
}

vec4 PAHorizontalStrip() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	vec3 color = low_square(st, 0.9) * low_displayColor;
    float index = mod(low_u_time, 2.0);
    vec2 line_st = st * 10.0;
    
    if(index >= 0.0 && index < 1.0)
        line_st += u_time;
    else if(index >= 1.0 && index < 2.0)
        line_st -= u_time;
    
    float draw = low_line(fract(line_st), vec2(0.5));
    color -= draw * low_displayColor;
    
    return vec4(color,1.0);
}