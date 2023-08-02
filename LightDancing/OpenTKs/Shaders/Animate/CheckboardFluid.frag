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


vec4 CheckboardFluid() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = color_1;
    
    vec3 thisColor;
    if(mod(floor(u_time), 8.0) == 0.0){
        thisColor = color_1;
        color = color_8;
    }
    else if(mod(floor(u_time), 8.0) == 1.0){
        thisColor = color_2;
        color = color_1;
    }
    else if(mod(floor(u_time), 8.0) == 2.0){
        thisColor = color_3;
        color = color_2;
    }
    else if(mod(floor(u_time), 8.0) == 3.0){
        thisColor = color_4;
        color = color_3;
    }
    else if(mod(floor(u_time), 8.0) == 4.0){
        thisColor = color_5;
        color = color_4;
    }
    else if(mod(floor(u_time), 8.0) == 5.0){
        thisColor = color_6;
        color = color_5;
    }
    else if(mod(floor(u_time), 8.0) == 6.0){
        thisColor = color_7;
        color = color_6;
    }
    else if(mod(floor(u_time), 8.0) == 7.0){
        thisColor = color_8;
        color = color_7;
    }
    
    vec2 _st = st * 3.0;
    vec2 melt_st = mix(vec2(low_noise(_st + u_time)), _st, sin(u_time / 10.0) + cos(u_time)); 
    if(mod(floor(melt_st.x) + floor(melt_st.y), 2.0) == 1.0)
        color = mix(color, thisColor,  mod(fract(melt_st.x) + fract(melt_st.y), 4.0));
	_st = st * 20.0;
    _st.y = _st.y / 6.0;
    
    
    _st += u_time;
    _st.y -= u_time * 3.496;
    
    //color = vec3(fill, fillY, fillB * sin(u_time));
    return vec4(color,1.0);
}