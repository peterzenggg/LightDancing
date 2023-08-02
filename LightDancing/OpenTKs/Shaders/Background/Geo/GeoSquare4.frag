mat2 BG_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float BG_square(vec2 _st, float angle, float size){
    vec2 pos = _st - 0.5;
    pos *= BG_rotate2d(angle);
    pos += 0.5;
    
    vec2 result = step(0.5 - size / 2.0, pos) - step(0.5 + size / 2.0, pos);
    return result.x * result.y;
}

vec4 GeoSquare4(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color  = vec3(0.0);
    vec3 fillcolor;
    
    //low_intensity + 0.1 to prevent /0
    for(int i = 0; i < int(1.0 / (pow(low_intensity, 0.5) + 0.1) * 10.0); i++){
        float squareCount = 1.0 / (pow(low_intensity, 0.5) + 0.1) * 10.0;
        //Assign Color
        if(mod(float(i), 2.0) == 0.0)
            fillcolor = low_displayColor * low_intensity;
        else
            fillcolor = vec3(0.001);
        
        float angle = 2.0 * PI / squareCount * float(int(squareCount) - i);
        float size = 1.0 / squareCount * float(int(squareCount) - i);
        float draw = BG_square(st, angle, size);
        if(draw != 0.0)
            color = draw * fillcolor;
    }
    return vec4(color, 1.0);
}