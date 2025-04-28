#version 400

precision highp float;

in vec2 position;

layout (location = 0) out vec4 out_Colour;

uniform vec4 renderBounds; // RTLB

const int MAX_ITERATIONS = 1000;

vec2 squareImaginary(vec2 im) {
    return vec2(
        pow(im.x, 2) - pow(im.y, 2),
        2 * im.x * im.y
    );
}

vec3 colourForIterations(int iteration) {
    float i = log(float(iteration)) / log(float(MAX_ITERATIONS)) * 255.0;
    float r = sin(0.024 * i + 0) * 0.5 + 0.5;
    float g = sin(0.024 * i + 2) * 0.1 + 0.5;
    float b = sin(0.024 * i + 4) * 0.5 + 0.5;

    return vec3(r, g, b);
}

vec3 iterateMandelbrot(vec2 coord) {
    vec2 z = vec2(0, 0);
    for (int i = 0; i < MAX_ITERATIONS; i++) {
        z = squareImaginary(z) + coord;
        if (length(z) >= 2.0) {
            return colourForIterations(i);
        }
    }
    return vec3(0.0);
}

void main(void) {
    // position goes from (-1, -1) to (1, 1) (inclusive).
    vec2 coordinate = vec2(
        renderBounds.x + (position.x + 1.0) / 2.0 * (renderBounds.z - renderBounds.x),
        renderBounds.y + (position.y + 1.0) / 2.0 * (renderBounds.w - renderBounds.y)
    );

    vec3 colour = iterateMandelbrot(coordinate);

    out_Colour = vec4(colour, 1);
}