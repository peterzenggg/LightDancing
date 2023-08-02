float BG_Square(vec2 _st, vec2 center){
    float x = step(center.x - low_intensity * 0.15, _st.x) - step(center.x + low_intensity * 0.15, _st.x);
    float y = step(center.y - low_intensity * 0.15, _st.y) - step(center.y + low_intensity * 0.15, _st.y);
    
    return x * y;
    
}

mat2 BG_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float BG_Heart(vec2 _st, vec2 center){
    vec2 center1 = center - vec2(low_intensity * 0.15);
    vec2 center2 = center - vec2(low_intensity * 0.15, -low_intensity * 0.15);
    vec2 center3 = center - vec2(-low_intensity * 0.15, low_intensity * 0.15);
    
    vec2 pos = _st - center;
    pos = BG_rotate2d(0.832) * pos;
    pos += center;
    
    return BG_Square(pos, center1) + BG_Square(pos, center2) + BG_Square(pos, center3);
  
}

vec4 ClappingBall(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    float ballSize = 0.3;
    int index = int(mod(low_u_time, 2.0));
    color += BG_Heart(st, vec2(0.5)) * vec3(1.0, 0.0, 0.0) * low_intensity;
    if(index == 1){
        if(st.x > 0.5 + low_intensity / 2.0){
            vec2 pos = vec2(0.5 + low_intensity / 2.0 - st.x, 0.5 - st.y);
            float length = length(pos);

            color += step(length, ballSize) * low_displayColor;

        }
        else if(st.x < 0.5 - low_intensity / 2.0){
            vec2 pos = vec2(0.5 - low_intensity / 2.0 - st.x, 0.5 - st.y);
            float length = length(pos);

            color += step(length, ballSize) * low_displayColor;
        }
    }
    else{
        if(st.y > 0.5 + low_intensity / 2.0){
            vec2 pos = vec2(0.5 - st.x, 0.5 + low_intensity / 2.0 - st.y);
            float length = length(pos);

            color += step(length, ballSize) * low_displayColor;

        }
        else if(st.y < 0.5 - low_intensity / 2.0){
            vec2 pos = vec2(0.5 - st.x, 0.5  - low_intensity / 2.0 - st.y);
            float length = length(pos);

            color += step(length, ballSize) * low_displayColor;
        }
    }
    
    
    return vec4(color,1.0);
}