vec2 low_random2(vec2 st){
    st = vec2( dot(st,vec2(127.1,311.7)),
              dot(st,vec2(269.5,183.3)) );
    return -1.0 + 2.0*fract(sin(st)*43758.5453123);
}

// Gradient Noise by Inigo Quilez - iq/2013
// https://www.shadertoy.com/view/XdXGW8
float low_noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);

    vec2 u = f*f*(3.056-2.120*f);

    return mix( mix( dot( low_random2(i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ),
                     dot( low_random2(i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                mix( dot( low_random2(i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ),
                     dot( low_random2(i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
}

float plot(vec2 _st, float y){
    return step(y-0.9, _st.y) - step(y+0.02, _st.y);
}


vec4 CheckboardFire() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    
    vec2 _st = st * 3.0;
    vec2 melt_st = mix(vec2(low_noise(_st + u_time)), _st, sin(u_time / 10.0) + cos(u_time)); 
    if(mod(floor(melt_st.x) + floor(melt_st.y), 2.0) == 1.0)
        color = vec3(1.0);
	_st = st * 20.0;
    _st.y = _st.y / 6.0;
    
    
    _st += u_time;
    _st.y -= u_time * 3.496;
    
    st.y -= low_intensity / 2.0;
    
    float y = low_noise(_st);
    float yellow = low_noise(_st + 0.4);
    float blue = low_noise(_st + 0.8);
    
    float fill = plot(st, y);
    float fillY = plot(st, yellow);
    float fillB = plot(st, blue / 1.0);
    
    //color = vec3(fill, fillY, fillB * sin(u_time));
    if(fill != 0.0)
    	color = vec3(1.0, 0.0, 0.0);
    if(fillY != 0.0)
    	color += vec3(1.0, 1.0, 0.0);
    
    

    return vec4(color,1.0);
}