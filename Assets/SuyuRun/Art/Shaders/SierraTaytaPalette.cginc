#ifndef SUYURUN_SIERRA_TAYTA_PALETTE
#define SUYURUN_SIERRA_TAYTA_PALETTE
// Discrete mapping within the existing Sierra palette. Silhouette/alpha/UVs
// are unchanged: neutral cloth highlights and warmer, less luminous skin.
float3 SierraTaytaMatte(float3 value)
{
    float3 c=value;
    #ifndef UNITY_COLORSPACE_GAMMA
    c=LinearToGammaSpace(c);
    #endif
    float3 rgb=c*255.0;
    if(all(abs(rgb-float3(255,255,255))<2.5)||all(abs(rgb-float3(232,237,242))<2.5)||all(abs(rgb-float3(255,243,208))<2.5))c=float3(184,184,190)/255.0;
    else if(all(abs(rgb-float3(245,223,160))<2.5)||all(abs(rgb-float3(232,200,120))<2.5))c=float3(217,123,85)/255.0;
    else if(all(abs(rgb-float3(240,211,117))<2.5)||all(abs(rgb-float3(224,187,69))<2.5))c=float3(201,162,39)/255.0;
    #ifndef UNITY_COLORSPACE_GAMMA
    c=GammaToLinearSpace(c);
    #endif
    return c;
}
#endif
