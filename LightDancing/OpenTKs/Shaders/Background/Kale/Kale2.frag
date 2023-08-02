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

    vec2 u = f*f*(3.0-2.0*f);

    return mix( mix( dot( low_random2(i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ),
                     dot( low_random2(i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                mix( dot( low_random2(i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ),
                     dot( low_random2(i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
}

vec4 Kale2(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color  = vec3(0.0);

    vec2 melt_st = st * 5.0;

    //Circle
    vec2 pos = st - 0.5;
    float lengthr = length(pos);
    float a = acos(pos.x/lengthr);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    
    float sides = 40.0;
    float index = floor(a / (2.0 * PI) * sides);
    float para = mod(a / (2.0 * PI), 0.1) / sides;
    vec2 kale_st;
    if(mod(index, 2.0) == 0.0){
            kale_st.x = lengthr * cos(mod(a, 2.0 * PI / sides));
            kale_st.y = lengthr * sin(mod(a, 2.0 * PI / sides));    
    }
    else if(mod(index, 2.0) == 1.0){
            kale_st.x = lengthr * cos(mod(-a, 2.0 * PI / sides));
            kale_st.y = lengthr * sin(mod(-a, 2.0 * PI / sides));
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
        
        float size = float(10 - i) / 10.0;
        float y = size;
        
        vec2 still = kale_st;
        
        float wave = 0.02 + 0.05 * float(8 - i);
        float target = sin(index) * wave * low_intensity + size;
        float fill = step(lengthr, y);
        vec2 kale_pos =  kale_st - 0.5;
        float kale_length = length(kale_pos);
        float draw = step(y, kale_st.x) - step(y + 0.4 * size, kale_st.x);
        kale_st = vec2(low_noise(kale_st * 1.0 + u_time / 10.0));
        kale_st = mix(vec2(low_noise(kale_st + low_u_time / 30.0)), kale_st, sin((u_time) / 30.0) + cos((u_time) / 4.5)); 
        draw = smoothstep(target - 0.2, target, kale_length) - smoothstep(target, target + 0.2 * size, kale_length);
        
        if(draw != 0.0)
            color += draw * displayColor * 0.4;
    }

    return vec4(color, 1.0);
}