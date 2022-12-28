﻿precision mediump float;

uniform float uAlpha;
uniform sampler2D uSampler;
uniform bool uColorCoding;
uniform float uHighlighting;

varying vec2 vTexCoord;
varying vec4 vIdColor;


void main(void) {
	if (uColorCoding)
	{
		gl_FragColor = vIdColor;
	}
	else
	{
		vec4 pixel = texture2D(uSampler, vTexCoord);
		if (vIdColor.x < 0.0)
		{
			//is selected
			gl_FragColor = vec4(pixel.rgb * uHighlighting, uAlpha);
		}
		else
		{
			//is not selected
			gl_FragColor = vec4(pixel.rgb, uAlpha);
		}
	}
}