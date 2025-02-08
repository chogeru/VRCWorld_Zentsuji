Shader "Universal Render Pipeline/Custom/UvScroll"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Speed("Speed", float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Int) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        Cull [_CullMode]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


            struct Attributes
            {
                float4 positionOS   : POSITION;
                // uv 変数には特定の頂点のテクスチャにおける UV 座標が含まれる
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                // uv 変数には特定の頂点のテクスチャにおける UV 座標が含まれる
                float2 uv           : TEXCOORD0;
            };

            // このマクロは _BaseMap を Texture2D オブジェクトとして宣言する
            TEXTURE2D(_BaseMap);
            // このマクロは _BaseMap テクスチャのサンプラーを宣言する
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                // フラグメントシェーダーで _BaseMap 変数を使用できるように _BaseMap_ST 変数を宣言
                // タイリングおよびオフセットを機能させるために _ST サフィックスが必要
                float4 _BaseMap_ST;
                // _BaseColor 変数を宣言
                half4 _BaseColor;
                // _Speed 変数を宣言
                float _Speed;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // UVをスクロールさせる
                float2 offset = float2(_Time.x * _Speed, 0);

                // TRANSFORM_TEX マクロはタイリングとオフセット変換を行う
                // OUT.uv = IN.uvでは無効
                OUT.uv = TRANSFORM_TEX(IN.uv + offset, _BaseMap);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // SAMPLE_TEXTURE2D マクロは指定されたサンプラーでテクスチャをサンプリングする
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                // テクスチャに色を混ぜ合わせる
                return baseTex * _BaseColor;
            }
            ENDHLSL
        }
    }
}