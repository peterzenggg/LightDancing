float plotCircle(float length, float radius){
    float thickness = 0.03;
    return step(radius - thickness, length) - step(radius+thickness, length);
}

vec4 Swirl(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	float _size = 300.;
    vec2 pos = st - vec2(0.5);
    float length = length(pos);
    float a = acos(pos.x/length);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    int index = int(mod(a / (2.0*PI/ _size) + u_time * 500.0, _size));
    
    
    vec3 color = vec3(0.);
    float thickness = 0.003 * ( 1.0 - mid_intensity);
    float radius = thickness * _size;
    for(int i = 0; i < 300; i++){
        
        color += (plotCircle(length, float(i)*radius+thickness*float(index))) * mid_displayColor;
    }

    return vec4(color,1.0);
}