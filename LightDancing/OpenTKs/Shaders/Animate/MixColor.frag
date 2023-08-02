mat2 rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec4 MixColor() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    st = st - 0.5;
    st *= rotate2d(u_time);
    st += 0.5;
    vec3 colorLB = mix(color_1, color_8, abs(sin(u_time / 3.0)));
	vec3 colorLT = mix(color_2, color_7, abs(sin(u_time / 1.0)));
    vec3 colorRB = mix(color_3, color_6, abs(sin(u_time / 7.0)));
    vec3 colorRT = mix(color_4, color_5, abs(sin(u_time / 4.0)));
    
    vec3 colorB = mix(colorLB, colorRB, st.x);
    vec3 colorT = mix(colorLT, colorRT, st.x);
    vec3 color = mix(colorB, colorT, st.y);
    return vec4(color,1.0);
}