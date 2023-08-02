float BG_HalfCircle(vec2 _st, float radius, vec2 center){
    vec2 pos = _st - center;
    float length = length(pos);
    return 1. - step(radius, length);
}

vec3 BG_hsb2rgb( in vec3 c ){
    vec3 rgb = clamp(abs(mod(c.x*6.0+vec3(0.0,4.0,2.0),
                             6.0)-3.0)-1.0,
                     0.0,
                     1.0 );
    rgb = rgb*rgb*(3.0-2.0*rgb);
    return c.z * mix(vec3(1.0), rgb, c.y);
}

vec4 HalfRainbow(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;

    vec3 color = vec3(0.);
    
    float freq = floor(mod(low_u_time, 6.0));
    if(freq == 0.0){
        for(int i = 0; i < 10; i++){

            float ifFill = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + fract(u_time), vec2(0.5, -0.2));
            if(ifFill != 0.0){
                color = BG_hsb2rgb(vec3(float(i) * 0.1+ fract(u_time / 5.0), 1.0, ifFill * 0.6));
            }
        }
    }
    else if(freq == 1.0){
        for(int i = 0; i < 10; i++){

            float ifFill = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + fract(u_time), vec2(0.5, 1.2));
            if(ifFill != 0.0){
                color = BG_hsb2rgb(vec3(float(i) * 0.1+ fract(u_time / 5.0), 1.0, ifFill * 0.6));
            }
        }
    }
    else if(freq == 2.0){
        for(int i = 0; i < 10; i++){

            float ifFill = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + fract(u_time), vec2(-0.2, 0.5));
            if(ifFill != 0.0){
                color = BG_hsb2rgb(vec3(float(i) * 0.1+ fract(u_time / 5.0), 1.0, ifFill * 0.6));
            }
        }
    }
    else if(freq == 3.0){
        for(int i = 0; i < 10; i++){

            float ifFill = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + fract(u_time), vec2(1.2, 0.5));
            if(ifFill != 0.0){
                color = BG_hsb2rgb(vec3(float(i) * 0.1+ fract(u_time / 5.0), 1.0, ifFill * 0.6));
            }
        }
    }
    else if(freq == 4.0){
        for(int i = 0; i < 10; i++){

            float ifFill = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + fract(u_time), vec2(1.2, 0.5));
            if(ifFill != 0.0){
                color = BG_hsb2rgb(vec3(float(i) * 0.1+ fract(u_time / 5.0), 1.0, ifFill * 0.6));
            }
        }
        for(int i = 0; i < 10; i++){

            float ifFill = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + fract(u_time), vec2(-0.2, 0.5));
            if(ifFill != 0.0){
                color = BG_hsb2rgb(vec3(float(i) * 0.1+ fract(u_time / 5.0), 1.0, ifFill * 0.6));
            }
        }
    }
    else if(freq == 5.0){
        for(int i = 0; i < 10; i++){

            float ifFill = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + fract(u_time), vec2(0.5, 1.2));
            if(ifFill != 0.0){
                color = BG_hsb2rgb(vec3(float(i) * 0.1+ fract(u_time / 5.0), 1.0, ifFill * 0.6));
            }
        }
        for(int i = 0; i < 10; i++){

            float ifFill = BG_HalfCircle(st, 1.0 - float(i) * 0.1 + fract(u_time), vec2(0.5, -0.2));
            if(ifFill != 0.0){
                color = BG_hsb2rgb(vec3(float(i) * 0.1+ fract(u_time / 5.0), 1.0, ifFill * 0.6));
            }
        }
    }

    return vec4(color,1.0);
}