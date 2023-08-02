float random (in float x) {
    return fract(sin(x)*1e4);
}

float DrawCircle(vec2 _st, vec2 center, float size){
    vec2 pos = _st - center;
    float length = length(pos);
    
    return 1.0 - step(size, length);
}

vec4 ColorBounceBall() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    st.x *= u_resolution.x/u_resolution.y;
    vec3 color = vec3(0.);
    vec3 _displayColor;

    for(int i = 0; i < 8; i++){
        if(i == 0)
            _displayColor = color_1;
        else if(i == 1)
            _displayColor = color_2;
        else if(i == 2)
            _displayColor = color_3;
        else if(i == 3)
            _displayColor = color_4;
        else if(i == 4)
            _displayColor = color_5;
        else if(i == 5)
            _displayColor = color_6;
        else if(i == 6)
            _displayColor = color_7;
        else if(i == 7)
            _displayColor = color_8;
        
        vec2 position = vec2(random(float(i)));
        float angle = random(float(i));

        position.x = mod(position.x + u_time * cos(angle), 2.0) > 1.0? abs(mod(position.x + u_time * cos(angle), 2.0) - 2.0): mod(position.x + u_time * cos(angle), 2.0);
        position.y = mod(position.y + u_time * sin(angle), 2.0) > 1.0? abs(mod(position.y + u_time * sin(angle), 2.0) - 2.0) : mod(fract(position.y + u_time * sin(angle)), 2.0);
        
        position.x *= u_resolution.x/u_resolution.y;
        position.y = position.y * 0.8 + 0.1;
        position.x = position.x * (u_resolution.x/u_resolution.y - 0.2)/(u_resolution.x/u_resolution.y ) + 0.1;
        float ifDraw = DrawCircle(st, position, 0.1 );
        if(ifDraw != 0.0)
            color = ifDraw * _displayColor;
    }

    return vec4(color,1.0);
}