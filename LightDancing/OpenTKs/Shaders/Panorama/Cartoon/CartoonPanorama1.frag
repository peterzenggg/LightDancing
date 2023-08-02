float mid_Square(vec2 _st, vec2 center, float length){
    float x = step(center.x - mid_intensity * length, _st.x) - step(center.x + mid_intensity * length, _st.x);
    float y = step(center.y - mid_intensity * length, _st.y) - step(center.y + mid_intensity * length, _st.y);
    
    return x * y;
    
}

mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float DrawHeart(vec2 _st, vec2 center, float size){
    float length = 0.3 * size;
    vec2 center1 = center - vec2(mid_intensity * length);
    vec2 center2 = center - vec2(mid_intensity * length, -mid_intensity * length);
    vec2 center3 = center - vec2(-mid_intensity * length, mid_intensity * length);
    
    vec2 pos = _st - center;
    pos = mid_rotate2d(0.832) * pos;
    pos += center;
    
    return mid_Square(pos, center1, length) + mid_Square(pos, center2, length) + mid_Square(pos, center3, length);
  
}

vec4 CartoonPanorama1(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    int index = int(mod(mid_u_time, 4.0));
    
    vec2 position;
    if(index == 0){
        position = vec2(0.25);
    }
    else if(index == 1){
        position = vec2(0.25, 0.75);
    }
    else if(index == 2){
        position = vec2(0.75, 0.25);
    }
    else if(index == 3){
        position = vec2(0.75);
    }
    
    color = DrawHeart(st, position, 1.0) * mid_displayColor * mid_intensity;
    float fill = DrawHeart(st, position, 0.8 + 0.01);
    if(fill != 0.0)
        color = fill * vec3(0.065,0.065,0.065) * mid_intensity;
    
    fill = DrawHeart(st, position, 0.8);
    if(fill != 0.0)
        color = fill * mid_displayColor.zyx * mid_intensity;
    fill = DrawHeart(st, position, 0.6 + 0.01);
    if(fill != 0.0)
        color = fill * vec3(0.065,0.065,0.065) * mid_intensity;
    fill = DrawHeart(st, position, 0.6);
    if(fill != 0.0)
        color = fill * mid_displayColor.yzx * mid_intensity;
    fill = DrawHeart(st, position, 0.4 + 0.01);
    if(fill != 0.0)
        color = fill * vec3(0.065,0.065,0.065) * mid_intensity;
	fill = DrawHeart(st, position, 0.4);
        if(fill != 0.0)
            color = fill * mid_displayColor.zxy * mid_intensity;
    fill = DrawHeart(st, position, 0.2 + 0.01);
    if(fill != 0.0)
        color = fill * vec3(0.065,0.065,0.065) * mid_intensity;
    fill = DrawHeart(st, position, 0.2);
        if(fill != 0.0)
            color = fill * mid_displayColor.zzy * mid_intensity;
    
    return vec4(color,1.0);
}