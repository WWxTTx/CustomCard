Shader "Custom/StarfieldShader"
{
    Properties
    {
        _MainTex ("Star Texture", 2D) = "white" {}
        _Speed ("Movement Speed", Range(0, 0.5)) = 0.01
        _Layer1Color ("Layer 1 Color", Color) = (1,1,1,1)
        _Layer2Color ("Layer 2 Color", Color) = (0.8,0.9,1,1)
        _Layer3Color ("Layer 3 Color", Color) = (0.6,0.7,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend One One // 加法混合
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _Speed;
            float4 _Layer1Color;
            float4 _Layer2Color;
            float4 _Layer3Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // 伪随机函数
            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 三层星空参数（不同速度/方向）
                float2 layer1Offset = float2(_Time.y * _Speed * 0.3, _Time.y * _Speed * 0.2);
                float2 layer2Offset = float2(_Time.y * _Speed * -0.4, _Time.y * _Speed * 0.3);
                float2 layer3Offset = float2(_Time.y * _Speed * 0.2, _Time.y * _Speed * -0.5);
                
                // 采样三层星空
                fixed4 layer1 = tex2D(_MainTex, i.uv + layer1Offset);
                fixed4 layer2 = tex2D(_MainTex, i.uv * 1.3 + layer2Offset);
                fixed4 layer3 = tex2D(_MainTex, i.uv * 1.7 + layer3Offset);
               
                // 组合三层星空并应用闪烁
                fixed4 col = layer1 * _Layer1Color;
                col += layer2 * _Layer2Color * 0.8;
                col += layer3 * _Layer3Color * 0.6;
                
                return col * 2.0; // 增强亮度
            }
            ENDCG
        }
    }
}
