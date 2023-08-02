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

float circle(vec2 _st, vec2 position, float size){
    vec2 pos = _st - position;
    float length = length(pos);
    
    return step(length, size);
}

float covercircle(vec2 _st, vec2 position, float size, float cover){
    vec2 pos = _st - position;
    float length = length(pos);
    float result = 0.0;
    if(pos.y <= - size * cover || pos.y >=  size * cover)
        result = step(length, size);
    
    return result;
}

vec3 eyes(vec2 _st, vec2 position){
    vec3 display;
    vec2 center = position - vec2(0.2, 0.0);
    float fill = circle(_st, center, 0.1);
    display = fill * vec3(1.0);
    fill = circle(_st, center, 0.02);
    if(fill != 0.0)
        display = fill * vec3(0.0001);
    fill = covercircle(_st, center, 0.1, fract(u_time));
    if(fill != 0.0)
        display = fill * vec3(0.985,0.920,0.429);
    
    center = position + vec2(0.2, 0.0);
    fill = circle(_st, center, 0.1);
    if(fill != 0.0)
    display = fill * vec3(1.0);
    if(fill != 0.0)
    	fill = circle(_st, center, 0.02);
    if(fill != 0.0)
        display = fill * vec3(0.0001);
    fill = covercircle(_st, center, 0.1, fract(u_time));
    if(fill != 0.0)
        display = fill * vec3(0.985,0.920,0.429);
    
    return display;
}
vec4 Monster(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color  = vec3(0.0);
    
    //st = mix(vec2(noise(st)), st, sin(u_time / 10.0) + cos(u_time)); 
    
    vec2 melt_st = st * 5.0;
    
    //melt_st = mix(vec2(noise(melt_st)), melt_st, sin(u_time / 100.0) + cos(u_time)); 
     st *= 1.0;
    //Circle
    vec2 pos = fract(st) - 0.5;
    float lengthR = length(pos);
    float a = acos(pos.x/lengthR);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
   
    float sides = 4.0;
    float index = floor(a / (2.0 * PI) * sides);
    float para = mod(a / (2.0 * PI), 0.1) / sides;
    vec2 kale_st;
    if(mod(index, 2.0) == 0.0){
            kale_st.x = lengthR * cos(mod(a, 2.0 * PI / sides));
            kale_st.y = lengthR * sin(mod(a, 2.0 * PI / sides));    
    }
    else if(mod(index, 2.0) == 1.0){
            kale_st.x = lengthR * cos(mod(-a, 2.0 * PI / sides));
            kale_st.y = lengthR * sin(mod(-a, 2.0 * PI / sides));
    }
    
    kale_st = mix(vec2(noise(kale_st + (u_time + low_u_time) / 4.0)), kale_st, sin((u_time) / 15.0) + cos((u_time) / 1.5)); 
    vec2 posC = kale_st - 0.0;
    float lengthC = length(posC);
    float aC = acos(pos.x/lengthC);
    if(pos.y < .0){
        //aC = 2.0 * PI - a;
    }
    
	float sidesC = 20.0;
    float indexC = floor(aC / (2.0 * PI) * sidesC + lengthC * 10.0) ;
    vec3 displayColor;
	if(mod(indexC, 2.0) == 0.0){
             displayColor = vec3(0.602,1.000,0.832); 
    }
    else if(mod(indexC, 2.0) == 1.0){
            displayColor = vec3(1.000,0.544,0.832);
    }
    
    color = displayColor;
    
    vec3 draw = eyes(st, vec2(0.50,0.750));
    if(draw != vec3(0.0))
        color = draw;
    

    return vec4(color, 1.0);
}