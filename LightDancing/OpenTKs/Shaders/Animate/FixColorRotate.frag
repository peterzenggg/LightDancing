mat2 rotate2d_fix(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

vec3 GetColor_fix(float beats){
    beats = mod(beats, 1.0);
    vec3 color;
    if(beats >= 0.0 && beats <= 0.125){
        color = mix(color_1, color_2, (beats - 0.0) / 0.125);
    }
    else if(beats >= 0.125 && beats < 0.25){
        color = mix(color_2, color_3, (beats - 0.125) / 0.125);
    }
    else if(beats >= 0.25 && beats < 0.375){
        color = mix(color_3, color_4, (beats - 0.25) / 0.125);
    }
    else if(beats >= 0.375 && beats < 0.5){
        color = mix(color_4, color_5, (beats - 0.375) / 0.125);
    }
    else if(beats >= 0.5 && beats < 0.625){
        color = mix(color_5, color_6, (beats - 0.5) / 0.125);
    }
    else if(beats >= 0.625 && beats < 0.750){
        color = mix(color_6, color_7, (beats - 0.625) / 0.125);
    }
    else if(beats >= 0.750 && beats < 0.875){
        color = mix(color_7, color_8, (beats - 0.750) / 0.125);
    }
    else if(beats >= 0.875 && beats <= 1.0){
        color = mix(color_8, color_1, (beats - 0.875) / 0.125);
    }
    
    return color;
}

vec4 FixColorRotate(){
    vec2 st = gl_FragCoord.xy/u_resolution;
    vec3 color = vec3(0.0);

    st = st - 0.5;
    st = st * rotate2d_fix(- u_time);
    st += 0.5;

    // Use polar coordinates instead of cartesian
    vec2 toCenter = vec2(0.5)-st;
    float angle = atan(toCenter.y,toCenter.x) / 3.0;
    float radius = length(toCenter)*2.0;

    // Map the angle (-PI to PI) to the Hue (from 0 to 1)
    // and the Saturation to the radius
    color = GetColor_fix((angle/ PI));

    return vec4(color,1.0);
}