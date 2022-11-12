void GetMainLightDirection_float(in float3 ObjectPos, in float3 WorldPos, out float3 Direction) {
    #ifdef SHADERGRAPH_PREVIEW
        Direction = float3(0.5, 0.5, 0);
    #else
        #if SHADOWS_SCREEN
            half4 shadowCoord = ComputeScreenPos(ObjectPos);
        #else
            half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
        #endif

        #if _MAIN_LIGHT_SHADOWS_CASCADE || _MAIN_LIGHT_SHADOWS
            Light light = GetMainLight(shadowCoord);
        #else
            Light light = GetMainLight();
        #endif

        Direction = light.direction;
    #endif
}