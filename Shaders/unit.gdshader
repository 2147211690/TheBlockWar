shader_type canvas_item;

// === 水参数 ===
uniform float water_height : hint_range(0.0, 1000.0) = 50.0;
uniform sampler2D water_mask : filter_nearest, repeat_disable;
uniform vec4 water_color : source_color = vec4(0.0, 0.5, 1.0, 1.0);
uniform float water_a_ignore : hint_range(0.0, 1.0) = 0.5;

// === 描边参数 ===
uniform bool enable_outline = false;
uniform vec4 outline_color : source_color = vec4(1.0, 0.0, 0.0, 1.0);
uniform float outline_width : hint_range(0.0, 10.0) = 1.0;
uniform float outline_alpha_threshold : hint_range(0.0, 1.0) = 0.5;
uniform float outline_ignore_alpha : hint_range(0.0, 1.0) = 0.5;

varying vec4 vertex_color;

void vertex() {
    // 保存顶点颜色
    vertex_color = COLOR;
}


void fragment() {
	vec4 final_color = COLOR;
	
	vec2 uv = UV;
	vec4 rect = REGION_RECT;
	vec2 pixel = TEXTURE_PIXEL_SIZE;
	
	// === 1. 贴图向下偏移：UV 向上偏移 ===
	vec2 offset_uv = uv;
	offset_uv.y -= water_height * pixel.y;
	
	// 采样偏移后的主纹理（完整保留，不做 alpha 剔除）
	vec4 tex_color = vec4(0.0);
	if (offset_uv.x >= 0.0 && offset_uv.y >= 0.0 && offset_uv.x <= 1.0 && offset_uv.y <= 1.0) {
		tex_color = texture(TEXTURE, offset_uv);
	}
	
	// === 2. 先描边（基于偏移后的采样位置）===
	// 描边判断时忽略低 alpha 像素，但 tex_color 本身完整保留
	vec4 outlined_color = tex_color;
	
	if (enable_outline && tex_color.a < outline_alpha_threshold) {
		bool has_outline = false;
		
		for (float d = 1.0; d <= outline_width; d += 1.0) {
			vec2[8] offsets = vec2[](
				vec2(d, 0.0), vec2(-d, 0.0), vec2(0.0, d), vec2(0.0, -d),
				vec2(d, d), vec2(-d, d), vec2(d, -d), vec2(-d, -d)
			);
			
			for (int i = 0; i < 8; i++) {
				vec2 check_uv = offset_uv + offsets[i] * pixel;
				
				if (check_uv.x >= 0.0 && check_uv.y >= 0.0 && check_uv.x <= 1.0 && check_uv.y <= 1.0) {
					vec4 check_color = texture(TEXTURE, check_uv);
					// 描边判断时：低于 outline_ignore_alpha 的像素视为透明，不参与描边
					if (check_color.a > outline_ignore_alpha) {
						has_outline = true;
						break;
					}
				}
			}
			if (has_outline) break;
		}
		
		if (has_outline) {
			outlined_color = outline_color;
		}
	}
	
	// 无水时：直接输出描边结果（低 alpha 颜色保留，不参与描边但不剔除）
	final_color = outlined_color;
	
	// === 3. 后水逻辑（基于描边后的颜色）===
	if (water_height > 0.0) {
		vec4 mask_color = vec4(0.0);
		vec2 mask_size = vec2(textureSize(water_mask, 0));
		vec2 mask_uv = (uv - rect.xy) / pixel / mask_size;
		
		if (mask_uv.x >= 0.0 && mask_uv.y >= 0.0 && mask_uv.x <= 1.0 && mask_uv.y <= 1.0) {
			mask_color = texture(water_mask, mask_uv);
		}
		
		vec4 white = vec4(1.0, 1.0, 1.0, 1.0);
		vec4 blue  = vec4(0.0, 0.0, 1.0, 1.0);
		vec4 black = vec4(0.0, 0.0, 0.0, 1.0);
		
		// 水逻辑中：使用 water_a_ignore 判断贴图是否"非透明"
		vec4 effective_tex = (tex_color.a > water_a_ignore) ? tex_color : vec4(0.0);
		
		if (mask_color.a < 0.01) {
			// mask 透明：保持原色（含描边，低 alpha 保留）
			final_color = outlined_color;
		} else if (mask_color == white) {
			// 白色：透明
			final_color = vec4(0.0);
		} else if (mask_color == blue) {
			// 蓝色：有效贴图非透明 ? Water色 : 透明
			final_color = (effective_tex.a > 0.01) ? water_color : vec4(0.0);
		} else if (mask_color == black) {
			// 黑色：Water色
			final_color = water_color;
		} else {
			final_color = vec4(0.0);
		}
	}
	
	COLOR = final_color * vertex_color;
}