vec4 Rectangle(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    int index = int(mod(low_u_time, 6.0));
    if(index == 0){
        color = step(st.x, low_intensity) * low_displayColor;
    }
    else if(index == 1){
        color = (1.0 - step(st.x, 1.0 - low_intensity)) * low_displayColor;
    }
    else if(index == 2){
        color = step(st.y, low_intensity) * low_displayColor;
    }
    else if(index == 3){
        color = (1.0 - step(st.y, 1.0 - low_intensity)) * low_displayColor;
    }
    else if(index == 4){
        color = (1.0 - step(st.x, 1.0 - low_intensity)) * low_displayColor;
        color += step(st.y, low_intensity) * low_displayColor;
    }
    else if(index == 5){
        color = step(st.y, low_intensity) * low_displayColor;
        color += (1.0 - step(st.y, 1.0 - low_intensity)) * low_displayColor;
    }
        
    
    return vec4(color,1.0);
}