float Ring(vec2 _st, float thickness, float delay){
    vec2 pos = _st - 0.5;
    float length = length(pos);
    return step(fract(delay), length) - step(fract(delay) + thickness, length);
}
vec4 ColorRings() {
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    float time = u_time / 20.;
    color = Ring(st, 1.0/ 8., time) * color_1;
    //color += Ring(st, 1.0/ 8., time + 1./8.) * color_2;
    color += Ring(st, 1.0/ 8., time + 2./8.) * color_3;
    //color += Ring(st, 1.0/ 8., time + 3./8.) * color_4;
    color += Ring(st, 1.0/ 8., time + 4./8.) * color_5;
    //color += Ring(st, 1.0/ 8., time + 5./8.) * color_6;
	color += Ring(st, 1.0/ 8., time + 6./8.) * color_7;
    //color += Ring(st, 1.0/ 8., time + 7./8.) * color_8;
    
    return vec4(color,1.0);
}