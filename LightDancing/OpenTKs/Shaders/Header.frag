//BackgroundOnly
#version 330 
#ifdef GL_FRAGMENT_PRECISION_HIGH
    precision highp float;
#else
    precision mediump float;
#endif
#define PI 3.1415926
uniform vec2 u_resolution;
uniform float u_time;
uniform float low_u_time;

uniform float low_intensity;
uniform vec3 low_displayColor;



//PanoramaOnly
#version 330
#ifdef GL_FRAGMENT_PRECISION_HIGH
    precision highp float;
#else
    precision mediump float;
#endif
#define PI 3.14159
uniform vec2 u_resolution;
uniform float u_time;

uniform float mid_u_time;
uniform float mid_intensity;
uniform vec3 mid_displayColor;

//ProtagonistOnly
#version 330
#ifdef GL_FRAGMENT_PRECISION_HIGH
    precision highp float;
#else
    precision mediump float;
#endif
#define PI 3.14159
uniform vec2 u_resolution;
uniform float u_time;
uniform float high_u_time;
uniform float high_intensity;
uniform vec3 high_displayColor;

//BackgroundAndPanorama
#version 330
#ifdef GL_FRAGMENT_PRECISION_HIGH
    precision highp float;
#else
    precision mediump float;
#endif
#define PI 3.14159
uniform vec2 u_resolution;
uniform float u_time;
uniform float low_u_time;
uniform float low_intensity;
uniform vec3 low_displayColor;

uniform float mid_u_time;
uniform float mid_intensity;
uniform vec3 mid_displayColor;

//BackgroundAndProtagonist
#version 330
#ifdef GL_FRAGMENT_PRECISION_HIGH
    precision highp float;
#else
    precision mediump float;
#endif
#define PI 3.14159
uniform vec2 u_resolution;
uniform float u_time;
uniform float low_u_time;
uniform float low_intensity;
uniform vec3 low_displayColor;

uniform float high_u_time;
uniform float high_intensity;
uniform vec3 high_displayColor;

//PanoramaAndProtagonist
#version 330
#ifdef GL_FRAGMENT_PRECISION_HIGH
    precision highp float;
#else
    precision mediump float;
#endif
#define PI 3.14159
uniform vec2 u_resolution;
uniform float u_time;
uniform float mid_u_time;
uniform float mid_intensity;
uniform vec3 mid_displayColor;
uniform float high_u_time;
uniform float high_intensity;
uniform vec3 high_displayColor;

//All
#version 330
#ifdef GL_FRAGMENT_PRECISION_HIGH
    precision highp float;
#else
    precision mediump float;
#endif
#define PI 3.14159
uniform vec2 u_resolution;
uniform float u_time;
uniform float low_u_time;
uniform float low_intensity;
uniform vec3 low_displayColor;
uniform float mid_u_time;
uniform float mid_intensity;
uniform vec3 mid_displayColor;
uniform float high_u_time;
uniform float high_intensity;
uniform vec3 high_displayColor;

//FourBands
#version 330
#ifdef GL_FRAGMENT_PRECISION_HIGH
    precision highp float;
#else
    precision mediump float;
#endif
#define PI 3.14159
uniform vec2 u_resolution;
uniform float u_time;
uniform float low_u_time;
uniform float low_intensity;
uniform vec3 low_displayColor;
uniform float mid_u_time;
uniform float mid_intensity;
uniform vec3 mid_displayColor;
uniform float high_u_time;
uniform float high_intensity;
uniform vec3 high_displayColor;
uniform float kick_u_time;
uniform float kick_intensity;
uniform vec3 kick_displayColor;


//MusicAnimate
#version 330
#ifdef GL_FRAGMENT_PRECISION_HIGH
    precision highp float;
#else
    precision mediump float;
#endif
#define PI 3.14159
uniform vec2 u_resolution;
uniform float u_time;
uniform float beat_u_time;
uniform float low_u_time;
uniform float low_intensity;
uniform vec3 low_displayColor;
uniform float mid_u_time;
uniform float mid_intensity;
uniform vec3 mid_displayColor;
uniform float high_u_time;
uniform float high_intensity;
uniform vec3 high_displayColor;
uniform float kick_u_time;
uniform float kick_intensity;
uniform vec3 kick_displayColor;
uniform float u_time_beat;
uniform float volume;
uniform vec3 color_1;
uniform vec3 color_2;
uniform vec3 color_3;
uniform vec3 color_4;
uniform vec3 color_5;
uniform vec3 color_6;
uniform vec3 color_7;
uniform vec3 color_8;

//WallpaperAnimate
#version 330
#ifdef GL_FRAGMENT_PRECISION_HIGH
    precision highp float;
#else
    precision mediump float;
#endif
#define PI 3.14159
uniform vec2 u_resolution;
uniform float u_time;
uniform vec3 color_1;
uniform vec3 color_2;
uniform vec3 color_3;
uniform vec3 color_4;
uniform vec3 color_5;
uniform vec3 color_6;
uniform vec3 color_7;
uniform vec3 color_8;