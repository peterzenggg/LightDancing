float random (in vec2 st) {
    return fract(sin(dot(st.xy,
                         vec2(12.9898,78.233)))
                * 43758.5453123);
}

// Value noise by Inigo Quilez - iq/2013
// https://www.shadertoy.com/view/lsf3WH
float noise(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);
    vec2 u = f*f*(3.0-2.0*f);
    return mix( mix( random( i + vec2(0.0,0.0) ),
                     random( i + vec2(1.0,0.0) ), u.x),
                mix( random( i + vec2(0.0,1.0) ),
                     random( i + vec2(1.0,1.0) ), u.x), u.y);
}

mat2 rotate2d(float angle){
    return mat2(cos(angle),-sin(angle),
                sin(angle),cos(angle));
}

float line(in vec2 pos, float b){
    float scale = 3.0;
    pos *= scale;
    //if(mod(floor(pos.x), 2.0) == 0.0)
        return smoothstep(0.0,
                        b*0.044,
                        abs((cos(pos.x*3.1415)))*.5) * 0.5 + smoothstep(0.0,
                        b*0.028,
                        abs((sin(pos.y*3.1415)))*.5) * 0.5;
}

vec4 FluidGrid() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    st = st - 0.5;
    st *= rotate2d(u_time);
    st += 0.5;
    st.x *= u_resolution.x/u_resolution.y;
    vec2 pos = st.yx*vec2(1.,1.);

    // Add noise
    pos = rotate2d(noise(pos + u_time) + u_time) * pos;
    // Draw lines
    float pattern = step(0., line(pos,noise(pos + u_time)));
    
    vec3 displayC = mix(color_1, color_5, fract(pos.x));
    displayC = mix(displayC, color_2, fract(pos.y));
    displayC = mix(displayC, color_3, fract(pos.x));
    displayC = mix(displayC, color_4, fract(pos.y));
    displayC = mix(displayC, color_6, fract(pos.x));
    displayC = mix(displayC, color_7, fract(pos.y));
    displayC = mix(displayC, color_8, fract(pos.x));
    vec3 color;
    color = pattern * displayC;
	
    return vec4(vec3(color),1.0);
}