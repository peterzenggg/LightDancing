
float halfcircle(vec2 _st, vec2 center, vec2 yThreshold, float size){
    vec2 pos = _st - center;
    float length = length(pos);
    
    float fill = 0.0;
    float bias = ( yThreshold.y - (_st.y-center.y)) / (yThreshold.y - yThreshold.x);
    if(pos.y > yThreshold.x && pos.y <= yThreshold.y){

        fill = step(length, size  -0.054 * pow(bias,4.0));

    }
    
    return fill;
}

vec3 bread(vec2 _st, vec2 center, float size){
    vec3 display = vec3(0.0);
    float burgerLength = 0.432;
    vec2 position = center - vec2(0.000,0.500) * size;
    float fill = halfcircle(_st, position, vec2(0.2, burgerLength) * size, burgerLength * size);
    if(fill != 0.0)
    	display = vec3(1.000,0.758,0.358);

    return display;
}

float rect(vec2 _st, vec2 center, vec2 size){
    vec2 result = smoothstep(center - size / 2.0, center, _st) - smoothstep(center ,center + size / 2.0, _st);
    
    return result.x * result.y;
}

vec3 lettuce(vec2 _st, vec2 center, float size){
    vec3 displayColor = vec3(0.0);
    
    vec2 pos = _st - center;
    float length = length(pos);
    float a = acos(pos.x/length);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    
    float index = a / (2.0 * PI) * 3.14 * 40.0;
    float y = sin(index) * 0.03 + size;
    if(pos.y < -0.5)
    displayColor = (step(y , length)  - step(y + 0.15, length)) * vec3(0.484,1.000,0.316);
    return displayColor;
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

float sesame(vec2 _st, vec2 center, float size){
    float index = (_st.y - center.y + 0.3) * 6.0;
    float x = smoothstep(center.x - 0.17 * index * size, center.x + 0.05 * index * size, _st.x) - smoothstep(center.x - 0.05 * index * size, center.x + 0.17 * index * size, _st.x);
    float y = smoothstep(center.y - 0.3 * size, center.y - 0.0, _st.y) - smoothstep(center.y, center.y + 0.3 * size, _st.y);
    
    return x * y;
}

vec3 hamburger(vec2 _st, vec2 center){
    
    vec3 color  = vec3(0.0);
    
    vec2 position = vec2(0.510,0.890);
    float draw = rect(_st, position + vec2(0.0, -0.34), vec2(0.65, 0.15));
    if(draw > 0.005)
        color =  vec3(1.000,0.222,0.029);
    draw = rect(_st, position + vec2(0.02, -0.6), vec2(0.75, 0.25));
    if(draw > 0.05)
        color =  vec3(1.000,0.758,0.358);
    draw = rect(_st, position + vec2(0.02, -0.46), vec2(0.68, 0.15));
    if(draw > 0.005)
        color =  vec3(0.360,0.248,0.067);
    
    
    vec3 fill = lettuce(_st, position + vec2(0.0, 0.26), 0.5);
    if(fill != vec3(0.0))
        color = fill;
    fill = bread(_st, position, 1.0);
    if(fill != vec3(0.0))
        color = fill;
    draw = sesame(_st, position + vec2(0.2, -0.2), 0.05);
    draw += sesame(_st, position + vec2(0.060,-0.130), 0.05);
    draw += sesame(_st, position + vec2(-0.030,-0.160), 0.05);
    draw += sesame(_st, position + vec2(-0.180,-0.180), 0.05);
    draw += sesame(_st, position + vec2(-0.090,-0.240), 0.05);
    draw += sesame(_st, position + vec2(0.100,-0.240), 0.05);
    if(draw > 0.02)
        color = vec3(1.0);
    return color;
}
vec4 Hamburger(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    
    vec3 color  = vec3(0.300,0.472,0.935);
    vec2 melt_st = mix(vec2(noise(st + u_time + low_u_time)), st, sin(u_time / 10.0) + cos(u_time)); 
    
    if(hamburger(melt_st, vec2(0.5, 0.5)) != vec3(0.0))
    color = hamburger(melt_st, vec2(0.5, 0.5));
    
    
    return vec4(color, 1.0);
}