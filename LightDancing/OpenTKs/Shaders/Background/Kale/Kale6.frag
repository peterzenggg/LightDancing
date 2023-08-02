vec2 low_random (in float x) {
    return vec2(fract(sin(x)*1e4 * 5.0), fract(cos(x)*1e4));
}

float low_random1 (in float x) {
    return fract(sin(x)*1e4);
}

float low_circle(vec2 _st, vec2 center){
    vec2 pos = _st - center;
    float length = length(pos);
    return smoothstep(0.03 - 0.04, 0.03, length) - smoothstep(0.03, 0.03 + 0.04, length);
}
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

vec4 Kale6(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color  = vec3(0.0);
    
    //Circle
    vec2 pos = st - 0.5;
    float lengthr = length(pos);
    float a = acos(pos.x/lengthr);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    
    //Kale_st
    float sides = mod(u_time, 40.0);
    sides = 12.0;
    float index = floor(a / (2.0 * PI) * sides);
    vec2 kale_st;
    if(mod(index, 2.0) == 0.0){
            kale_st.x = lengthr * cos(mod(a, 2.0 * PI / sides)) * 2.0;
            kale_st.y = lengthr * sin(mod(a, 2.0 * PI / sides)) * 2.0;    
    }
    else if(mod(index, 2.0) == 1.0){
            kale_st.x = lengthr * cos(mod(-a, 2.0 * PI / sides)) * 2.0;
            kale_st.y = lengthr * sin(mod(-a, 2.0 * PI / sides)) * 2.0;
    }
    
    kale_st = vec2(low_noise(kale_st + u_time / 10.0));
    kale_st = mix(vec2(low_noise(kale_st)), kale_st, sin((u_time) / 15.0) + cos((u_time) / 1.5)); 

    //Draw
    float indexD = mod(low_u_time, 4.0);
    vec2 positionMove;
    if(indexD >= 0.0 && indexD < 1.0)
        positionMove.x += u_time / 100.0;
    else if(indexD >= 1.0 && indexD < 2.0)
        positionMove.x -= u_time / 100.0;
    else if(indexD >= 2.0 && indexD < 3.0)
        positionMove.y += u_time / 100.0;
    else if(indexD >= 3.0 && indexD < 4.0)
        positionMove.y -= u_time / 100.0;
    
    float result = 0.0;
    for(int i = 0; i < 10; i++){
        for(int j = 0; j < 10; j++){
            vec2 bias = vec2(0.05, (mod(float(i), 2.0) + 1.0) * 0.05 - 0.025);
            vec2 position = fract(vec2(1.0 / 10.0 * float(i), 1.0 / 10.0 * float(j)) + bias + positionMove);
            result = low_circle(kale_st, position);
            if(result != 0.0)
                color += result * low_displayColor * 0.4;
        }
    }
 
    return vec4(color, 1.0);
}