float mid_Square(vec2 _st, vec2 center){
    float x = step(center.x - mid_intensity * 0.3, _st.x) - step(center.x + mid_intensity * 0.3, _st.x);
    float y = step(center.y - mid_intensity * 0.3, _st.y) - step(center.y + mid_intensity * 0.3, _st.y);
    
    return x * y;
    
}

mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float DrawHeart(vec2 _st, vec2 center){
    vec2 center1 = center - vec2(mid_intensity * 0.3);
    vec2 center2 = center - vec2(mid_intensity * 0.3, -mid_intensity * 0.3);
    vec2 center3 = center - vec2(-mid_intensity * 0.3, mid_intensity * 0.3);
    
    vec2 pos = _st - center;
    pos = mid_rotate2d(0.832) * pos;
    pos += center;
    
    return mid_Square(pos, center1) + mid_Square(pos, center2) + mid_Square(pos, center3);
  
}
vec4 Heart(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    int index = int(mod(mid_u_time, 4.0));
    
    if(index == 0){
        color = DrawHeart(st, vec2(0.25)) * mid_displayColor * mid_intensity;
    }
    else if(index == 1){
        color = DrawHeart(st, vec2(0.25, 0.75)) * mid_displayColor * mid_intensity;
    }
    else if(index == 2){
        color = DrawHeart(st, vec2(0.75, 0.25)) * mid_displayColor * mid_intensity;
    }
    else if(index == 3){
        color = DrawHeart(st, vec2(0.75)) * mid_displayColor * mid_intensity;
    }
    

    return vec4(color,1.0);
}