float mid_light(vec2 _st, vec2 center){
    vec2 pos = _st - center;
    float length = length(pos);
    float a = acos(pos.x/length);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    
    float width = mid_intensity / 30.0;
    if(center.y > 0.5){
        if(a > (1.476 - width) * PI && a < (1.520 + width) * PI){
            return 1.0;
        }
        else{
        return 0.0;
        }
    }
    else{
        if(a > (0.484 - width) * PI && a < (0.516 + width)  * PI){
            return 1.0;
        }
        else{
        return 0.0;
        }
    }
}

vec4 CartoonPanorama5(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    int index = int(mod(mid_u_time, 2.0));
    if(index == 0){
        for(int i = 0; i < 5; i++){
            vec3 displayColor;
            if(mod(float(i), 3.0) == 0.0){
                displayColor = mid_displayColor;
            }
            else if(mod(float(i), 3.0) == 1.0){
                displayColor = mid_displayColor.zyx;
            }
            else if(mod(float(i), 3.0) == 2.0){
                displayColor = mid_displayColor.xzy;
            }
            vec2 position = vec2(1.0 / 5.0 * float(i) + 0.1, 1.1);
            color += mid_light(st, position) * displayColor * mid_intensity;
        }
    }
    else if(index == 1){
        for(int i = 0; i < 5; i++){
            vec3 displayColor;
            if(mod(float(i), 3.0) == 0.0){
                displayColor = mid_displayColor;
            }
            else if(mod(float(i), 3.0) == 1.0){
                displayColor = mid_displayColor.zyx;
            }
            else if(mod(float(i), 3.0) == 2.0){
                displayColor = mid_displayColor.xzy;
            }
            vec2 position = vec2(1.0 / 5.0 * float(i) + 0.1, -0.1);
            color += mid_light(st, position) * displayColor * mid_intensity;
        }
    }
    

    return vec4(color,1.0);
}