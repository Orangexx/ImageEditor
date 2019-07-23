Shader "myshaders/BSC"
{
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}
		B("B",Float) = 1
		S("S",Float) = 1
		C("C",Float) = 1
		_Gamma("_Gamma",float)=1
	}
		SubShader
		{
			Pass
			{
				ZTest Always Cull Off ZWrite Off
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				sampler2D _MainTex;
				half B;
				half S;
				half C;
				int _Gray;
				float _Gamma;
			struct v2a
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};
			v2f vert (v2a v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}
			fixed4 frag (v2f i) : SV_Target
			{
			fixed4 renderTex = tex2D(_MainTex,i.uv);
			fixed3 finalColor = renderTex.rgb*B;
			fixed luminance = 0.2125*renderTex.r + 0.7154*renderTex.g + 0.0721*renderTex.b;
			fixed3 luminanceColor = fixed3(luminance, luminance, luminance);
			finalColor = lerp(luminanceColor, finalColor, S);
			fixed3 avgColor = fixed3(0.5, 0.5, 0.5);//这是一个对比度为零的颜色
			finalColor = lerp(avgColor, finalColor, C);//对比度
			/*if(_Gray==1){
				finalColor.rgb=dot(finalColor.rgb,float3(0.299,0.587,0.114));
			}*/
			finalColor=pow(finalColor,float3(_Gamma,_Gamma,_Gamma));
			return fixed4(finalColor, renderTex.a);
			}
			ENDCG
		}
	}
			Fallback Off
}
