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

float Ghost(vec2 _st, vec2 center, float size){
    _st.x = fract(_st.x);
    float fill = 0.0;
    float upBound = pow((pow(0.07 * size, 2.0) - pow(_st.x - center.x, 2.0)), 0.5);
    float bottomBound = -abs(sin(_st.x * 45.0 / size)) * 0.02 * size + 0.25;
    float y = step(_st.y - center.y, upBound) - step(_st.y, bottomBound);
    float x = step(center.x - 0.2 * size, _st.x) - step(center.x + 0.2 * size, _st.x);
    if( x * y > 0.0)
    	fill = 1.0;
    
    return fill;
}

float rect(vec2 _st, vec2 center, vec2 size){
    vec2 result = smoothstep(center - size / 2.0, center, _st) - smoothstep(center ,center + size / 2.0, _st);
    
    return result.x * result.y;
}

float GhostContour(vec2 _st, vec2 center, float size){
    float fill = Ghost(_st, center, size) - Ghost(_st, center, size * 0.95);
    float eye = rect(_st, center - vec2(0.03, -0.02) * size, vec2(0.02, 0.05) * size);
    if(eye > 0.2)
        fill = eye;
    eye = rect(_st, center + vec2(0.03, 0.02) * size, vec2(0.02, 0.05) * size);
    if(eye > 0.2)
        fill = eye;
    
    
    return fill;
}



vec4 Pacman() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.);
    
    vec2 melt_st = mix(vec2(noise(st + u_time + low_u_time)), st, sin(u_time / 10.0) + cos(u_time)); 
    
    float fill = GhostContour(fract(melt_st), vec2(0.4,0.5), 5.0);
    if(fill != 0.0)
    	color = vec3(1.0, 0.0, 0.0);
    fill = GhostContour(fract(melt_st), vec2(0.5), 5.0);
    if(fill != 0.0)
    	color = vec3(0.000,1.000,0.011);
    fill = GhostContour(fract(melt_st), vec2(0.6, 0.5), 5.0);
    if(fill != 0.0)
    	color = vec3(0.229,0.082,1.000);

    return vec4(color,1.0);
}