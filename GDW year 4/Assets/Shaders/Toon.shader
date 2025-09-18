Shader "Custom/Toon"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _RampTex("Ramp Texture", 2D) = "white"{}
        _Step1("1", Range(0, 1)) = 0.25
        _Step2("2", Range(0, 1)) = 0.5
        _Step3("3", Range(0, 1)) = 0.75
    }
    SubShader
    {
        CGPROGRAM
        #pragma surface surf ToonRamp

        float4 _Color;
        sampler2D _RampTex;
        float _Step1;
        float _Step2;
        float _Step3;

        float4 LightingToonRamp(SurfaceOutput s, fixed3 lightDir, fixed atten)
        {
            float diff = dot(s.Normal, lightDir);
            float h = diff * 0.5 + 0.5;

            float rampValue;
            // Assign the first section to the lightest color
            if (h < _Step1)
                rampValue = 1.2; // Lightest value
            else if (h < _Step2)
                rampValue = 1.0; // Slightly darker
            else if (h < _Step3)
                rampValue = 0.66; // Mid-tone
            else
                rampValue = 0.33; // Darkest value

            float2 rh = float2(rampValue, 0);
            float3 ramp = tex2D(_RampTex, rh).rgb;

            float4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * (ramp);
            c.a = s.Alpha;
            return c;
        }

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
        }

        ENDCG
    }
    FallBack "Diffuse"
}
