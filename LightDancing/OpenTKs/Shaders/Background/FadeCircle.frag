vec4 FadeCircle(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    
    vec2 pos = st - 0.5;
    float length = length(pos);
    
    //color = vec3(length - fract(u_time / 3.0));
	color = (smoothstep(0.0, 0.5, length) - smoothstep(0.5, low_intensity * 0.6, length )) * low_displayColor;
    return vec4(color,1.0);
}