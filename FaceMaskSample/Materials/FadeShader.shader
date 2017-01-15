Shader "Hide/FadeShader" {
Properties
 {
     _MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}
     _Color ("Color", Color) = (1, 1, 1, 1)
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

             struct appdata_t
             { 
                 float4 vertex : POSITION;
                 float2 texcoord : TEXCOORD0;
             };
 
             struct v2f
             {
                 float4 vertex : SV_POSITION;
                 half2 texcoord : TEXCOORD0;
             };
 
             sampler2D _MainTex;
             float4 _MainTex_ST;
             float4 _Color;
             float _Fade;

             v2f vert (appdata_t v)
             {
                 v2f o;
                 o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                 o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                 return o;
             }
             
             fixed4 frag (v2f i) : COLOR
             {
                 fixed4 base = tex2D(_MainTex, i.texcoord) * (_Color);

                 base.w =  base.w * (1 - _Fade);
                 return base;
             }
         ENDCG
     }
 }
}
