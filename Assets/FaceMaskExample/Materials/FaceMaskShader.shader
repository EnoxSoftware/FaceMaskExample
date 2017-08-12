Shader "Hide/FaceMaskShader" {
Properties
 {
     _MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}
     _MaskTex ("Mask", 2D) = "white" {}
     _Color ("Color", Color) = (0.5, 0.5, 0.5, 0.5)
     _Fade ("Fade", Range(0,1)) = 0
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

             v2f vert (appdata_t v)
             {
                 v2f o;
                 o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                 o.uv1 = TRANSFORM_TEX(v.texcoord, _MainTex);
                 o.uv2 = TRANSFORM_TEX(v.texcoord1, _MaskTex);
                 return o;
             }
             
             half4 frag (v2f i) : COLOR
             {
                 half4 base = tex2D(_MainTex, i.uv1) * (_Color * 2.0f);
                 half4 mask = tex2D (_MaskTex, i.uv2);

                 base.w = base.w * mask.x * mask.x * mask.x * (1 - _Fade);
                 return base;
             }
         ENDCG
     }
 }
}
