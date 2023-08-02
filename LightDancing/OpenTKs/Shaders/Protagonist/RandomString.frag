float high_random (in float x) {
    return fract(sin(x)*1e4);
}

mat2 high_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float high_Line(vec2 _st, vec2 center){
    vec2 pos = _st - center;
    pos = high_rotate2d(u_time * center.x) * pos;
    pos += center;
    float x = smoothstep(center.x - high_intensity/2.0, center.x, pos.x) - smoothstep(center.x , center.x + high_intensity/2.0, pos.x);
    float y = smoothstep(center.y - 0.02, center.y, pos.y) - smoothstep(center.y , center.y + 0.02, pos.y);
    
    return x * y;
}
vec4 RandomString(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    for(int i = 0; i < int(high_u_time); i++){
        vec2 position = vec2(high_random(float(i)), fract(1.0 - u_time - sin(float(i))));
        float draw = high_Line(st, position);
    
		color += draw * high_displayColor ;
    }
    
    return vec4(color,1.0);
}