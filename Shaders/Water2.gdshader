shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

uniform float time_speed = 1.0;
uniform float wave_strength = 0.02;
uniform float wave_frequency = 20.0;

uniform float bubble_speed = 0.3;
uniform float bubble_density = 15.0;
uniform float bubble_size = 0.01;

uniform vec4 water_color : source_color = vec4(0.0, 0.5, 0.8, 0.5); // azul transparente

float rand(vec2 co) {
	return fract(sin(dot(co.xy, vec2(12.9898, 78.233))) * 43758.5453);
}

float draw_bubble(vec2 uv, vec2 center, float radius) {
	float dist = length(uv - center);
	return smoothstep(radius, radius * 0.8, dist);
}

void fragment() {
	float t = TIME * time_speed;

	// ========== ONDAS ==========
	float wave_x = sin(UV.y * wave_frequency + t) * wave_strength;
	float wave_y = cos(UV.x * wave_frequency * 0.6 + t * 1.2) * wave_strength * 0.6;
	vec2 distortion = vec2(wave_x, wave_y);

	// ========== REFRAÇÃO ==========
	vec4 screen_color = texture(SCREEN_TEXTURE, SCREEN_UV + distortion);

	// ========== BOLHAS ==========
	float bubble_layer = 0.0;
	for (int i = 0; i < 10; i++) {
		vec2 seed = vec2(float(i), float(i) * 3.13);
		vec2 bubble_pos = vec2(
			fract(rand(seed) + t * bubble_speed + float(i) * 0.1),
			fract(rand(seed + 1.0) + t * bubble_speed * 0.7 + float(i) * 0.13)
		);
		bubble_pos.y = mod(bubble_pos.y + t * bubble_speed * (0.3 + rand(seed)), 1.0);
		float size = bubble_size * (0.5 + rand(seed + 2.0));
		bubble_layer += draw_bubble(UV, bubble_pos, size);
	}

	bubble_layer = clamp(bubble_layer, 0.0, 1.0);

	// ========== COR FINAL ==========
	vec4 base = mix(screen_color, water_color, 0.5);
	base.rgb += bubble_layer * vec3(1.0); // bolhas brancas

	COLOR = base;
}
 