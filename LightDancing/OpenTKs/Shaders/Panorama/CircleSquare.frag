float mid_circle(vec2 _st, vec2 center){
    vec2 pos = center - _st;
    float length = length(pos);
    float radius = mid_intensity * 0.15;
    return smoothstep(0.0, radius / 2.0, length) - smoothstep(radius/2.0, radius, length);
}

float mid_square(vec2 _st, vec2 center){
    float radius = mid_intensity * 0.15;
    float x = smoothstep((center.x - radius/2.0), center.x, _st.x) - smoothstep(center.x, (center.x + radius/2.0), _st.x);
    float y = smoothstep((center.y - 0.1), center.y, _st.y) - smoothstep(center.y, (center.y + 0.1), _st.y);
    
    return x * y;
}

mat2 mid_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}
vec4 CircleSquare(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    int index = int(mod(mid_u_time, 2.0));

    
    for(int i = 0; i < int( 5.0 ) ; i++){
        
        if(index == 0){
            vec2 position = vec2(1.0 / 5.0 * float(i) + 0.1, 0.1);
            vec2 position2 = vec2(1.0 / 5.0 * float(i) + 0.1, 0.9);
            float draw;
            if(mod(float(i), 2.0) == 0.0){
            	draw += mid_circle(st, position);
                draw += mid_circle(st, position2);
            }
            else{
                //draw = mid_square(st, position);
            }
            color += draw * mid_displayColor * mid_intensity; 
        }
        else if(index == 1){
            float draw;
            vec2 position = vec2(1.0 / 5.0 * float(i) + 0.1, 0.1);
            vec2 position2 = vec2(1.0 / 5.0 * float(i) + 0.1, 0.9);
            if(mod(float(i), 2.0) == 0.0){
            	//draw = mid_circle(st, position);
            }
            else{
                draw += mid_square(st, position);
                draw += mid_square(st, position2);
            }
            color += draw * mid_displayColor * mid_intensity; 

            
            
        }
    }

    return vec4(color,1.0);
}