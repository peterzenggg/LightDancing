float low_halfcircle(vec2 _st, vec2 center, float size){
    vec2 pos = _st - center;
    float length = length(pos);

    
    float fill = 0.0;
    if(pos.x >= 0.0 )
        fill = step(length, size);

    return fill;
}

float low_rect(vec2 _st, vec2 center, vec2 size){
    vec2 result = smoothstep(center - size / 2.0, center, _st) - smoothstep(center ,center + size / 2.0, _st);
    
    return result.x * result.y;
}

vec3 low_bottle(vec2 _st, vec2 center, float size){
    vec3 display = vec3(0.0);
    
    float fill = low_rect(_st, center, vec2(0.3, 0.6) * size);
    if(fill != 0.0)
    	display = vec3(1.000,0.169,0.207);
    fill = low_rect(_st, center - vec2(0.0, 0.3) * size, vec2(0.2, 0.05) * size);
    if(fill != 0.0)
    	display = vec3(1.000,0.169,0.207);
    fill = low_rect(_st, center - vec2(0.0,0.320) * size, vec2(0.05, 0.12) * size);
    if(fill != 0.)
        display = vec3(1.000,0.169,0.207);

    return display;
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

    vec2 u = f*f*(3.0-2.0*f);

    return mix( mix( dot( random2(i + vec2(0.0,0.0) ), f - vec2(0.0,0.0) ),
                     dot( random2(i + vec2(1.0,0.0) ), f - vec2(1.0,0.0) ), u.x),
                mix( dot( random2(i + vec2(0.0,1.0) ), f - vec2(0.0,1.0) ),
                     dot( random2(i + vec2(1.0,1.0) ), f - vec2(1.0,1.0) ), u.x), u.y);
}

vec3 hand(vec2 _st, vec2 center, float size){
    vec3 displayColor = vec3(0.0);
    vec2 fingerSize = vec2(0.45, 0.1) * size;
    
    for(int i = -1; i < 3; i++){
        vec2 position = center + vec2(0.0, fingerSize.y * 0.8) * float(i);
        float fill = low_rect(_st, position, fingerSize);
        if(fill > 0.05)
            displayColor = vec3(1.0);
    }
	
    return displayColor;
}

vec3 bottleHand(vec2 _st, vec2 center, float size){
    
    vec3 color  = vec3(0.0);
    
    float fill = low_halfcircle(_st, center + vec2(0.05, 0.06) * size, 0.2 * size);
    if(fill != 0.0)
        color =  vec3(1.000,0.696,0.386);
    vec3 draw = low_bottle(_st, center, size);
    if(draw != vec3(0.0))
        color =  vec3(1.000,0.222,0.029);
    //hand
    draw = hand(_st, center, size);
    if(draw != vec3(0.0))
        color =  vec3(1.000,0.614,0.338);
    return color;
}

vec3 rainbow(vec2 _st, vec2 center){
    vec2 pos = _st - center;
    float length = length(pos);
    float a = acos(pos.x/length);
    vec3 result;
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    
    float index = floor(a / 3.14 * 10.0);
    if(index == 12.0)
        result = vec3(1.000,0.212,0.019);
    else if(index == 13.0)
        result = vec3(1.000,0.625,0.042);
    else if(index == 14.0)
        result = vec3(1.000,0.998,0.064);
    else if(index == 15.0)
        result = vec3(0.506,1.000,0.089);
    else if(index == 16.0)
        result = vec3(0.320,0.388,1.000);
    else if(index == 17.0)
        result = vec3(1.000,0.215,0.783);
    
    return result;
}

vec4 BottleRainbow(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    
    vec3 color  = vec3(0.300,0.472,0.935);
    vec2 melt_st = mix(vec2(noise(st + u_time)), st, sin(u_time / 10.0) + cos(u_time)); 
    
    vec2 position = vec2(0.530,0.800);
    position += noise(position + u_time + low_u_time);
    float size = 0.520;
    vec3 fill = rainbow(melt_st, position - vec2(0.0, 0.385) * size);
    if(fill != vec3(0.0))
    	color = fill;
    
    fill = bottleHand(st, position, size);
    if(fill != vec3(0.0))
    	color = fill;
    
    
    
    return vec4(color, 1.0);
}