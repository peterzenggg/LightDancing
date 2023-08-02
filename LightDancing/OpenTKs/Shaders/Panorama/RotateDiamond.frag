mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float mid_Line(vec2 _st){
    return step(mid_intensity, _st.x) - step(mid_intensity + 0.2, _st.x);
}

vec4 RotateDiamond(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(.0);
    
    st *= vec2(2.0);

    int index = int(st.x) + int(st.y) * 2;
    float hight = mid_intensity * 0.5;
    float angle = (1.0 - mid_intensity) * -2.288;
    if(index == 0){
        vec2 pos = st - 1.0;
        pos = mid_rotate2d(0.872 + angle) * pos;
        pos += 1.000;
        
        pos.x += hight;
        
        color += mid_Line(fract(pos)) * mid_displayColor * mid_intensity;
    }
    else if(index == 1){
        vec2 pos = st - 1.0;
        pos = mid_rotate2d(-4.016 - angle) * pos;
        pos += 1.0;
        
        pos.x += hight;
        
        color += mid_Line(fract(pos)) * mid_displayColor * mid_intensity;
    }
    else if(index == 2){
        vec2 pos = st - 1.0;
        pos = mid_rotate2d(-0.880 - angle) * pos;
        pos += 1.0;
        
        pos.x += hight;
        
        color += mid_Line(fract(pos)) * mid_displayColor * mid_intensity;
    }
    else if(index == 3){
        vec2 pos = st - 1.0;
        pos = mid_rotate2d(-2.264 + angle) * pos;
        pos += 1.0;
        
        pos.x += hight;
        
        color += mid_Line(fract(pos)) * mid_displayColor * mid_intensity;
    }
    
    
    return  vec4(color,1.0);
}