vec4 ProtagonistBar() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	vec3 color = vec3(0.0);
    if(st.x >= 0.75){
        color = high_displayColor * high_intensity;
    }
    
    

    return vec4(color,1.0);
}