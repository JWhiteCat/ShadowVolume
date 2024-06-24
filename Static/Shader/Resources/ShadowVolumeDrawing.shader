Shader "Hidden/ShadowVolume/Drawing"
{
	Properties
	{
	}

	HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
    	UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	
	uniform half4 _ShadowColor;
	uniform sampler2D _ShadowVolumeRT;
	uniform sampler2D _ShadowVolumeColorRT;

	v2f vert_sv_stencil (appdata v)
	{
		v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, v);
		o.vertex = TransformWorldToHClip(v.vertex.xyz);
		return o;
	}
	
	half4 frag_sv_stencil (v2f i) : SV_Target
	{
		return half4(0,0,0,0);
	}

	v2f vert_shadow(appdata v)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, v);
		o.vertex = v.vertex;

		if (_ProjectionParams.x < 0)
		{
			o.vertex.y *= -1;
		}

		return o;
	}

	half4 frag_shadow(v2f i) : SV_Target
	{
		return _ShadowColor;
	}

	v2f vert_overlay_shadow (appdata v)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, v);
		o.vertex = TransformWorldToHClip(v.vertex);
		o.uv = v.uv;
		return o;
	}

	half4 frag_overlay_shadow(v2f i) : SV_Target
	{
		half4 shadow = tex2D(_ShadowVolumeRT, i.uv);
		half4 color = tex2D(_ShadowVolumeColorRT, i.uv);

		// Bright dark area in the shadow
		/*
		half s = shadow.r < 0.8 ? 1 : 0;

		half gray = (color.r + color.g + color.b) * 0.3333;
		gray -= 0.4;// dark controller
		gray = saturate(gray) * s + 1;

		return shadow * color * gray;
		*/

		return shadow * color;
	}
    ENDHLSL

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		// ZFail
		// Pass 0
		Pass
		{
			Stencil{
				Ref 1
				Comp Always
				ZFail IncrSat
			}
			ZWrite Off
			ColorMask 0
			Cull Front
			HLSLPROGRAM
			#pragma vertex vert_sv_stencil
			#pragma fragment frag_sv_stencil
			#pragma multi_compile_instancing
			ENDHLSL
		}
		// Pass 1
		Pass
		{
			Stencil{
				Ref 1
				Comp Always
				ZFail DecrSat
			}
			ZWrite Off
			ColorMask 0
			Cull Back
			HLSLPROGRAM
			#pragma vertex vert_sv_stencil
			#pragma fragment frag_sv_stencil
			#pragma multi_compile_instancing
			ENDHLSL
		}


		// ZPass
		// Pass 0
		// Pass
		// {
		//  	Stencil{
		//  		Ref 1
		//  		Comp Always
		//  		Pass IncrSat
		//  	}
		//  	ZWrite Off
		//  	ColorMask 0
		//  	Cull Back
		//  	HLSLPROGRAM
		//  	#pragma vertex vert_sv_stencil
		//  	#pragma fragment frag_sv_stencil
		//  	ENDHLSL
		//  }
		// Pass 1
		// Pass
		// {
		//  	Stencil{
		//  		Ref 1
		//  		Comp Always
		//  		Pass DecrSat
		//  	}
		//  	ZWrite Off
		//  	ColorMask 0
		//  	Cull Front
		//  	HLSLPROGRAM
		//  	#pragma vertex vert_sv_stencil
		//  	#pragma fragment frag_sv_stencil
		//  	ENDHLSL
		// }


		// Draw Shadow
		// Pass 2
		Pass
		{
			Stencil{
				Ref 0
				Comp NotEqual
				Pass Keep
			}
			Blend Zero SrcColor
			ZWrite Off
			ColorMask RGB
			Cull Back
			ZTest Always
			HLSLPROGRAM
			#pragma vertex vert_shadow
			#pragma fragment frag_shadow
			#pragma multi_compile_instancing
			ENDHLSL
		}

		// Clear Stencil Buffer
		// Pass 3
		Pass
		{
			Stencil{
				Ref 0
				Comp Always
				Pass Replace
			}
			ColorMask 0
			Cull Back
			ZWrite Off
			ZTest Always
			HLSLPROGRAM
			#pragma vertex vert_shadow
			#pragma fragment frag_shadow
			#pragma multi_compile_instancing
			ENDHLSL
		}

		// Two-Side Stencil
		// Pass 4
		Pass
		{
			Stencil{
				Ref 1
				Comp Always
				ZFailBack IncrWrap
				ZFailFront DecrWrap
			}
			ZWrite Off
			ColorMask 0
			Cull Off
			HLSLPROGRAM
			#pragma vertex vert_sv_stencil
			#pragma fragment frag_sv_stencil
			#pragma multi_compile_instancing
			ENDHLSL
		}

		// RenderTexture Composite
		// Overlay shadow on the color
		// Pass 5
		Pass
		{
			ZWrite Off
			ColorMask RGB
			Cull Back
			ZTest Always
			HLSLPROGRAM
			#pragma vertex vert_overlay_shadow
			#pragma fragment frag_overlay_shadow
			#pragma multi_compile_instancing
			ENDHLSL
		}

		// RenderTexture Composite
		// Draw Shadow
		// Pass 6
		Pass
		{
			Stencil{
				Ref 0
				Comp NotEqual
				Pass Keep
			}
			ZWrite Off
			ColorMask RGB
			Cull Back
			ZTest Always
			HLSLPROGRAM
			#pragma vertex vert_shadow
			#pragma fragment frag_shadow
			#pragma multi_compile_instancing
			ENDHLSL
		}
	}
}
