#version 120

layout(location = 0) in vec3 position;
layout(location = 1) in vec3 normal;
layout(location = 2) in vec2 texture;
layout(location = 3) in vec4 colour;
layout(location = 4) in float selected;
//layout(location = 5) in vec4 highlightcolor;
layout(location = 6) in float haswireframe;
layout(location = 7) in vec4 wireframecolor;
//layout(location = 8) in float ignoretexture;

varying vec4 vertexColour;
varying float vertexSelected;
varying float vertexhasWireframe;
varying vec4 vertexHighlightColor;

uniform mat4 modelViewMatrix;
uniform mat4 perspectiveMatrix;
uniform mat4 cameraMatrix;
uniform mat4 selectionTransform;

void main()
{
    vec4 pos = vec4(position, 1);
    if (selected > 0.9) pos = selectionTransform * pos;
    vec4 modelPos = modelViewMatrix * pos;
    
	vec4 cameraPos = cameraMatrix * modelPos;
	gl_Position = perspectiveMatrix * cameraPos;

	vertexColour = colour;
    vertexSelected = selected;
	vertexhasWireframe = haswireframe;
	vertexHighlightColor = wireframecolor;
}
