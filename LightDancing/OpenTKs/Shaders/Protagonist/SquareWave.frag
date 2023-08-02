float high_plot(vec2 _st, float y){
     return step(y + 0.5 - 0.5 * high_intensity , _st.y) - step(y + 0.5 + 0.5 * high_intensity, _st.y);
 }

vec4 SquareWave() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
	
    float y = floor(sin((st.x *  high_intensity + high_intensity)* 10.0 * float(high_u_time))) * 0.1;
    color = high_plot(st, y) * high_displayColor;
    
    return vec4(color,1.0);
}