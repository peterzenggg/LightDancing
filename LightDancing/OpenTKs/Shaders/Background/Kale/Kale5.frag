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

mat2 BG_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float BG_square(vec2 _st, float size, vec2 center){
    float fill = 0.0;
    vec2 result = step(center - size / 2.0, _st) - step(center + size / 2.0, _st);
    vec2 pos = _st - center;
    float length = length(pos);
    float angle = acos(pos.x/length);
    if(pos.y < .0){
        angle = 2.0 * PI - angle;
    }
    
    if(result.x * result.y == 1.0 && angle >= 0.0 && angle <= PI / 4.0)
        fill = 1.0;
    else if(result.x * result.y == 1.0)
        fill = 0.5;
    if(result.x * result.y == 1.0 && angle >= PI * 0.75 && angle <= PI * 1.25)
        fill = 1.0;
    else if(result.x * result.y == 1.0)
        fill = 0.5;
    return fill;
}

float BG_circle(vec2 _st, float size){
    vec2 pos = _st - 0.5;
    float length = length(pos);
    
    return step(length, size / 2.0);
}

vec4 Kale5(){
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
    

    float sides = 8.0;
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

	for(int i = 1; i < 3; i++){
        vec3 displayColor;
        if(mod(float(i), 4.0) == 0.0)
            displayColor = low_displayColor;
        else if(mod(float(i), 4.0) == 1.0)
            displayColor = low_displayColor.zxy;
        else if(mod(float(i), 4.0) == 2.0)
            displayColor = low_displayColor.yzx;
        else if(mod(float(i), 4.0) == 3.0)
            displayColor = low_displayColor.xzy;
        
        float size = 1.0 - pow(0.3 , float(i));
        
        vec2 still = kale_st;
        vec2 center = vec2(0.5, 0.5) ;
		kale_st = mix(vec2(low_noise(kale_st + u_time / 10.0 )), kale_st, sin((u_time) / 15.0 + low_u_time / 30.0 + pow(low_intensity, 7.0)));

        vec2 kale_pos =  kale_st - 0.5;
        float kale_length = length(kale_pos);
        vec2 rotate_st = kale_st - 0.5;
        rotate_st *= BG_rotate2d(0.5 * PI * float(i * 2));
        rotate_st += 0.5;
        
        float draw = BG_square(fract(kale_st), size, center);
        
        if(draw == 1.0)
            color += draw * displayColor * 0.4;
        if(draw == 0.5)
           color += draw * displayColor.zyx * 0.4;
    }

    return vec4(color, 1.0);
}