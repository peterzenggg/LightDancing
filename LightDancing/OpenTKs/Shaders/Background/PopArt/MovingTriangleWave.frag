float low_plot(vec2 _st, float y, float height){
    float size = 0.1;
    return step(y - size / 2.0 + height, _st.y) - step(y + size / 2.0 + height, _st.y);
}

float low_square(vec2 _st, float size){
    vec2 square_st = step(0.5 - size / 2.0, _st) - step(0.5 + size / 2.0, _st);
    
    return square_st.x * square_st.y;
}

mat2 low_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec4 MovingTriangleWave() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	vec3 color = low_square(st, 0.9) * low_displayColor;
    float index = mod(low_u_time, 2.0);
    

	
    float move;
    if(index >= 0.0 && index < 1.0)
        move += u_time;
    else if(index >= 1.0 && index < 2.0)
        move -= u_time;
    
    float y = mix(sin(floor(st.x * 10.0) * PI / 2.0), sin(floor(st.x * 10.0 + 1.0) * PI / 2.0), fract(st.x * 10.0)) * 0.05;
    
    for(int i = 0; i < 5; i++){
        float height = 1.0 / 5.0 * float(i) + 0.1 + move;
        color -= low_plot(st, y, fract(height)) * low_displayColor;
    }
    
    return vec4(color,1.0);
}