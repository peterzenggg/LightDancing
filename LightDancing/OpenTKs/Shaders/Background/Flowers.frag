mat2 low_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec2 low_rotateCoord(vec2 _st, float angle){
    _st -= vec2(0.5);
    _st = low_rotate2d(low_intensity * 10.0 * angle) * _st;
    _st += vec2(0.5);
    return _st;
    
}
vec4 Flowers(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	
    //st *= 5.0;
    vec3 color = vec3(0.0);

    for(int i = 0; i < 10; i++){
    	vec2 rotateSt = low_rotateCoord(st, float(i) / 10.);
        vec2 pos = vec2(0.5)-rotateSt;
        
        float r = length(pos)* mod(low_intensity * 10.0 + 1.0 * float(i) , 10.0);
        float a = atan(pos.y,pos.x);

        float f = abs(cos(a*3.));

    	color += ((step(f, r) - step(f+0.1, r))) * low_displayColor;
    }

    return vec4(color, 1.0);
}