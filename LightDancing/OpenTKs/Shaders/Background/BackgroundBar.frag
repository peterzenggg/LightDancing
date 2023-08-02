vec4 BackgroundBar() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	vec3 color = vec3(0.0);
    if(st.x <= 0.5 && st.x >= 0.25){
        color = low_displayColor * low_intensity;
    }
    
    

    return vec4(color,1.0);
}