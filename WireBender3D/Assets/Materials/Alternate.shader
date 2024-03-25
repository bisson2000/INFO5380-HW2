Shader "Unlit/Alternate"
{
    Properties
    {
        _Color1 ("Color1", Color) = (1, 1, 1, 1)
        _Color2 ("Color2", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint vid : SV_VertexID;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Declare the variables to hold the colours
            // we want to show above & below the line.
            fixed4 _Color1;
            fixed4 _Color2;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // third texture coordinate
                o.uv.z = (float)(v.vid % 8U < 4U);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // This turns the worldspace y position
                // into a tight blending ramp from 0 to 1.
                float blend = saturate(i.uv.z);

                // This blends between the two colours.
                fixed4 col = lerp(_Color1, _Color2, blend);

                return col;
            }
            ENDCG
        }
    }
}