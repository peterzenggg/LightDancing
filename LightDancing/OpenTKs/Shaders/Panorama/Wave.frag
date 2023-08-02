float mid_plot_sin(vec2 _st, float hight){
    float width = 0.5 / (mid_u_time + 1.0);
    float y = sin( (_st.x *  mid_intensity + mid_intensity)* 10.0) * 0.1 + hight;
    return step(y - width / 2.0, _st.y) - step(y + width / 2.0, _st.y);
}
vec4 Wave(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    color = mid_plot_sin(st, 0.5) * mid_displayColor;
    
    for(int i = 0; i < int(mid_u_time); i++){
        float hight = 1.0 / mid_u_time * float(i);
        color += mid_plot_sin(st, hight) * mid_displayColor * mid_intensity;
    }

    return vec4(color,1.0);
}