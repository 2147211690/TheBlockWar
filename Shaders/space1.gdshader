shader_type canvas_item;

// Star Nest by Pablo Roman Andrioli
// License: MIT
// Converted to Godot Shader

uniform float speed : hint_range(0.0, 0.05) = 0.01;
uniform float brightness : hint_range(0.0, 0.01) = 0.0015;
uniform float darkmatter : hint_range(0.0, 1.0) = 0.3;
uniform float distfading : hint_range(0.0, 1.0) = 0.73;
uniform float saturation : hint_range(0.0, 1.0) = 0.85;
uniform vec2 mouse = vec2(0.5, 0.5); // 用鼠标控制视角，默认居中

const float zoom = 0.8;
const float tile = 0.85;
const int iterations = 17;
const float formuparam = 0.53;
const int volsteps = 20;
const float stepsize = 0.1;

void fragment() {
    // 获取像素坐标与屏幕分辨率（适用于全屏 ColorRect）
    vec2 fragCoord = FRAGCOORD.xy;
    vec2 iResolution = vec2(1.0) / SCREEN_PIXEL_SIZE;
    float iTime = TIME;

    // 坐标转换到中心原点，保持纵横比
    vec2 uv = fragCoord.xy / iResolution.xy - 0.5;
    uv.y *= iResolution.y / iResolution.x;
    vec3 dir = vec3(uv * zoom, 1.0);
    float time = iTime * speed + 0.25;

    // 鼠标/触摸控制旋转（若无鼠标则使用默认视角）
    float a1 = 0.5 + mouse.x * 2.0;
    float a2 = 0.8 + mouse.y * 2.0;
    mat2 rot1 = mat2(vec2(cos(a1), sin(a1)), vec2(-sin(a1), cos(a1)));
    mat2 rot2 = mat2(vec2(cos(a2), sin(a2)), vec2(-sin(a2), cos(a2)));
    dir.xz *= rot1;
    dir.xy *= rot2;
    vec3 from = vec3(1.0, 0.5, 0.5);
    from += vec3(time * 2.0, time, -2.0);
    from.xz *= rot1;
    from.xy *= rot2;

    // 体积渲染
    float s = 0.1;
    float fade = 1.0;
    vec3 v = vec3(0.0);
    for (int r = 0; r < volsteps; r++) {
        vec3 p = from + s * dir * 0.5;
        p = abs(vec3(tile) - mod(p, vec3(tile * 2.0))); // 平铺折叠
        float pa = 0.0;
        float a = 0.0;
        for (int i = 0; i < iterations; i++) { 
            p = abs(p) / dot(p, p) - formuparam; // 核心公式
            a += abs(length(p) - pa);            // 变化量的绝对和
            pa = length(p);
        }
        float dm = max(0.0, darkmatter - a * a * 0.001); // 暗物质
        a *= a * a; // 增强对比度
        if (r > 6) fade *= 1.0 - dm; // 近处不渲染暗物质
        v += fade;
        v += vec3(s, s * s, s * s * s * s) * a * brightness * fade;
        fade *= distfading;
        s += stepsize;
    }
    v = mix(vec3(length(v)), v, saturation); // 饱和度调整
    COLOR = vec4(v * 0.01, 1.0);
}