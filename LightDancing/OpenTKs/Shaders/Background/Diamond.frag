vec2 skew (vec2 st) {
    vec2 r = vec2(0.0);
    r.x = 1.1547*st.x;
    r.y = st.y+0.5*r.x;
    return r;
}

vec3 simplexGrid (vec2 st) {
    vec3 xyz = vec3(0.0);

    vec2 p = fract(skew(st));
    if (p.x > p.y) {
        xyz.xy = 1.0-vec2(p.x,p.y-p.x)+low_intensity;
        xyz.z = p.y-fract(u_time/100.0);
    } else {
        xyz.yz = 1.0-vec2(p.x-p.y,p.y)+low_intensity;
        xyz.x = p.x-fract(u_time/100.0);
    }

    return fract(xyz);
}

vec4 Diamond(){
    vec2 st = gl_FragCoord.xy/u_resolution.xy;
    vec3 color = vec3(0.0);

    // Scale the space to see the grid
    float scale = floor(u_time);
    scale = 3.;
    st *= scale;

    // Show the 2D grid
    color.rg = fract(st);

    // Skew the 2D grid
    // color.rg = fract(skew(st));

    // Subdivide the grid into to equilateral triangles
     color = simplexGrid(st);

    return vec4(color,1.0);
}