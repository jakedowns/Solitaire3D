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

// disable reports of use of potentially uninitialized variable
// these occur sometimes when we use preprocessor
#pragma warning (disable:4000)

float4 _texture_0_ST;
sampler2D _texture_0;
sampler2D _texture_1;
sampler2D _texture_2;
sampler2D _texture_3;
sampler2D _texture_4;
sampler2D _texture_5;
sampler2D _texture_6;
sampler2D _texture_7;
sampler2D _texture_8;
sampler2D _texture_9;
sampler2D _texture_10;
sampler2D _texture_11;
sampler2D _texture_overflow;

sampler2D _MainTex;
 
uniform float _texture_overflow_cols;
uniform float _texture_overflow_rows;

// switch point between _texture_11 and _texture_overflow
static const float overflowLimit = 12.0;

float _Brightness;

// samples from a subsection of a tiled "overflow" texture, as if the uv coord were in the tile of the "overflow" texture
inline float2 uv_in_overflow(float2 uv, int view)
{
	// tiled texture slots do not span from 0...11, they start at 12+. but math is for index i of a tiled image, so map 12+ to index 0,1,2...
    view = view - overflowLimit;

	// then read out of a texture with tiles. see LeiaMediaMaterial shader's remap
    float xoffset = _texture_overflow_cols - 1.0 - fmod(view, _texture_overflow_cols);
    float yoffset = floor(view / _texture_overflow_cols);

    return float2((uv + float2(xoffset, yoffset)) / float2(_texture_overflow_cols, _texture_overflow_rows));
}

uniform float rainbowViews;

uniform float minView;
uniform float maxView;

