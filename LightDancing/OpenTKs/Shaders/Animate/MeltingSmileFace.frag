mat2 rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec2 random2(vec2 st){
    st = vec2( dot(st,vec2(127.1,311.7)),
              dot(st,vec2(269.5,183.3)) );
    return -1.0 + 2.0*fract(sin(st)*43758.5453123);
}

// Gradient Noise by Inigo Quilez - iq/2013
// https://www.shadertoy.com/view/XdXGW8
float noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);

    vec2 u = f*f*(3.056-2.120*f);

    return mix( mix( dot( random2(i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ),
                     dot( random2(i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                mix( dot( random2(i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ),
                     dot( random2(i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
}

float circle(vec2 _st, vec2 center, float size){
    vec2 pos = _st - center;
    float length = length(pos);
    
    return step(length, size) - step(length + 0.01, size);
}

float smile(vec2 _st, vec2 center, float size){
    vec2 pos = _st - center;
    float length = length(pos);
    
    float result;
    if(pos.y < .0){
        result = step(length, 0.1 * size) - step(length, 0.09 * size);
    }    
    else{
        result = 0.0;
    }
    return result;
}

float smileFace(vec2 _st, vec2 center, float size){
    float fill = circle(_st, center, 0.2 * size);
    vec2 eye = center - vec2(0.080,-0.060) * size;
    if(circle(_st, eye, 0.01 * size) != 0.0){
    	fill = circle(_st, eye, 0.01 * size);
    }
    eye = center + vec2(0.080, 0.060) * size;
    if(circle(_st, eye, 0.01 * size) != 0.0){
    	fill = circle(_st, eye, 0.01 * size);
    }
    vec2 smileP = center - vec2(0.0, 0.02) * size;
    if(smile(_st, smileP, size) != 0.0){
    	fill = smile(_st, smileP, size);
    }
    return fill;
}

vec4 MeltingSmileFace() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    st = st - 0.5;
    st *= rotate2d(u_time);
    st += 0.5;
    st.x *= u_resolution.x/u_resolution.y;
    vec3 color = color_1;
    vec3 displayColor_1, displayColor_2;
    if(mod(floor(u_time), 4.0) == 0.0){
        displayColor_1 = color_3;
        displayColor_2 = color_5;
    }
    else if(mod(floor(u_time), 4.0) == 1.0){
        displayColor_1 = color_2;
        displayColor_2 = color_6;
    }
    else if(mod(floor(u_time), 4.0) == 2.0){
        displayColor_1 = color_3;
        displayColor_2 = color_7;
    }
    else if(mod(floor(u_time), 4.0) == 3.0){
        displayColor_1 = color_4;
        displayColor_2 = color_8;
    }
    vec2 melt_st = mix(vec2(noise(st + u_time)), st, sin(u_time / 10.0) + cos(u_time)); 
    //melt_st = st;
    float size = 1.528;
    float fill = smileFace(fract(melt_st), vec2(0.5), size);
    if(fill != 0.0)
        color += displayColor_1;
    fill = smileFace(fract(melt_st), vec2(0.5) + vec2(0.07, 0.0) * size, size);
    if(fill != 0.0)
        color += displayColor_2;

    return vec4(color,1.0);
}