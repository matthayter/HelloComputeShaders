Shader "Custom/HelloWorldSurfShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert nolightmap
        #pragma instancing_options procedural:setup

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            struct MyBufferData {
                float3 position;
                float3 direction;
                float speed;
                float padding0;
            };
            StructuredBuffer<MyBufferData> sharedBuffer;
        #endif

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float3 _MyPos;
        float4x4 _LookAtMatrix;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float4x4 look_at_matrix(float3 at, float3 eye, float3 up) {
			float3 zaxis = normalize(at - eye);
			float3 xaxis = normalize(cross(up, zaxis));
			float3 yaxis = cross(zaxis, xaxis);
			return float4x4(
				xaxis.x, yaxis.x, zaxis.x, 0,
				xaxis.y, yaxis.y, zaxis.y, 0,
				xaxis.z, yaxis.z, zaxis.z, 0,
				0, 0, 0, 1
			);
		}

        void setup() {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            _MyPos = sharedBuffer[unity_InstanceID].position;
            _LookAtMatrix = look_at_matrix(_MyPos + sharedBuffer[unity_InstanceID].direction, _MyPos, float3(0.0, 1.0, 0.0));
            #endif
        }

        void vert(inout appdata_full v) {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            v.vertex = mul(_LookAtMatrix, v.vertex);
            v.vertex.xyz += _MyPos;
            #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