float4 sample_view(float2 uv, int view)
{
#if 0
    if (showR0Test > 0.5)
    {
        if (view == 0)
            return float4(1.0, 0.0, 0.0, 1.0);
        return float4(0.0, 0.0, 0.0, 1.0);
    }
    
    if (view < minView || view > maxView)
        return float4(0.0, 0.0, 0.0, 1.0);

    if (rainbowViews > 0.5)
    {

        if (_viewsX * _viewsY == 9)
        {
            if (view == 0)
                return tex2D(_MainTex, float2(0.000, 0.66) + float2(0.333, 0.333) * uv.xy);
            else if (view == 1)
                return tex2D(_MainTex, float2(0.333, 0.66) + float2(0.333, 0.333) * uv.xy);
            else if (view == 2)
                return tex2D(_MainTex, float2(0.666, 0.66) + float2(0.333, 0.333) * uv.xy);
            else if (view == 3)
                return tex2D(_MainTex, float2(0.000, 0.333) + float2(0.333, 0.333) * uv.xy);
            else if (view == 4)
                return tex2D(_MainTex, float2(0.333, 0.333) + float2(0.333, 0.333) * uv.xy);
            else if (view == 5)
                return tex2D(_MainTex, float2(0.666, 0.333) + float2(0.333, 0.333) * uv.xy);
            else if (view == 6)
                return tex2D(_MainTex, float2(0.000, 0.00) + float2(0.333, 0.333) * uv.xy);
            else if (view == 7)
                return tex2D(_MainTex, float2(0.333, 0.00) + float2(0.333, 0.333) * uv.xy);
            else if (view == 8)
                return tex2D(_MainTex, float2(0.666, 0.00) + float2(0.333, 0.333) * uv.xy);
        }
        else
        {
            // top row, 3 cols
            if (view == 0)
                return tex2D(_MainTex, float2(0.000, 0.75) + float2(0.333, 0.25) * uv.xy);
            else if (view == 1)
                return tex2D(_MainTex, float2(0.333, 0.75) + float2(0.333, 0.25) * uv.xy);
            else if (view == 2)
                return tex2D(_MainTex, float2(0.666, 0.75) + float2(0.333, 0.25) * uv.xy);
            else if (view == 3)
                return tex2D(_MainTex, float2(0.000, 0.50) + float2(0.333, 0.25) * uv.xy);
            else if (view == 4)
                return tex2D(_MainTex, float2(0.333, 0.50) + float2(0.333, 0.25) * uv.xy);
            else if (view == 5)
                return tex2D(_MainTex, float2(0.666, 0.50) + float2(0.333, 0.25) * uv.xy);
            else if (view == 6)
                return tex2D(_MainTex, float2(0.000, 0.25) + float2(0.333, 0.25) * uv.xy);
            else if (view == 7)
                return tex2D(_MainTex, float2(0.333, 0.25) + float2(0.333, 0.25) * uv.xy);
            else if (view == 8)
                return tex2D(_MainTex, float2(0.666, 0.25) + float2(0.333, 0.25) * uv.xy);
            else if (view == 9)
                return tex2D(_MainTex, float2(0.000, 0.00) + float2(0.333, 0.25) * uv.xy);
            else if (view == 10)
                return tex2D(_MainTex, float2(0.333, 0.00) + float2(0.333, 0.25) * uv.xy);
            else if (view == 11)
                return tex2D(_MainTex, float2(0.666, 0.00) + float2(0.333, 0.25) * uv.xy);
        }
    }
#endif
    if (perPixelCorrection == 2)
    {
#ifdef BLACK_VIEW
        if (view < _viewsX / 2)
            return clamp(tex2D(_texture_0, uv.xy) * sqrt(_Brightness), 0.0, 1.0) * smoothstep(minView - 1.0, minView, (float)view);
        else if (view >= _viewsX / 2 && view < _viewsX)
            return clamp(tex2D(_texture_1, uv.xy) * sqrt(_Brightness), 0.0, 1.0) * (1.0 - smoothstep(maxView, maxView + 1.0, (float)view));
#else
        if (view < _viewsX / 2) {
            return clamp(tex2D(_texture_0, uv.xy) * sqrt(_Brightness), 0.0, 1.0);
        }
        else if (view >= _viewsX / 2 && view < _viewsX) {
            return clamp(tex2D(_texture_1, uv.xy) * sqrt(_Brightness), 0.0, 1.0);
        }
#endif
#if SHADER_API_GLES3
        else {
            return float4(0.0, 0.0, 0.0, 1.0);
        }
#endif


    }
    else
    {
    // gets compiled to jump table. no need for binary search through 16 elements
        if (view == 0)
            return tex2D(_texture_0, uv.xy);
        else if (view == 1)
            return tex2D(_texture_1, uv.xy);
        else if (view == 2)
            return tex2D(_texture_2, uv.xy);
        else if (view == 3)
            return tex2D(_texture_3, uv.xy);
        else if (view == 4)
            return tex2D(_texture_4, uv.xy);
        else if (view == 5)
            return tex2D(_texture_5, uv.xy);
        else if (view == 6)
            return tex2D(_texture_6, uv.xy);
        else if (view == 7)
            return tex2D(_texture_7, uv.xy);
        else if (view == 8)
            return tex2D(_texture_8, uv.xy);
        else if (view == 9)
            return tex2D(_texture_9, uv.xy);
        else if (view == 10)
            return tex2D(_texture_10, uv.xy);
        else if (view == 11)
            return tex2D(_texture_11, uv.xy);
        else if (view == 12)
            return tex2D(_texture_overflow, uv_in_overflow(uv.xy, view));
        else if (view == 13)
            return tex2D(_texture_overflow, uv_in_overflow(uv.xy, view));
        else if (view == 14)
            return tex2D(_texture_overflow, uv_in_overflow(uv.xy, view));
        else if (view == 15)
            return tex2D(_texture_overflow, uv_in_overflow(uv.xy, view));
    }
    return float4(0.0, 0.0, 0.0, 1.0);
    
}
//NEED TO CHECK
// if this function were named "sample_view" and it called another function named "sample_view" it would not be possible to compile in HLSL
float4 sample_lerped_view(float2 uv, float view)
{
    float viewCount = _viewsX * _viewsY;
    float view1 = floor(view); // assume this truncates
    float view2 = int(view1 + 1.1) % viewCount; // should be equivalent to ceil mod n but who knows
    float linear_mix = view - view1;
    float non_linear_mix = smoothstep(0.0, 1.0, linear_mix);

    float4 pixL = sample_view(uv, view1);
    float4 pixR = sample_view(uv, view2);

    // assume mix in GLSL is equivalent to lerp in HLSL
    return lerp(pixL, pixR, non_linear_mix);
}
