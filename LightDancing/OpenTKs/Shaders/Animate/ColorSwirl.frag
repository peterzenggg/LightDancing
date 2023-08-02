float plotCircle(float length, float radius){
    float thickness = 0.03;
    return step(radius - thickness, length) - step(radius+thickness, length);
}

vec3 hsb2rgb( float x ){
    float devide = 0.11;
    if(x < devide)
        return mix(color_1, color_2, x / devide);
    else if(x < devide * 2.0)
        return mix(color_2, color_3, (x - devide) / devide);
    else if(x < devide * 3.0)
        return mix(color_3, color_4, (x - devide * 2.0) / devide);
    else if(x < devide * 4.0)
        return mix(color_4, color_5, (x - devide * 3.0) / devide);
    else if(x < devide * 5.0)
        return mix(color_5, color_6, (x - devide * 4.0) / devide);
    else if(x < devide *6.0)
        return mix(color_6, color_7, (x - devide * 5.0) / devide);
    else if(x < devide *7.0)
        return mix(color_7, color_8, (x - devide * 7.0) / devide);
    else
        return mix(color_8, color_1, (x - devide * 8.0) / 1.0 - devide * 8.0);
    
}

vec3 CircleRamp(float time){
    vec2 st = gl_FragCoord.xy/u_resolution;
    vec3 color = vec3(0.0);
	vec2 pos = st - 0.5;
    float length = length(pos);
    color = hsb2rgb(mod(time + length , 1.0));
    
    return color;
}

vec4 ColorSwirl() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    float _size = 300.0;
    vec2 pos = st - vec2(0.5);
    float length = length(pos);
    float a = acos(pos.x/length);
    if(pos.y < .0){
        a = 2.0 * PI - a;
    }
    int index = int(mod(a / (2.0*PI/ _size) + u_time * 300.0, _size));
    
    vec3 color = vec3(0.);
    float temp = 1.0 - abs(sin((u_time / 10.)));
    if(temp < 0.1)
        temp = 0.1;

    float thickness = 0.003 * temp;
    //thickness = 0.003 * 0.3;
    vec3 displayColor = vec3(0.0);
    
    float radius = thickness * _size;
    
    for(int i = 0; i < 300; i++){
        color += (plotCircle(length, float(i)*radius+thickness*float(index))) * CircleRamp(temp);
    }
    return vec4(color,1.0);
}