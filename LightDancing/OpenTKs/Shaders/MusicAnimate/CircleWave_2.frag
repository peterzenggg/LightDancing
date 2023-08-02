float exponentialEasing (float x, float a){
  
  float epsilon = 0.00001;
  float min_param_a = 0.0 + epsilon;
  float max_param_a = 1.0 - epsilon;
  a = max(min_param_a, min(max_param_a, a));
  
  if (a < 0.5){
    // emphasis
    a = 2.0*(a);
    float y = pow(x, a);
    return y;
  } else {
    // de-emphasis
    a = 2.0*(a-0.5);
    float y = pow(x, 1.0/(1-a));
    return y;
  }
}

mat2 rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec4 mainImage(){
	vec3 c;
	float l,z=beat_u_time;
	float index = smoothstep(0., 1., low_intensity);
	index = exponentialEasing(low_intensity, 0.2);
	float midindex = smoothstep(0., 1., mid_intensity);
	midindex = exponentialEasing(mid_intensity, 0.2);;
	for(int i=0;i<3;i++) {
		vec2 uv,p=gl_FragCoord.xy/u_resolution;
		uv=p;
		p-=.5;
		//p.x*=r.x/r.y;
		z+=2.0;
		l=length(p);
		//uv+=p/l*(sin(z)+1.)*abs(sin(l*1.));
        //uv += mix(p/l*(sin(z)+1.)*abs(sin(l*7.*low_intensity)), p/l*(sin(z)+1.)*abs(sin(l*7.*low_intensity-z-z)), kick_intensity);
		//uv += p/l*(tan(z)+1.)*abs(cos(l*4.-z-z)*tan(u_time/100.));
		uv += p/l*(sin(z+1.))*abs(sin(3. + l*3.-z-z));
		
		//uv += vec2(low_u_time+beat_u_time);
		//uv.x += (mid_u_time+beat_u_time);
		//uv.y -= (mid_u_time+beat_u_time);
		if(uv.x < 0.5){
			uv.x -= beat_u_time / 10.0;
		}
		else{
			uv.x += beat_u_time / 10.0;
		}
		if(uv.y < 0.5){
			uv.y -= beat_u_time / 10.0;
		}
		else{
			uv.y += beat_u_time / 10.0;
		}
		// uv += beat_u_time;
        uv = fract(uv);
		if(i == 0)
			c[i]=(0.02 + 0.02*(index))/length(mod(uv,1.)-.5);
			//c[i]=(0.02 + 0.01)/length(mod(uv,1.)-.5);
		else if(i == 1){
			uv -= 0.5;
			uv *= rotate2d(-z/2. + u_time);
			uv -= 0.5;
			c[i]=(0.03 + 0.02*(midindex))/length(mod(uv,1.)-.5);
			//c[i]=(0.03 + 0.01)/length(mod(uv,1.)-.5);
		}
		else{
			uv -= 0.5;
			uv *= rotate2d(-z/3. + u_time);
			uv -= 0.5;
			c[i]=(0.04 + 0.02*(high_intensity))/length(mod(uv,1.)-.5) ;
			//c[i]=(0.04 + 0.01)/length(mod(uv,1.)-.5);
			}
	}
	return vec4(c/l,1.);
}

vec4 CircleWave_2() {
	vec4 color = mainImage();
	color = floor(color);
    return color;
}