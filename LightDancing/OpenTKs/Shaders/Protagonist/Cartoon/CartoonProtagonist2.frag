mat2 high_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec2 high_random (in float x) {
    return vec2(fract(sin(x)*1e4 + 0.2), fract(cos(x)*1e4));
}

float high_Square(vec2 _st, vec2 center, float size){
    float x = step(center.x - high_intensity * 0.050 * size, _st.x) - step(center.x + high_intensity * 0.05 * size, _st.x);
    float y = step(center.y - high_intensity * 0.05 * size, _st.y) - step(center.y + high_intensity * 0.05 * size, _st.y);
    
    return x * y;
    
}

float high_DrawHeart(vec2 _st, vec2 center, float size){
    vec2 center1 = center - vec2(high_intensity * 0.05 * size);
    vec2 center2 = center - vec2(high_intensity * 0.05 * size, -high_intensity * 0.05 * size);
    vec2 center3 = center - vec2(-high_intensity * 0.05 * size, high_intensity * 0.05 * size);
    
    vec2 pos = _st - center;
    pos = high_rotate2d(0.832) * pos;
    pos += center;
    
    return high_Square(pos, center1, size) + high_Square(pos, center2, size) + high_Square(pos, center3, size);
  
}

vec4 CartoonProtagonist2(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    for(int i = 0; i < int(high_u_time); i++){
        vec2 position = high_random(float(i));

        float ifDraw = high_DrawHeart(st, position, 1.0);
        if(ifDraw != 0.0)
        	color = ifDraw * vec3(0.075,0.075,0.075);
        ifDraw = high_DrawHeart(st, position, 1.0 - 0.03);
        if(ifDraw != 0.0)
        	color = ifDraw * high_displayColor;
        position = position - vec2(0.0, 0.025);
        ifDraw = high_DrawHeart(st, position, 0.5 + 0.03);
        if(ifDraw != 0.0)
        	color = ifDraw * vec3(0.075,0.075,0.075);
        ifDraw = high_DrawHeart(st, position, 0.5);
        if(ifDraw != 0.0)
        	color = ifDraw * high_displayColor.zyx;
    }
    
    return vec4(color,1.0);
}