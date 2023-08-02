vec4 ColorShift() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    
    st.x += u_time / 2.0;
    st.x = mod(st.x, 8.0);
    
    vec3 color = vec3(0.);
    if(floor(st.x) == 0.0){
        color = mix(color_1, color_2, fract(st.x));
    }
    else if(floor(st.x) == 1.0){
        color = mix(color_2, color_3, fract(st.x));
    }
    else if(floor(st.x) == 2.0){
        color = mix(color_3, color_4, fract(st.x));
    }
    else if(floor(st.x) == 3.0){
        color = mix(color_4, color_5, fract(st.x));
    }
    else if(floor(st.x) == 4.0){
        color = mix(color_5, color_6, fract(st.x));
    }
    else if(floor(st.x) == 5.0){
        color = mix(color_6, color_7, fract(st.x));
    }
    else if(floor(st.x) == 6.0){
        color = mix(color_7, color_8, fract(st.x));
    }
    else if(floor(st.x) == 7.0){
        color = mix(color_8, color_1, fract(st.x));
    }

    return vec4(color,1.0);
}