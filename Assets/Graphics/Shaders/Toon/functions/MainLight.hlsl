void MainLight_float(in float3 ClipSpacePos, in float3 WorldPos, out float3 Color, out float3 Direction) {
    #ifdef SHADERGRAPH_PREVIEW
        Color = float3(1.0, 1.0, 1.0);
        Direction = float3(0.5, 0.5, 0);
    #else
        #if SHADOWS_SCREEN
            half4 shadowCoord = ComputeScreenPos(ClipSpacePos);
        #else
            half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
        #endif

        #if _MAIN_LIGHT_SHADOWS_CASCADE || _MAIN_LIGHT_SHADOWS
            Light light = GetMainLight(shadowCoord);
        #else
            Light light = GetMainLight();
        #endif

        Color = light.color;
        Direction = light.direction;
    #endif
}