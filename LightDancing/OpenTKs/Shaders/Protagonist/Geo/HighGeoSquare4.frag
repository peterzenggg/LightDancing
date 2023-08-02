mat2 high_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float high_square(vec2 _st, float angle, float size){
    vec2 pos = _st - 0.5;
    pos *= high_rotate2d(angle);
    pos += 0.5;
    
    vec2 result = step(0.5 - size / 2.0, pos) - step(0.5 + size / 2.0, pos);
    return result.x * result.y;
}

vec4 HighGeoSquare4(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color  = vec3(0.0);
    vec3 fillcolor;
    
    //high_intensity + 0.1 to prevent /0
    for(int i = 0; i < int(1.0 / (high_intensity + 0.1) * 10.0); i++){
        float squareCount = 1.0 / (high_intensity + 0.1) * 10.0;
        //Assign Color
        if(mod(float(i), 2.0) == 0.0)
            fillcolor = high_displayColor;
        else
            fillcolor = vec3(0.001);
        
        float angle = 2.0 * PI / squareCount * float(int(squareCount) - i);
        float size = 1.0 / squareCount * float(int(squareCount) - i);
        float draw = high_square(st, angle, size);
        if(draw != 0.0)
            color = draw * fillcolor;
    }
    return vec4(color, 1.0);
}