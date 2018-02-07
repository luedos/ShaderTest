Shader "Unlit/Sh_Water"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Speed("Wave speed", Range(-15,0.1)) = -1
		_Frequency("Frequency", Float) = 10
		_Amplitude("Amplitude", Float) = 1
		_MaxSpeed("Max speed", Range(0.0,15)) = 5
		[Toggle]_AffectReflection("Affect reflection by wave", Float) = 0
		[Toggle]_UseNormals("Obscure midwaves", Float) = 0
		[HideInInspector] _ReflectionTex("", 2D) = "white" {}
	}
		SubShader
	{
		Tags{
		"RenderType" = "Transparent"
		"Queue" = "Transparent"
		"LightMode" = "ForwardBase"
	}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag


		#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float3 normal : NORMAL;
			float4 vertex : SV_POSITION;
			float4 refl : TEXCOORD1;
		};

		sampler2D _MainTex;
		sampler2D _ReflectionTex;
		float4	_MainTex_ST;
		float4	_Color;
		float	_Speed;
		float	_Frequency;
		float	_Amplitude;
		half4	_DistArray[10];
		half4	_SpeedArray[10];
		float	_ArrayLength = 0;
		float	_MaxSpeed;
		float	_AffectReflection;
		float	_UseNormals;

		v2f vert(appdata v)
		{
			v2f o;

			float4 offset = float4(0, 0, 0, 0);

			float vertOffset;
			float test;
			float2 SpeedOffset;
			if (_ArrayLength > 0)
				for (int i = 0; i < _ArrayLength - 0.1; ++i)
				{
					// offseting circle in case we moving
					if (_SpeedArray[i].z > 0)
					{
						// basicly clamping offset
						if (_SpeedArray[i].z / _MaxSpeed > 1)
						{
							SpeedOffset.x = _SpeedArray[i].x * _DistArray[i].z * 0.8 / _SpeedArray[i].z;
							SpeedOffset.y = _SpeedArray[i].y * _DistArray[i].z * 0.8 / _SpeedArray[i].z;
						}
						else
						{
							SpeedOffset.x = _SpeedArray[i].x * _DistArray[i].z * 0.8 / _MaxSpeed;
							SpeedOffset.y = _SpeedArray[i].y * _DistArray[i].z * 0.8 / _MaxSpeed;
						}

						test = 1 - ((_DistArray[i].x - v.vertex.x + SpeedOffset.x) * (_DistArray[i].x - v.vertex.x + SpeedOffset.x)
							+ (_DistArray[i].y - v.vertex.z + SpeedOffset.y) * (_DistArray[i].y - v.vertex.z + SpeedOffset.y)) / (_DistArray[i].z * _DistArray[i].z);
					}
					else
						test = 1 - ((_DistArray[i].x - v.vertex.x) * (_DistArray[i].x - v.vertex.x)
							+ (_DistArray[i].y - v.vertex.z) * (_DistArray[i].y - v.vertex.z)) / (_DistArray[i].z * _DistArray[i].z);

					// if we in the circle
					if (test > 0)
					{	

						// if we moving change frequency based on vertex position (if vertex in the back frequency lower)
						if (_SpeedArray[i].z > 0)
						{
							SpeedOffset.x = 1.5 + 1 * dot(float2(_SpeedArray[i].x, _SpeedArray[i].y), float2(_DistArray[i].x - v.vertex.x, _DistArray[i].y - v.vertex.z))
								/ (length(float2(v.vertex.x - _DistArray[i].x, v.vertex.z - _DistArray[i].y))*_SpeedArray[i].z);
							if (_SpeedArray[i].z < 0.25 * _MaxSpeed)
								SpeedOffset.x = 1 + (SpeedOffset.x - 1) * _SpeedArray[i].z / (0.25 * _MaxSpeed);
						}
						else
							SpeedOffset.x = 1;

						// circles allways going from object position
						vertOffset = (v.vertex.x - _DistArray[i].x) * (v.vertex.x - _DistArray[i].x) + (v.vertex.z - _DistArray[i].y) * (v.vertex.z - _DistArray[i].y);

						vertOffset = (test * 0.8 + 0.2) * _DistArray[i].w * sin(_Time.w * _Speed + vertOffset * _Frequency * SpeedOffset.x);

						offset.y += vertOffset;
					}

				}

			o.vertex = UnityObjectToClipPos(v.vertex + offset);

			if(_AffectReflection > 0.5)
				o.refl = ComputeScreenPos(UnityObjectToClipPos(v.vertex));
			else
				o.refl = ComputeScreenPos(o.vertex);

			o.normal = v.normal + offset;
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);			

			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			// sample the texture
			fixed4 col = fixed4(1,1,1,1);

			if(_UseNormals > 0.5)
				col.rgb *= (dot(i.normal, float3 (0,1,0)) * 0.5 + 0.5);

			fixed4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(i.refl));

			return _Color * col * refl;
		}
			ENDCG
		}
	}
}
