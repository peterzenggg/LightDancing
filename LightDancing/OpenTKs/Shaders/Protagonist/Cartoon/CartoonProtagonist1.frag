﻿mat2 high_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec2 high_random (in float x) {
    return vec2(fract(sin(x)*1e4 + 0.2), fract(cos(x)*1e4));
}

float high_Rec(vec2 _st, vec2 size, vec2 position){
    float x = step(position.x - size.x / 2.0, _st.x) - step(position.x + size.x / 2.0, _st.x);
    float y = step(position.y - size.y / 2.0, _st.y) - step(position.y + size.y / 2.0, _st.y);
    
    return x*y;
}

float high_cross(vec2 _st, vec2 position, float size){
    float result = 0.0;
    result += high_Rec(_st, vec2(0.2,0.05) * high_intensity * size, position);
    
    
    _st -= position;
    _st = _st * high_rotate2d(PI/2.0);
    _st += position;
    
    if(result == 0.0){
    	result += high_Rec(_st, vec2(0.2,0.05) * high_intensity * size, position);
    }
    return result;
}

vec4 CartoonProtagonist1(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    for(int i = 0; i < int(high_u_time); i++){
        vec2 position = high_random(float(i));

        float ifDraw = high_cross(st, position, 1.0);
        if(ifDraw != 0.0)
        	color = ifDraw * vec3(0.075,0.075,0.075);
        ifDraw = high_cross(st, position, 1.0 - 0.03);
        if(ifDraw != 0.0)
        	color = ifDraw * high_displayColor;
        
        ifDraw = high_cross(st, position, 0.7 + 0.03);
        if(ifDraw != 0.0)
        	color = ifDraw * vec3(0.075,0.075,0.075);
        ifDraw = high_cross(st, position, 1.0 - 0.01);
        ifDraw = high_cross(st, position, 0.7);
        if(ifDraw != 0.0)
        	color = ifDraw * high_displayColor.zyx;
    }
    
    return vec4(color,1.0);
}