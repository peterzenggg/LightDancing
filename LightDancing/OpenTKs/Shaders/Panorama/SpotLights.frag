float mid_circle(vec2 _st, vec2 center){
    vec2 pos = center - _st;
    float length = length(pos);
    float radius = mid_intensity * 0.15;
    return smoothstep(0.0, radius / 2.0, length) - smoothstep(radius/2.0, radius, length);
}
vec4 SpotLights(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    for(int i = 0; i < 5; i++){
        vec2 position = vec2(1.0 / 5.0 * float(i) + 0.1, 0.1);
        float draw = mid_circle(st, position);
        color += draw * mid_displayColor * mid_intensity;
        
        vec2 position2 = vec2(1.0 / 5.0 * float(i) + 0.1, 0.9);
        float draw2 = mid_circle(st, position2);
        color += draw2 * mid_displayColor * mid_intensity;
    }

    return vec4(color,1.0);
}