float mid_circle(vec2 _st, vec2 center){
    vec2 pos = center - _st;
    float length = length(pos);
    float radius = mid_intensity * 0.15;
    return smoothstep(0.0, radius / 2.0, length) - smoothstep(radius/2.0, radius, length);
}
mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}
vec4 DiamondLights(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    int index = int(mod(mid_u_time, 4.0));
    st -= 0.5;
    st = mid_rotate2d(0.75) * st;
    st += 0.5;
    
    for(int i = 0; i < int( 5.0 * mid_intensity) ; i++){
        
        if(index == 0){
            vec2 position = vec2(1.0 / 5.0 * float(i) + 0.1, 0.1);
            float draw = mid_circle(st, position);
            color += draw * mid_displayColor * mid_intensity; 
        }
        else if(index == 1){
            vec2 position2 = vec2(1.0 / 5.0 * float(i) + 0.1, 0.9);
            float draw2 = mid_circle(st, position2);
            color += draw2 * mid_displayColor * mid_intensity;
        }
        else if(index == 2){
            vec2 position2 = vec2(0.1, 1.0 / 5.0 * float(i) + 0.1);
            float draw2 = mid_circle(st, position2);
            color += draw2 * mid_displayColor * mid_intensity;
        }
        else if(index == 3){
            vec2 position2 = vec2(0.9, 1.0 / 5.0 * float(i) + 0.1);
            float draw2 = mid_circle(st, position2);
            color += draw2 * mid_displayColor * mid_intensity;
        }
    }

    return vec4(color,1.0);
}