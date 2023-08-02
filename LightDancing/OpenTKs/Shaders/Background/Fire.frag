vec2 fire_random(vec2 st){
    st = vec2( dot(st,vec2(127.1,311.7)),
              dot(st,vec2(269.5,183.3)) );
    return -1.0 + 2.0*fract(sin(st)*43758.5453123);
}


float fire_noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);

    vec2 u = f*f*(3.056-2.120*f);

    return mix( mix( dot( fire_random(i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ),
                     dot( fire_random(i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                mix( dot( fire_random(i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ),
                     dot( fire_random(i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
}

float plot(vec2 _st, float y){
    return step(y-0.9, _st.y) - step(y+0.02, _st.y);
}


vec4 Fire() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    st.x *= u_resolution.x/u_resolution.y;
	vec2 _st = st * 20.0;
    _st.y = _st.y / 6.0;
    vec3 color = vec3(0.);
    
    _st += u_time;
    _st.y -= u_time * 3.496;
    
    st.y -= (0.5 * low_intensity);
    
    float y = fire_noise(_st);
    float yellow = fire_noise(_st + 0.4);
    float blue = fire_noise(_st + 0.4);
    
    float fill = plot(st, y);
    float fillY = plot(st, yellow);
    float fillB = plot(st, blue / 1.0);
    
    //color = vec3(fill, fillY, fillB * sin(u_time));
    color = fill * low_displayColor;
    color += fillY * (1.0 - low_displayColor);

    return vec4(color,1.0);
}