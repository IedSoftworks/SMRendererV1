﻿#version 330
in vec2 vTexture;
in vec4 gl_FragCoord;
in vec4 vPosition;

uniform sampler2D uTexture;
uniform vec2 uWindowSize;
uniform vec2 uTexSize;
uniform vec2 uObjSize;
uniform vec4 uColor;

uniform int uBloomUsage;
uniform vec4 uBloom;

uniform int uBorderUsage;
uniform vec4 uBorderColor;
uniform int uBorderWidth;
uniform int uBorderLength;

out vec4 color;
out vec4 bloom;

void main(){
	vec2 pos = vPosition.xy * uObjSize + vec2(uObjSize.x / 2, uObjSize.y /2);

	vec4 texColor = texture(uTexture, vTexture);
	color = texColor * uColor;
	if (uBorderUsage > 0) {
		if (uBorderUsage == 1 && texColor.w == 0) {
			vec2 pxSize = 1.0 / vec2(textureSize(uTexture,0));
		
			vec4 check1 = texture(uTexture, vTexture + vec2(1,0) * pxSize);
			vec4 check2 = texture(uTexture, vTexture + vec2(-1,0) * pxSize);
			vec4 check3 = texture(uTexture, vTexture + vec2(0,1) * pxSize);
			vec4 check4 = texture(uTexture, vTexture + vec2(0,-1) * pxSize);

			if(check1.w > 0 && check1 != uBorderColor) color = uBorderColor;
			else if(check2.w > 0 && check2 != uBorderColor) color = uBorderColor;
			else if(check3.w > 0 && check3 != uBorderColor) color = uBorderColor;
			else if(check4.w > 0 && check4 != uBorderColor) color = uBorderColor;
		}
		if (uBorderUsage == 2) {
			if ((pos.x > 0 && pos.x < uBorderWidth) || (pos.x < uObjSize.x && pos.x > uObjSize.x - uBorderWidth) || (pos.y > 0 && pos.y < uBorderWidth) || (pos.y < uObjSize.y && pos.y > uObjSize.y - uBorderWidth) ) {
				color = uBorderColor;
			}
		}
		if (uBorderUsage == 3) {
			float len = uBorderLength;
			if (uBorderLength < 0) {
				float aspect = (uObjSize.x + uObjSize.y) / 2;
				len = (aspect * .1);
			}
			bool check1 = (pos.y > 0 && pos.y < len && pos.x > 0 && pos.x < uBorderWidth) || (pos.x > 0 && pos.x < len && pos.y > 0 && pos.y < uBorderWidth); // upper left
			bool check2 = (pos.y < uObjSize.y && pos.y > uObjSize.y - len && pos.x > 0 && pos.x < uBorderWidth) || (pos.x > 0 && pos.x < len && pos.y < uObjSize.y && pos.y > uObjSize.y - uBorderWidth);
			bool check3 = (pos.x < uObjSize.x && pos.x > uObjSize.x - len && pos.y > 0 && pos.y < uBorderWidth) || (pos.y > 0 && pos.y < len && pos.x < uObjSize.x && pos.x > uObjSize.x - uBorderWidth);
			bool check4 = (pos.x < uObjSize.x && pos.x > uObjSize.x - len && pos.y < uObjSize.y && pos.y > uObjSize.y - uBorderWidth) || (pos.y < uObjSize.y && pos.y > uObjSize.y-len && pos.x < uObjSize.x && pos.x > uObjSize.x - uBorderWidth);
			if (check1 || check2 || check3 || check4 ) {
				color = uBorderColor;
			}
		}
	}

	if (uBloomUsage == 0) {
		bloom = vec4(0,0,0,0);
	} else if (uBloomUsage == 1) {
		bloom = vec4(uBloom.xyz, color.w * uBloom.w);
	} else if (uBloomUsage == 2) { 
		if (texColor.w > 0) bloom = uColor;
	} else if (uBloomUsage == 3) {
		bloom = texColor;
	}
}