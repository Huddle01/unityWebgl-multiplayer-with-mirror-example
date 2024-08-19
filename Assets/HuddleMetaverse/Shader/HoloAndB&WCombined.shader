Shader "Custom/BlackAndWhiteHologramShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Speed("Scroll Speed", Range(0.1, 10.0)) = 1.0
        _Intensity("Intensity", Range(0.0, 1.0)) = 0.5
        _RayCount("Ray Count", Range(1, 100)) = 10
        _RayIntensity("Ray Intensity", Range(0.0, 1.0)) = 0.5
        _RayWidth("Ray Width", Range(10.0, 100.0)) = 20.0
        _RayColor("Ray Color", Color) = (1, 1, 1, 1)
        _RayTransparency("Ray Transparency", Range(0.0, 1.0)) = 0.5
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Color;
                float _Speed;
                float _Intensity;
                float _RayCount;
                float _RayIntensity;
                float _RayWidth;
                float4 _RayColor;
                float _RayTransparency;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 uv = i.uv;
                    float scrollOffset = _Time.y * _Speed;

                    // Sample the main texture
                    float3 col = tex2D(_MainTex, uv).rgb;

                    // Convert to grayscale
                    float gray = dot(col, float3(0.299, 0.587, 0.114));
                    float3 grayscale = float3(gray, gray, gray);

                    // Apply hologram effect
                    float hologramEffect = 0.0;
                    for (int j = 0; j < _RayCount; ++j)
                    {
                        hologramEffect += sin((uv.y + scrollOffset + j * 0.1) * _RayWidth) * 0.5 + 0.5;
                    }
                    hologramEffect /= _RayCount;

                    float3 hologram = grayscale * hologramEffect * _RayIntensity;
                    hologram = lerp(grayscale, hologram, _Intensity);

                    float3 rayEffect = _RayColor.rgb * hologramEffect * _RayIntensity;
                    rayEffect = lerp(rayEffect, grayscale, 1.0 - _RayTransparency);

                    return fixed4(hologram * _Color.rgb + rayEffect, 1.0);
                }
                ENDCG
            }
        }
            FallBack "Diffuse"
}
