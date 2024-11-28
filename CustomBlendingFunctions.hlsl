#ifndef CUSTOM_BLENDING_FUNCTIONS
#define CUSTOM_BLENDING_FUNCTIONS

 float3 ConvertLinearToRGB(float3 In)
{
    float3 sRGBLo = In * 12.92;
    float3 sRGBHi = (pow(max(abs(In), 1.192092896e-07), float3(1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
    return float3(In <= 0.0031308) ? sRGBLo : sRGBHi;
}

float3 ConvertRGBToLinear(float3 In)
{
    float3 linearRGBLo = In / 12.92;;
    float3 linearRGBHi = pow(max(abs((In + 0.055) / 1.055), 1.192092896e-07), float3(2.4, 2.4, 2.4));
    return float3(In <= 0.04045) ? linearRGBLo : linearRGBHi;
}

float3 Blend_Darken(float3 baseColor, float3 blendColor, float opacity)
{
    baseColor = ConvertLinearToRGB(baseColor);
    blendColor = ConvertLinearToRGB(blendColor);
    float3 minimumValue = min(blendColor, baseColor);
    float3 finalValue = lerp(baseColor, minimumValue, opacity);
    return ConvertRGBToLinear(finalValue);
}

float3 Blend_HardLight(float3 baseColor, float3 blendColor, float opacity)
{
    baseColor = ConvertLinearToRGB(baseColor);
    blendColor = ConvertLinearToRGB(blendColor);
    float3 result1 = 1.0 - 2.0 * (1.0 - baseColor) * (1.0 - blendColor);
    float3 result2 = 2.0 * baseColor * blendColor;
    float3 zeroOrOne = step(blendColor, 0.5);
    float3 finalValue = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
    finalValue = lerp(baseColor, finalValue, opacity);
    return ConvertRGBToLinear(finalValue);
}

float3 Blend_HardMix(float3 baseColor, float3 blendColor, float opacity)
{
    baseColor = ConvertLinearToRGB(baseColor);
    blendColor = ConvertLinearToRGB(blendColor);
    float3 finalValue = step(1 - baseColor, blendColor);
    finalValue = lerp(baseColor, finalValue, opacity);
    return ConvertRGBToLinear(finalValue);
}

float3 Blend_Lighten(float3 baseColor, float3 blendColor, float opacity)
{
    baseColor = ConvertLinearToRGB(baseColor);
    blendColor = ConvertLinearToRGB(blendColor);
    float3 finalValue = max(blendColor, baseColor);
    finalValue = lerp(baseColor, finalValue, opacity);
    return ConvertRGBToLinear(finalValue);
}

float3 Blend_LinearLight(float3 baseColor, float3 blendColor, float opacity)
{
    baseColor = ConvertLinearToRGB(baseColor);
    blendColor = ConvertLinearToRGB(blendColor);
    float3 finalValue = blendColor < 0.5 ? max(baseColor + (2 * blendColor) - 1, 0) : min(baseColor + 2 * (blendColor - 0.5), 1);
    finalValue = lerp(baseColor, finalValue, opacity);
    return ConvertRGBToLinear(finalValue);
}

float3 Blend_Overlay(float3 baseColor, float3 blendColor, float opacity)
{
    baseColor = ConvertLinearToRGB(baseColor);
    blendColor = ConvertLinearToRGB(blendColor);
    float3 result1 = 1.0 - 2.0 * (1.0 - baseColor) * (1.0 - blendColor);
    float3 result2 = 2.0 * baseColor * blendColor;
    float3 zeroOrOne = step(baseColor, 0.5);
    float3 finalValue = result2 * zeroOrOne + (1.0 - zeroOrOne) * result1;
    finalValue = lerp(baseColor, finalValue, opacity);
    return ConvertRGBToLinear(finalValue);
}

float3 Blend_Screen(float3 baseColor, float3 blendColor, float opacity)
{
    baseColor = ConvertLinearToRGB(baseColor);
    blendColor = ConvertLinearToRGB(blendColor);
    float3 finalValue = 1.0 - (1.0 - blendColor) * (1.0 - baseColor);
    finalValue = lerp(baseColor, finalValue, opacity);
    finalValue = finalValue;
    return ConvertRGBToLinear(finalValue);
}

float3 Blend_SoftLight(float3 baseColor, float3 blendColor, float opacity)
{
    baseColor = ConvertLinearToRGB(baseColor);
    blendColor = ConvertLinearToRGB(blendColor);
    float3 result1 = 2.0 * baseColor * blendColor + baseColor * baseColor * (1.0 - 2.0 * blendColor);
    float3 result2 = sqrt(baseColor) * (2.0 * blendColor - 1.0) + 2.0 * baseColor * (1.0 - blendColor);
    float3 zeroOrOne = step(0.5, blendColor);
    float3 finalValue = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
    finalValue = lerp(baseColor, finalValue, opacity);
    finalValue = finalValue;
    return ConvertRGBToLinear(finalValue);
}

float3 Blend_VividLight(float3 baseColor, float3 blendColor, float opacity) 
{
    baseColor = ConvertLinearToRGB(baseColor);
    blendColor = ConvertLinearToRGB(blendColor);
    float3 result1 = 1.0 - (1.0 - blendColor) / (2.0 * baseColor);
    float3 result2 = blendColor / (2.0 * (1.0 - baseColor));
    float3 zeroOrOne = step(0.5, baseColor);
    float3 finalValue = result2 * zeroOrOne + (1 - zeroOrOne) * result1;
    finalValue = lerp(baseColor, finalValue, opacity);
    return ConvertRGBToLinear(finalValue);
}

#endif // CUSTOM_BLENDING_FUNCTIONS