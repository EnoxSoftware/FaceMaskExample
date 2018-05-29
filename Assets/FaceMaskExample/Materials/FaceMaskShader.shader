Shader "Hide/FaceMaskShader" {
Properties
 {
     _MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}
     _MaskTex ("Mask", 2D) = "white" {}
     _Color ("Color", Color) = (0.5, 0.5, 0.5, 0.5)
     _Fade ("Fade", Range(0,1)) = 0

     _ColorCorrection ("ColorCorrection", Range(0,1)) = 0
     _LUTTex ("LUTTex", 2D) = "black" {}
 }
 
 SubShader
 {
     LOD 100

     Tags
     {
         "Queue" = "Transparent"
         "IgnoreProjector" = "True"
         "RenderType" = "Transparent"
         "PreviewType"="Plane"
     }
     Lighting Off
     ZWrite Off
     Blend SrcAlpha OneMinusSrcAlpha

     Pass
     {
         CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #include "UnityCG.cginc"

             sampler2D _MainTex;
             sampler2D _MaskTex;
             float4 _Color;
             float _Fade;

             float _ColorCorrection;
             sampler2D _LUTTex;

             struct appdata_t
             { 
                 float4 vertex : POSITION;
                 float2 texcoord : TEXCOORD0;
                 float2 texcoord1 : TEXCOORD1;
             };
 
             struct v2f
             {
                 float4 pos : SV_POSITION;
                 float2 uv1 : TEXCOORD0;
                 float2 uv2 : TEXCOORD1;
             };
             
             float4 _MainTex_ST;
             float4 _MaskTex_ST;
             float4 _LUTTex_ST;

             v2f vert (appdata_t v)
             {
                 v2f o;
                 o.pos = UnityObjectToClipPos(v.vertex);
                 o.uv1 = TRANSFORM_TEX(v.texcoord, _MainTex);
                 o.uv2 = TRANSFORM_TEX(v.texcoord1, _MaskTex);
                 return o;
             }

             half4 frag (v2f i) : COLOR
             {
                 half4 base = tex2D(_MainTex, i.uv1);
                 half4 mask = tex2D (_MaskTex, i.uv2);

                 if (_ColorCorrection > 0)
                 {
                    float w = base.w;
                    float3 lut_base;

                    lut_base.r = tex2D(_LUTTex, float2(base.r, 0)).r;
                    lut_base.g = tex2D(_LUTTex, float2(base.g, 0)).g;
                    lut_base.b = tex2D(_LUTTex, float2(base.b, 0)).b;

                    base = half4(lerp(base.rgb, lut_base.rgb, _ColorCorrection), w);
                 }

                 base = base * (_Color * 2.0f);

                 base.w = base.w * mask.x * mask.x * mask.x * (1 - _Fade);
                 return base;
             }
         ENDCG
     }
 }
}
