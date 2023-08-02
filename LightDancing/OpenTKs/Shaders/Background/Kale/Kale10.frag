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

float random (in float x) {
    return fract(sin(x)*1e4);
}

float Circle(vec2 _st, vec2 center, float size){
    vec2 pos = center - _st;
    float length = length(pos);
    
    return 1.0-smoothstep(size-0.01, size+0.1, length);
    
}

vec4 Kale10(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color  = vec3(0.0);
    
    vec2 melt_st = st * 5.0;
    
    st *=  1.0;
    //Circle
    vec2 pos = fract(st) - 0.5;
    float length = length(pos);
    float a = acos(pos.x/length);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    
    //kale_st
    float sides = 4.0;
    float index = floor(a / (2.0 * PI) * sides);
    float para = mod(a / (2.0 * PI), 0.1) / sides;
    vec2 kale_st;
    if(mod(index, 2.0) == 0.0){
            kale_st.x = fract(length * cos(mod(a, 2.0 * PI / sides))) * 2.0;
            kale_st.y = fract(length * sin(mod(a, 2.0 * PI / sides))) * 2.0;    
    }
    else if(mod(index, 2.0) == 1.0){
            kale_st.x = fract(length * cos(mod(-a, 2.0 * PI / sides))) * 2.0;
            kale_st.y = fract(length * sin(mod(-a, 2.0 * PI / sides))) * 2.0;
    }

    kale_st = mix(vec2(low_noise(kale_st + (u_time + low_u_time / 4.0) / 12.0)), kale_st, sin((u_time) / 35.0) + cos((u_time) / 2.5)); 
    
    
    float freq = u_time / 5.;
    for(int i = 0; i < 50; i++){
        vec2 position;
        position.x = random(float(i));
        position.y = random(float(i) + floor(float(i)));
        float size = abs(fract(float(i)))*random(float(i))*0.1;
        float ifDraw = Circle(fract(kale_st), position, size);
        float ifDraw_R = Circle(fract(kale_st), position + 0.05, size);
        float ifDraw_G = Circle(fract(kale_st), position - 0.05, size);
        float ifDraw_B = Circle(fract(kale_st), position + vec2(0.05, -0.05), size);
        color += vec3(ifDraw * 0.2);
        color += vec3(ifDraw_R * 0.2, 0.0, 0.0);
        color += vec3(0.0, ifDraw_G * 0.2, 0.0);
        color += vec3(0.0, 0.0, ifDraw_B * 0.2);
    }

    return vec4(color, 1.0);
}