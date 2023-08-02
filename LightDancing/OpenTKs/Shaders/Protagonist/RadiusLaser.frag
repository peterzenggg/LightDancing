mat2 high_rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

float high_random (in float x) {
    return fract(sin(x)*1e4);
}

float high_Line(vec2 _st, vec2 size, vec2 position){
    float x = smoothstep(position.x - size.x / 2.0, position.x, _st.x) - smoothstep(position.x, position.x + size.x / 2.0, _st.x);
    float y = smoothstep(position.y - size.y / 2.0, position.y, _st.y) - smoothstep(position.y, position.y + size.y / 2.0, _st.y);
    
    return x*y;
}

vec3 hsb2rgb( in vec3 c ){
    vec3 rgb = clamp(abs(mod(c.x*6.0+vec3(0.0,4.0,2.0),
                             6.0)-3.0)-1.0,
                     0.0,
                     1.0 );
    rgb = rgb*rgb*(3.0-2.0*rgb);
    return c.z * mix( vec3(1.0), rgb, c.y);
}

float high_Circle(vec2 _st, vec2 position){
    vec2 pos = _st - position;
    float length = length(pos);
    
    return smoothstep(0.262 /(high_u_time + 1.0), 0.01,  length);
}

vec4 RadiusLaser(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
	vec2 still_st = st;
    vec3 color = vec3(0.);
    st -=0.5;
    st = high_rotate2d(u_time) * st;
    st += 0.5;
    float ifFill;
    float ifCircle;
    for(int i = 0; i < int(high_u_time); i++){
        vec2 _st = st - 0.5;
        float angle = 2.0*PI / high_u_time * float(i);
    	_st = high_rotate2d(angle) * _st;
    	_st += 0.5;
        
        vec2 position = vec2(high_random(float(i)), 0.5);
        float size = high_random(float(i) + floor(u_time) );
        position.x = mod(position.x + u_time, 0.5) + 0.7;
        ifFill += high_Line(_st, vec2(size, 0.038), position);
        ifCircle += high_Circle(_st, position);
        
    }
    
    vec2 pos = 0.5 - st;
    float a = atan(pos.y/pos.x);
    float radius = length(pos);
    if(ifFill != 0.0){
    	color = ifFill * hsb2rgb(vec3((a/(2.0*PI)), 1.0 ,1.0));
    }
    //if(ifCircle != 0.0){
     //   if(high_u_time != 0.0){
     //       color += ifCircle * vec3(0.5/high_u_time);
     //   }
     //   else{
     //       color += ifCircle * vec3(0.5);
     //   }
    //}

    return vec4(color,1.0);
}