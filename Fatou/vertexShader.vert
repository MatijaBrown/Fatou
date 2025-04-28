#version 400

precision highp float;

layout (location = 0) in vec3 vertex;

uniform mat4 viewMatrix;

out vec2 position;

void main(void) {
    gl_Position = viewMatrix * vec4(vertex, 1.0);
    position = vertex.xy;
}