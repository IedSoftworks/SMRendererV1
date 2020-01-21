﻿#version 330
in vec3 aPosition;
in vec3 aNormal;
in vec2 aTexture;

out vec2 vTexture;
out vec4 vPosition;

uniform mat4 uMVP;
uniform mat4 uTexSize;

//functions

void main() {
	vTexture = aTexture;
	vPosition = vec4(aPosition, 1.0);
	gl_Position = uMVP * vPosition;
}