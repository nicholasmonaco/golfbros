Shader "Dither/Dither Transparent"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _Dither ("Dither", Range (0, 1)) = 1
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"
            #include "Dither Functions.cginc"
            #pragma vertex vert alpha
            #pragma fragment frag alpha

            // Make fog work
            #pragma multi_compile_fog


            uniform fixed4 _LightColor0;
            float4 _Color;
            float4 _MainTex_ST;         // For the Main Tex UV transform
            sampler2D _MainTex;         // Texture used for the line
            float _Dither;


            struct v2f
            {
                float4 pos      : POSITION;
                float4 col      : COLOR;
                float2 uv       : TEXCOORD0;
                float4 spos     : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o, o.pos);

                float4 norm = mul(unity_ObjectToWorld, v.normal);
                float3 normalDirection = normalize(norm.xyz);
                float4 AmbientLight = UNITY_LIGHTMODEL_AMBIENT;
                float4 LightDirection = normalize(_WorldSpaceLightPos0);
                float4 DiffuseLight = saturate(dot(LightDirection, -normalDirection)) * _LightColor0;
                o.col = float4(AmbientLight + DiffuseLight);
                o.spos = ComputeScreenPos(o.pos);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = _Color * tex2D(_MainTex, i.uv);
                ditherClip(i.spos.xy / i.spos.w, _Dither);

                // Apply fog
                float4 c = col * i.col;
                UNITY_APPLY_FOG(i.fogCoord, c);

                // return col * i.col;
                return c;
            }
            ENDCG
        }
    }
}
