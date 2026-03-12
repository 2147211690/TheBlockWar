shader_type canvas_item;

// 透明度阈值
uniform float alpha_threshold : hint_range(0.0, 1.0) = 0.5;

// 描边参数
uniform vec4 outline1_color : source_color = vec4(1.0, 0.0, 0.0, 1.0);
uniform float outline1_width : hint_range(0.0, 10.0) = 0;
uniform vec4 outline2_color : source_color = vec4(0.0, 1.0, 0.0, 1.0);
uniform float outline2_width : hint_range(0.0, 10.0) = 0.0;

// 安全采样函数（现在可以访问全局的region_min和region_max）
vec4 safe_region_sample(sampler2D tex, vec2 uv, vec2 region_min, vec2 region_max) {
    // 检查UV是否在当前帧区域内
    if (uv.x < region_min.x || uv.x > region_max.x || 
        uv.y < region_min.y || uv.y > region_max.y) {
        return vec4(0.0); // 超出区域视为透明
    }
    return texture(tex, uv);
}
void fragment() {

    vec4 original_color = texture(TEXTURE, UV);
    float alpha = original_color.a;

    bool show_outline1 = false;
    bool show_outline2 = false;

    if (alpha < alpha_threshold) {
		vec2 region_min = REGION_RECT.xy;
    	vec2 region_max = REGION_RECT.xy + REGION_RECT.zw;
        // 检测第一层描边
        for (float d = 1.0; d <= outline1_width; d += 1.0) {
            vec2[8] offsets = vec2[](
                vec2(d,0), vec2(-d,0), vec2(0,d), vec2(0,-d),
                vec2(d,d), vec2(-d,d), vec2(d,-d), vec2(-d,-d)
            );
            for (int i = 0; i < 8; i++) {
                vec2 sample_uv = UV + offsets[i] * TEXTURE_PIXEL_SIZE;
                vec4 sample_color = safe_region_sample(TEXTURE, sample_uv, region_min, region_max);
                if (sample_color.a >= alpha_threshold) {
                    show_outline1 = true;
                    break;
                }
            }
            if (show_outline1) break;
        }

        // 检测第二层描边
        if (!show_outline1) {
            for (float d = 1.0; d <= outline2_width; d += 1.0) {
                vec2[8] offsets = vec2[](
                    vec2(d,0), vec2(-d,0), vec2(0,d), vec2(0,-d),
                    vec2(d,d), vec2(-d,d), vec2(d,-d), vec2(-d,-d)
                );
                for (int i = 0; i < 8; i++) {
                    vec2 sample_uv = UV + offsets[i] * TEXTURE_PIXEL_SIZE;
                    vec4 sample_color = safe_region_sample(TEXTURE, sample_uv, region_min, region_max);
                    if (sample_color.a >= alpha_threshold) {
                        show_outline2 = true;
                        break;
                    }
                }
                if (show_outline2) break;
            }
        }
    }

    vec4 result = original_color;
    if (alpha < alpha_threshold) {
        if (show_outline1) {
            result = outline1_color;
        } else if (show_outline2) {
            result = outline2_color;
        }
    }

    COLOR = result;
}