vec3 BG_HalfCircle(vec2 _st, float radius, vec2 center){
    vec3 color;
    vec2 pos = _st - center;
    float length = length(pos);
    color = (1. - smoothstep(radius, radius + 0.1, length)) * color_1;
    color += (1. - smoothstep(radius / 8. * 7.,radius / 8. * 7. + 0.1, length)) * (color_2 - color_1);
    color += (1. - smoothstep(radius / 8. * 6., radius / 8. * 6. + 0.1,  length)) * (color_3 - color_2);
    color += (1. - smoothstep(radius / 8. * 5.,radius / 8. * 5. + 0.1, length)) * (color_4 - color_3);
    color += (1. - smoothstep(radius / 8. * 4.,radius / 8. * 4.+ 0.1, length)) * (color_5 - color_4);
    color += (1. - smoothstep(radius / 8. * 3.,radius / 8. * 3.+ 0.1, length)) * (color_6 - color_5);
    color += (1. - smoothstep(radius / 8. * 2.,radius / 8. * 2.+ 0.1, length)) * (color_7 - color_6);
    color += (1. - smoothstep(radius / 8. * 1.,radius / 8. * 1.+ 0.1, length)) * (color_8 - color_7);
    return color;
}

vec3 BG_hsb2rgb( in vec3 c ){
    vec3 rgb = clamp(abs(mod(c.x*6.0+vec3(0.0,4.0,2.0),
                             6.0)-3.0)-1.0,
                     0.0,
                     1.0 );
    rgb = rgb*rgb*(3.0-2.0*rgb);
    return c.z * mix(vec3(1.0), rgb, c.y);
}

vec4 HalfColorCircle(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    float freq = floor(mod(u_time , 120.0));
    
    if(freq < 20.0){
        for(int i = 0; i < 10; i++){

            color = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + mod(u_time, 10.0), vec2(0.5, -0.2));
                //color = BG_hsb2rgb(vec3(float(i) * 0.1+ fract(u_time / 5.0), 1.0, ifFill));
        }
    }
    else if(freq < 40.0){
        for(int i = 0; i < 10; i++){

            color = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + mod(u_time, 10.0), vec2(0.5, 1.2));
            
        }
    }
    else if(freq < 60.0){
        for(int i = 0; i < 10; i++){

            color = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + mod(u_time, 10.0), vec2(-0.2, 0.5));
            
        }
    }
    else if(freq < 80.0){
        for(int i = 0; i < 10; i++){

            color = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + mod(u_time, 10.0), vec2(1.2, 0.5));
            
        }
    }
    else if(freq < 100.0){
        for(int i = 0; i < 10; i++){

            color = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + mod(u_time, 10.0), vec2(1.2, 0.5));
            
        }
        for(int i = 0; i < 10; i++){
			if(color == vec3(0.0))
            	color = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + mod(u_time, 10.0), vec2(-0.2, 0.5));
            
        }
    }
    else if(freq < 120.0){
        for(int i = 0; i < 10; i++){

            color = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + mod(u_time, 10.0), vec2(0.5, 1.2));
            
        }
        for(int i = 0; i < 10; i++){
			if(color == vec3(0.0))
            	color = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + mod(u_time, 10.0), vec2(0.5, -0.2));
            
        }
    }

    return vec4(color,1.0);
}