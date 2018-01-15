﻿#version 120

varying vec4 vertexColour;
varying float vertexSelected;
varying float vertexhasWireframe;
varying vec4 vertexHighlightColor;

uniform bool drawSelectedOnly;
uniform bool drawUnselectedOnly;
uniform vec4 selectedColour;
uniform vec4 overrideColour;

void main()
{
    if ((drawSelectedOnly && vertexSelected <= 0.9) && vertexhasWireframe <= 0.9) discard;
    if ((drawUnselectedOnly && vertexSelected > 0.9) && vertexhasWireframe <= 0.9) discard;

	gl_FragColor = mix(vertexColour, selectedColour, vertexSelected);
	if (overrideColour.w > 0) gl_FragColor = overrideColour;
	if (vertexhasWireframe > 0.9 && vertexSelected <= 0.9) gl_FragColor = vertexHighlightColor;
}
