vec2 high_random (in float x) {
    return vec2(fract(sin(x)*1e4 + 0.2), fract(cos(x)*1e4));
}

float high_circle(vec2 _st, vec2 center, float size){
    vec2 pos = _st - center;
    float length = length(pos);
    
    return step(length, size);
}

float high_smile(vec2 _st, vec2 center, float size){
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

vec3 smileFace(vec2 _st, vec2 center, float size){
    vec3 result = high_circle(_st, center, 0.22 * size) * high_displayColor;
    if(high_circle(_st, center, 0.2 * size) != 0.0)
        result = high_circle(_st, center, 0.2 * size) * vec3(0.990,0.879,0.251);
    vec2 eye = center - vec2(0.080,-0.060) * size;
    if(high_circle(_st, eye, 0.01  * size) != 0.0){
    	result = high_circle(_st, eye, 0.01 * size) * vec3(0.065,0.058,0.016);
    }
    eye = center + vec2(0.080, 0.060) * size;
    if(high_circle(_st, eye, 0.01 * size) != 0.0){
    	result = high_circle(_st, eye, 0.01  * size) * vec3(0.065,0.058,0.016);
    }
    vec2 smileP = center - vec2(0.0, 0.02) * size;
    if(high_smile(_st, smileP, size) != 0.0){
    	result = high_smile(_st, smileP, size) * vec3(0.065,0.058,0.016);
    }
    return result;
}

vec4 CartoonProtagonist5(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    for(int i = 0; i < int(high_u_time); i++){
        vec2 position = high_random(float(i));

        if(smileFace(st, position, high_intensity) != vec3(0.0))
        	color = smileFace(st, position, high_intensity);
    }
    
    return vec4(color,1.0);
}