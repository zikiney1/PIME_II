shader_type canvas_item;

uniform bool active;
uniform vec3 flash_color : source_color = vec3(1.0); // branco por padrão

void fragment() {
	vec4 tex_color = texture(TEXTURE, UV);
	if (active) {
		COLOR = vec4(flash_color, 1.0) * tex_color.a; // usa a cor de flash, mas respeita o canal alpha da textura
	} else {
		COLOR = tex_color;
	}
}
