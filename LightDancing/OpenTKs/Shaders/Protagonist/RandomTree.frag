vec2 high_random (in float x) {
    return vec2(fract(sin(x)*1e4+0.2), mod(cos(x)*1e4, 0.2));
}

float high_rec(vec2 _st, vec2 position, vec2 size){
    float x = step(position.x - size.x / 2.0, _st.x) - step(position.x + size.x / 2.0, _st.x);
    float y = step(position.y - size.y / 2.0, _st.y) - step(position.y + size.y / 2.0, _st.y);
    
    return x*y;
}

float high_tree(vec2 _st, vec2 position){
    float result;
    for(int i = 0; i < 7; i++){
        vec2 size = vec2(0.3 *  high_intensity / 7.0 * float(7-i), 0.05);
        vec2 pos_position = vec2(position.x, position.y+ 0.06 * float(i));
        result += high_rec(_st, pos_position, size);
    }
    return result;
}

vec4 RandomTree() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);

    for(int i = 0; i < int(high_u_time); i++){
        vec2 position = vec2(high_random(float(i)));
       color += high_tree(st, position) * high_displayColor;
    }
    
    return vec4(color,1.0);
}