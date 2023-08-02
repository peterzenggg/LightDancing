vec2 low_random (in vec2 st) {
    return vec2(fract(sin(st.x)*1e4 * 5.0), fract(cos(st.y)*1e4));
}

mat2 low_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float low_head(vec2 _st, vec2 center, float size){
    float index = _st.y * 5.0;
    float x = smoothstep(center.x - 0.12 * size * index, center.x - 0.0 * size * index, _st.x) - smoothstep(center.x + 0.0 * size * index, center.x + 0.12 * size * index, _st.x);
    float y = smoothstep(center.y - 0.3  * size, center.y - 0.0 * size, _st.y) - smoothstep(center.y + 0.0 * size, center.y + 0.3 * size, _st.y);
    
    return x * y;
}

float low_eye(vec2 _st, vec2 center, float size){
    float index = _st.y * 5.0;
    float x = smoothstep(center.x - 0.12 * size * index, center.x - 0.0 * size * index, _st.x) - smoothstep(center.x + 0.0 * size * index, center.x + 0.12 * size * index, _st.x);
    float y = smoothstep(center.y - 0.46  * size, center.y - 0.0 * size, _st.y) - smoothstep(center.y + 0.0 * size, center.y + 0.4 * size, _st.y);
    
    return x * y;
}

float low_smile(vec2 _st, vec2 center){
    vec2 pos = _st - center;
    float length = length(pos);
    float fill = 0.0;
    
    if(pos.y < 0.0)
        fill = step(0.03, length) - step(0.04, length);
    
    return fill;
}

vec3 low_alien(vec2 _st, vec2 center){
    vec3 displayColor;
    float draw = low_head(_st, center, 1.0);
    
    if(draw > 0.5)
        displayColor = vec3(0.130,0.130,0.130);
    draw = low_head(_st, center, 0.9);
    if(draw > 0.5)
        displayColor = vec3(0.406,0.995,0.142);
    
    draw = low_eye(_st, center - vec2(0.060,-0.030), 0.2);
    if(draw > 0.5)
        displayColor = vec3(0.00001);
    
    draw = low_eye(_st, center - vec2(-0.060,-0.03), 0.2);
    if(draw > 0.5)
        displayColor = vec3(0.00001);
    draw = low_smile(_st, center - vec2(0.0, 0.06));
    if(draw > 0.5)
        displayColor = vec3(0.00001);
    return displayColor;
}

float random (in vec2 st) {
    return fract(sin(dot(st.xy,
                         vec2(12.9898,78.233)))
                 * 43758.5453123);
}

// 2D Noise based on Morgan McGuire @morgan3d
// https://www.shadertoy.com/view/4dS3Wd
float noise (in vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);

    // Four corners in 2D of a tile
    float a = random(i);
    float b = random(i + vec2(1.0, 0.0));
    float c = random(i + vec2(0.0, 1.0));
    float d = random(i + vec2(1.0, 1.0));

    // Smooth Interpolation

    // Cubic Hermine Curve.  Same as SmoothStep()
    vec2 u = f*f*(3.0-2.0*f);
    // u = smoothstep(0.,1.,f);

    // Mix 4 coorners percentages
    return mix(a, b, u.x) +
            (c - a)* u.y * (1.0 - u.x) +
            (d - b) * u.x * u.y;
}

vec4 FadeAlien() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	
    vec2 rotateSt = st - 0.5;
    rotateSt *= low_rotate2d(u_time/5.0);
    rotateSt += 0.5;
    
    vec2 melt_st = mix(st, vec2(noise(st + u_time + low_u_time)), sin(u_time));
    
    vec2 counterclockSt = st - 0.5;
    counterclockSt *= low_rotate2d(-u_time/5.0);
    counterclockSt += 0.5;
    
    vec3 color = vec3(0.);
    
    vec2 coordinate = rotateSt;
    
    if(st.x < 0.5){
        //coordinate = counterclockSt;
    }
    vec2 pos = coordinate - 0.5;
    float length = length(pos);
    float a = acos(pos.x/length);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    
    float index = a / (2.0 * PI) * 3.14 * 60.0 * low_intensity;

    for(int i = 0; i < 15; i++){
        vec3 displayColor;
        if(mod(float(i), 4.0) == 0.0)
            displayColor = vec3(0.840,0.641,0.341);
        else if(mod(float(i), 4.0) == 1.0)
            displayColor = vec3(1.000,0.037,0.190);
        else if(mod(float(i), 4.0) == 2.0)
            displayColor = vec3(0.257,1.000,0.463);
        else
            displayColor = vec3(0.001,0.769,1.000);
        
        float size = 1.0 / 15.0 * float(15 - i);
        float wave = 0.01 * float(15 - i);
        float y =  size + noise(st * 5.0) * sin(u_time) ;
        float fill = step(length, y);
        if(fill != 0.0)
            color = displayColor;
    }
    

    
    vec3 et = low_alien(melt_st, vec2(0.5));
    if(et != vec3(0.0))
        color = et;
    
    
    return vec4(color,1.0);
}