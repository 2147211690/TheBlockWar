shader_type canvas_item;

// 透明度阈值
uniform float alpha_threshold : hint_range(0.0, 1.0) = 0.5;

// 描边参数
uniform vec4 outline1_color : source_color = vec4(1.0, 0.0, 0.0, 1.0);
uniform float outline1_width : hint_range(0.0, 10.0) = 0;
uniform vec4 outline2_color : source_color = vec4(0.0, 1.0, 0.0, 1.0);
uniform float outline2_width : hint_range(0.0, 10.0) = 0.0;

// 水参数
uniform int water : hint_range(0, 200) = 0;
uniform vec4 water_color : source_color = vec4(0.25, 0.65, 1.0, 0.85);

// 安全采样
vec4 safe_region_sample(sampler2D tex, vec2 uv, vec2 region_min, vec2 region_max, float z, vec2 oruv) {
	if (uv.x < region_min.x || uv.x > region_max.x || 
        uv.y < region_min.y || uv.y > region_max.y) {
        return vec4(0.0);
    }
	float xv = (oruv.x - region_min.x) / z;
	xv = xv * 2.0 - 1.0;
	float h = sqrt(0.6 - xv * xv) + 0.8;
    if (oruv.y > h * xv && water > 0){
		if (oruv.y < h * xv + 0.05 && texture(tex, uv).a > 0.8)
		{
        	return water_color;
		}
		else
		{
			return vec4(0);
		}
    }
    return texture(tex, uv);
}

void fragment() {
    vec2 region_min = REGION_RECT.xy;
    vec2 region_max = REGION_RECT.xy + REGION_RECT.zw;
    
    // 单位向下偏移 water 像素
    float water_offset = float(water) * TEXTURE_PIXEL_SIZE.y;
    vec2 sample_uv = UV - vec2(0.0, water_offset);
    
    vec4 result = safe_region_sample(TEXTURE, sample_uv, region_min, region_max, REGION_RECT.z, UV);
    

        vec4 original_color = result;
        float alpha = original_color.a;
        
        bool show_outline1 = false;
        bool show_outline2 = false;
        
        if (alpha < alpha_threshold) {
            // 第一层描边
            for (float d = 1.0; d <= outline1_width; d += 1.0) {
                vec2[8] offsets = vec2[](
                    vec2(d,0), vec2(-d,0), vec2(0,d), vec2(0,-d),
                    vec2(d,d), vec2(-d,d), vec2(d,-d), vec2(-d,-d)
                );
                for (int i = 0; i < 8; i++) {
                    vec2 check_uv = sample_uv + offsets[i] * TEXTURE_PIXEL_SIZE;
                    vec4 sample_color = safe_region_sample(TEXTURE, check_uv, region_min, region_max, REGION_RECT.z, UV);
                    if (sample_color.a >= alpha_threshold) {
                        show_outline1 = true;
                        break;
                    }
                }
                if (show_outline1) break;
            }
            
            // 第二层描边
            if (!show_outline1) {
                for (float d = 1.0; d <= outline2_width; d += 1.0) {
                    vec2[8] offsets = vec2[](
                        vec2(d,0), vec2(-d,0), vec2(0,d), vec2(0,-d),
                        vec2(d,d), vec2(-d,d), vec2(d,-d), vec2(-d,-d)
                    );
                    for (int i = 0; i < 8; i++) {
                        vec2 check_uv = sample_uv + offsets[i] * TEXTURE_PIXEL_SIZE;
                        vec4 sample_color = safe_region_sample(TEXTURE, check_uv, region_min, region_max, REGION_RECT.z, UV);
                        if (sample_color.a >= alpha_threshold) {
                            show_outline2 = true;
                            break;
                        }
                    }
                    if (show_outline2) break;
                }
            }
        }
        
        result = original_color;
        if (alpha < alpha_threshold) {
            if (show_outline1) result = outline1_color;
            else if (show_outline2) result = outline2_color;
        }

    COLOR = result;
}