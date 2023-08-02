﻿vec2 low_random2(vec2 st){
    st = vec2( dot(st,vec2(127.1,311.7)),
              dot(st,vec2(269.5,183.3)) );
    return -1.0 + 2.0*fract(sin(st)*43758.5453123);
}

// Gradient Noise by Inigo Quilez - iq/2013
// https://www.shadertoy.com/view/XdXGW8
float low_noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);

    vec2 u = f*f*(3.0-2.0*f);

    return mix( mix( dot( low_random2(i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ),
                     dot( low_random2(i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                mix( dot( low_random2(i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ),
                     dot( low_random2(i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
}

vec4 Kale9(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color  = vec3(0.0);
    
    vec2 melt_st = st * 5.0;
    
    //Circle
    vec2 pos = st - 0.5;
    float length = length(pos);
    float a = acos(pos.x/length);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    

    float sides = 20.0;
    float index = floor(a / (2.0 * PI) * sides);
    float para = mod(a / (2.0 * PI), 0.1) / sides;
    vec2 kale_st;
    if(mod(index, 2.0) == 0.0){
            kale_st.x = length * cos(mod(a, 2.0 * PI / sides));
            kale_st.y = length * sin(mod(a, 2.0 * PI / sides));    
    }
    else if(mod(index, 2.0) == 1.0){
            kale_st.x = length * cos(mod(-a, 2.0 * PI / sides));
            kale_st.y = length * sin(mod(-a, 2.0 * PI / sides));
    }

	for(int i = 0; i < 10; i++){
        vec3 displayColor;
        if(mod(float(i), 4.0) == 0.0)
            displayColor = low_displayColor;
        else if(mod(float(i), 4.0) == 1.0)
            displayColor = low_displayColor.zxy;
        else if(mod(float(i), 4.0) == 2.0)
            displayColor = low_displayColor.yzx;
        else if(mod(float(i), 4.0) == 3.0)
            displayColor = low_displayColor.xzy;
        
        float size = float(i) / 10.0;
        float y = -kale_st.y + size;
        vec2 still = kale_st;
        kale_st = vec2(low_noise(fract(kale_st + (u_time + low_u_time) / 20.0)));
        kale_st = mix(vec2(low_noise(kale_st)), kale_st, sin((u_time) / 35.0) + cos((u_time) / 5.5)); 
        float draw = smoothstep(y - 0.08, y, kale_st.x) - smoothstep(y, y + 0.08, kale_st.x);
        if(draw != 0.0)
            color = draw * displayColor;
    }

    return vec4(color, 1.0);
}