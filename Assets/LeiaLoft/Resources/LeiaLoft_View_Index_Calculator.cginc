/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/

#ifndef LEIALOFT_VIEW_INDEX_CALCULATIONS
#define LEIALOFT_VIEW_INDEX_CALCULATIONS

// assume that these params are required by the including shader:
/*

uniform float _offsetX;

uniform float _width;
uniform float _height;

uniform float _viewsX;
uniform float _viewsY;
*/


uniform float _peelOffset;

uniform float dynamic_interlace_scale; // might need to be xy in future, but keep as scalar for now for consistency with OpenGL
uniform float dynamic_interlace_cos;
uniform float dynamic_interlace_sin;
float4x4 _interlace_matrix;


// from https://github.com/leaiss/orbital_player/blob/cca1adc694b8bbbe8d25728e749bf305a4b67bc1/orbitalplayer/shaders/interlace_shader.glsl#L76
uniform int perPixelCorrection;
uniform float n;
uniform float d_over_n;
uniform float p_over_du;
uniform float p_over_dv;
uniform int colorInversion;
uniform float colorSlant;
uniform float faceX;
uniform float faceY;
uniform float faceZ;
uniform float pixelPitch;
uniform float du;
uniform float dv;
uniform float s;
uniform float cos_theta;
uniform float sin_theta;
uniform float No;
uniform float NumViews;

uniform float showR0Test;

inline float periodic_mod(float a, float b) {
	return a - b * floor(a / b);
}
inline float2 periodic_mod(float2 a, float2 b) {
	return a - b * floor(a / b);
}
inline float3 periodic_mod(float3 a, float3 b) {
	return a - b * floor(a / b);
}
inline float4 periodic_mod(float4 a, float4 b) {
	return a - b * floor(a / b);
}

float3 calculateViewVector(float2 uv)
{	
	float3 views;
    views.r = (p_over_du * _width * uv.x) + (p_over_dv * _height * uv.y);
    views.g = (p_over_du * _width * uv.x) + (p_over_dv * _height * uv.y);
    views.b = (p_over_du * _width * uv.x) + (p_over_dv * _height * uv.y);

	views.r += 2.0 * colorInversion;
	views.g += 1.0;
	views.b += 2.0 * (1.0 - colorInversion);

	return views;
}

// from https://github.com/leaiss/orbital_player/blob/cca1adc694b8bbbe8d25728e749bf305a4b67bc1/orbitalplayer/shaders/interlace_shader.glsl#L114
float N(float x, float y, float z, float x0, float y0)
{
	float dx = s * x0 + (cos_theta - 1.0) * x0 - sin_theta * y0;
	float dy = s * y0 + (cos_theta - 1.0) * y0 + sin_theta * x0;

	float denom = sqrt(z * z + (1 - 1.0 / (n * n)) * ((x - x0) * (x - x0) + (y - y0) * (y - y0)));

	float u = dx + d_over_n * (x - x0) / denom;
	float v = dy + d_over_n * (y - y0) / denom;
	float result = u / du + v / dv;

	return No + result;
}

// gets view indices for R/G/B channels
float3 calculateViewIndices(float2 normalized_display_coord) {
/*
	if (showR0Test > 0.5)
	{
		perPixelCorrection = 0;
	}
*/
	float3 viewMatrix = calculateViewVector(normalized_display_coord);

	float viewCount = NumViews;
	// correct some float precision issues with this offset
	float correction = -1.0 / max(2.0, viewCount);

	// from https://github.com/leaiss/orbital_player/blob/cca1adc694b8bbbe8d25728e749bf305a4b67bc1/orbitalplayer/shaders/interlace_shader.glsl#L289
	float float_precision_offset = correction;

	//Ensures R0 at top left, checked in opengl and directx	
    float user_offset = -p_over_dv * (_height - 1);

	if (perPixelCorrection == 1) { //Peeling
		float x0 = (normalized_display_coord.x - 0.5) * _width * pixelPitch;
		float y0 = (normalized_display_coord.y - 0.5) * _height * pixelPitch;
		float dN = N(faceX, faceY, faceZ, 0.0, 0.0) - N(faceX, faceY, faceZ, x0, y0);
		user_offset += (viewCount - 1.0) * 0.5 - No;
		user_offset -= _peelOffset;
		user_offset += dN;
	}
	else if (perPixelCorrection == 2) //Stereo Sliding
	{
		float x0 = (normalized_display_coord.x - 0.5) * _width * pixelPitch;
		float y0 = (normalized_display_coord.y - 0.5) * _height * pixelPitch;
		user_offset += (viewCount - fmod(viewCount + 1.0, 2.0)) * 0.5 - N(faceX, faceY, faceZ, x0, y0);
	}

	// last row / row "3" of viewMatrix is ith view index, 
	float3 views = periodic_mod(viewMatrix + user_offset + float_precision_offset, viewCount);
	return views;
}

// process for converting view indices = int3 [view_index_r, view_index_g, view_index_b] into a float in the span from 0 - 255
float4 pixelFromInts(int3 viewIndices) {
	const float maxScale = 255.0;
	const float floatOffset = 0.1 / maxScale;

	float4 px = float4(
		(viewIndices.rgb + floatOffset) / maxScale,
		1.0);
	return px;
}

// process for calculating view indices = int3 [view_index_r, view_index_g, view_index_b] from onscreen UV coords and converting into a float in the span from 0 - 255
float4 pixelFromInts(float2 normalized_display_coord) {
	int3 viewIndices = calculateViewIndices(normalized_display_coord);
	return pixelFromInts(viewIndices);
}

// process for sampling view indices [r,g,b] from a texture which was previously written into
// this texture is currently not used, so don't create more properties for Unity to track
// sampler2D _interlaced_view_indices;
int3 sampleViewIndicesFromTexture(float2 normalized_display_coord) {
	const float maxScale = 255.0;
	const float floatOffset = 0.1 / maxScale;

	// float4 px = tex2D(_interlaced_view_indices, normalized_display_coord);
	float4 px = float4(0, 0, 0, 0);
	// each of the view indices is 0.1/255 units higher than their actual integral desired value
	// taking int3 rounds down
	int3 inds = int3(px.rgb * maxScale);
	return inds;
}

// this cginc is specifically for calculating view index, not for sampling ith texture.
// fixed4 interlaced_sample(float2 uv) is a texture sampling operation, so it goes in another cginc file

#endif // LEIALOFT_VIEW_INDEX_CALCULATIONS
