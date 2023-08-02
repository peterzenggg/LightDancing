vec4 KickBar() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	vec3 color = vec3(0.0);
    if(st.x <= 0.25){
        color = kick_displayColor * kick_intensity;
    }
    
    

    return vec4(color,1.0);
}