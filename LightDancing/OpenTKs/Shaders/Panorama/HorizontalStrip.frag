float mid_Line(vec2 _st, vec2 position, vec2 _size){
    vec2 result = smoothstep(position - _size / 2.0, position , _st) - smoothstep(position , position + _size / 2.0 , _st);
    return result.x * result.y;
}

float mid_random (in float x) {
    return fract(sin(x)*1e4);
}

vec4 HorizontalStrip(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    int index = int(mod(mid_u_time, 2.0));
    if(index == 0){
        for(int i = 0; i < int(mid_u_time); i++){
            vec2 position = vec2(fract(1.0 - u_time - sin(float(i))), mid_random(float(i)));
            float ifFill = mid_Line(st, position, vec2(mid_intensity, 0.5 /(mid_u_time+1.0)));
            color += ifFill * mid_displayColor;
        }
    }
    else{
        for(int i = 0; i < int(mid_u_time); i++){
            vec2 position = vec2(fract(1.0 + u_time - sin(float(i))), mid_random(float(i)));
            float ifFill = mid_Line(st, position, vec2(mid_intensity, 0.5 / (mid_u_time+1.0)));
            color += ifFill * mid_displayColor;
        }
    }
    return vec4(color,1.0);
}