// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "2D/Light View Mesh"
 {  
     Properties
     {
     }
     SubShader
     {
         Tags 
         { 
             "RenderType" = "Opaque" 
             "Queue" = "Transparent+1" 
         }
 
         Pass
         {
             ZWrite Off
             Blend One One
             //BlendOp Add
             Cull Off
  
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile DUMMY PIXELSNAP_ON
             #include "UnityCG.cginc"
  
             sampler2D _MainTex;
             float4 _Color;
			 float _LightFactor;
 
             struct appdata_t
			 {
				float4 vertex   : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
                float2 dir   : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};
  
            float2 _LightPos;
            float _Radius;
            float _Alpha;
            float _Pow;
            fixed4 _LightColor;
            float _BrightNess;
             v2f vert(appdata_t v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 float2 dir = v.vertex.xy - _LightPos;
                 o.dir = dir;
                 return o;
             }
                                                     
             float4 frag(v2f i) : COLOR
             {
                 //float alpha = pow(i.normedDis, _Pow) * _Alpha;
				 //float4 c = _LightColor;
                 //c.xyz *= _Alpha;
                 //c.xyz *= c.a;
                 /*
                 if(i.vertex.x > 0) {
                     c = float4(1, 1, 1, 1);
                 }
                 else {
                     c = float4(0, 0, 0, 0);
                 }
                 */
                 float normedDis =  clamp(length(i.dir) / _Radius, 0, 1);
                 float alpha = pow(1 - normedDis, _Pow);
                 alpha *= _BrightNess;
                 float4 c = float4(_LightColor.rgb * alpha, 1);
                 //float4 c = float4(i.dir.x / _Radius, i.dir.y / _Radius,1,1);
                 //c.rgb *= c.a;
                 //c.a = 1;
                 return c;
             }
 
             ENDCG
         }
     }
 }
