float low_circle(vec2 _st, vec2 center, vec2 yThreshold, float size){
    vec2 pos = _st - center;
    float length = length(pos);
    
    float fill = 0.0;
    if(pos.y > yThreshold.x && pos.y < yThreshold.y){
        fill = step(length, size);
    }
    
    return fill;
}

float low_rect(vec2 _st, vec2 center, vec2 size){
    vec2 result = step(center - size / 2.0, _st) - step(center + size / 2.0, _st);
    
    return result.x * result.y;
}

float low_ship(vec2 _st, vec2 center){
    vec2 position = center - vec2(0.000,0.500);
    float fill = low_circle(_st, position, vec2(0.25, 0.4), 0.4) + low_circle(_st, center, vec2(-0.4, -0.25), 0.4);
    position = center;
    position.y = position.y - 0.4 + 0.15 * 2.0 -0.03;
    if(low_circle(_st, position, vec2(0.0, 0.2), 0.15) != 0.0)
		fill = low_circle(_st, position, vec2(0.0, 0.15), 0.2);
    if(low_circle(_st, position, vec2(0.0, 0.2), 0.13) != 0.0 && fill != 0.0)
        fill -= low_circle(_st, position, vec2(0.0, 0.2), 0.13);
    position.y -= 0.06;
    if(low_rect(_st, position, vec2(0.26)) != 0.0)
        fill -= low_rect(_st, position, vec2(0.26, 0.12));
    return fill;
}

float head(vec2 _st, vec2 center, float size){
    float index = (_st.y - center.y + 0.3) * 6.0;
    float x = smoothstep(center.x - 0.17 * index * size, center.x + 0.05 * index * size, _st.x) - smoothstep(center.x - 0.05 * index * size, center.x + 0.17 * index * size, _st.x);
    float y = smoothstep(center.y - 0.3 * size, center.y - 0.0, _st.y) - smoothstep(center.y, center.y + 0.3 * size, _st.y);
    
    return x * y;
}

float eye(vec2 _st, vec2 center, float size){
    float index = _st.y * 5.0;
    float x = smoothstep(center.x - 0.12 * size * index, center.x - 0.0 * size * index, _st.x) - smoothstep(center.x + 0.0 * size * index, center.x + 0.12 * size * index, _st.x);
    float y = smoothstep(center.y - 0.46  * size, center.y - 0.0 * size, _st.y) - smoothstep(center.y + 0.0 * size, center.y + 0.4 * size, _st.y);
    
    return x * y;
}

float body(vec2 _st, vec2 center, float size){
    float index = (1.0 - _st.y - center.y + 0.3) * 6.0;
    float x = smoothstep(center.x - 0.17 * index * size, center.x + 0.05 * index * size, _st.x) - smoothstep(center.x - 0.05 * index * size, center.x + 0.17 * index * size, _st.x);
    float y = smoothstep(center.y - 0.3 * size, center.y - 0.0, _st.y) - smoothstep(center.y, center.y + 0.3 * size, _st.y);
    
    return x * y;
}

float alien(vec2 _st, vec2 center){
    float fill = 0.0;
    vec2 position = center;
    position.y -= 0.1;
    if(head(_st, position, 0.4) > 0.2)
        fill = 1.0;
    position += vec2(-0.04, 0.01);
    if(eye(_st, position, 0.06) > 0.2)
        fill -= 1.0;
    position += vec2(0.08, 0.0);
    if(eye(_st, position, 0.06) > 0.2)
        fill -= 1.0;
    position = center - vec2(0.0, 0.25);
    if(body(_st, position, 0.5) > 0.2)
        fill = 1.0;
    
    return fill;
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


vec4 AlienShip(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color  = vec3(0.0);
    vec2 melt_st = mix(vec2(noise(st)), st, sin(u_time / 10.0) + cos(u_time)); 
    vec2 position = vec2(0.690,0.800);
    position += noise(position + u_time + low_u_time);
    float draw = low_ship(st, position);
    color = draw * low_displayColor;
    draw = alien(st ,position);
    if(draw != 0.0)
        color = low_displayColor;
    position.y -= 0.25;
    if(rainbow(melt_st, position) != vec3(0.0) && color == vec3(0.0))
        color = rainbow(melt_st, position);
    
    return vec4(color, 1.0);
}