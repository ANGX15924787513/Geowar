shader_type canvas_item;

uniform float blur_amount : hint_range(0.0, 10.0) = 2.0;

void fragment() {
    vec4 sum = vec4(0.0);
    float count = 0.0;
    vec2 tex_size = 1.0 / TEXTURE_PIXEL_SIZE;

    for (float x = -blur_amount; x <= blur_amount; x += 1.0) {
        for (float y = -blur_amount; y <= blur_amount; y += 1.0) {
            vec2 offset = vec2(x, y) / tex_size;
            sum += texture(TEXTURE, UV + offset);
            count += 1.0;
        }
    }
    COLOR = sum / count;
}