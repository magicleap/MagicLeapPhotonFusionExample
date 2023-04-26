Shader "Custom/DesktopARBackground"
{
    Properties
    {
        _MainTex ( "Main Texture", 2D ) = "white" {} 
        _RotationDegrees ( "Rotation Degrees", Float) = 0
    }
   
    SubShader
    {      
        Pass
        {
            CGPROGRAM
           
            #pragma vertex vert
            #pragma fragment frag
            float4 _MainTex_ST;
            uniform sampler2D _MainTex;
            #include "UnityCG.cginc"

            struct vertexInput
            {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
            };
           
            struct vertexOutput
            {
                float4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
            };



            float _RotationDegrees;
            vertexOutput vert(vertexInput v)
            {  
                const float Deg2Rad = (UNITY_PI * 2.0) / 360.0;
                float rotationRadians = _RotationDegrees * Deg2Rad;
                vertexOutput o;
                v.texcoord.xy -=0.5;
                float s = sin (rotationRadians);
                float c = cos (rotationRadians);
                float2x2 rotationMatrix = float2x2( c, -s, s, c);
                rotationMatrix *=0.5;
                rotationMatrix +=0.5;
                rotationMatrix = (rotationMatrix * 2)-1;
                v.texcoord.xy = mul ( v.texcoord.xy, rotationMatrix );
                v.texcoord.xy += 0.5;


        
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
               
                return o;
            }
           
            float4 frag(vertexOutput i) : SV_Target
            {
               float2 texSize = _MainTex_ST.zw;
                float texAspect = texSize.x / texSize.y;
                // Get the screen size and aspect ratio
                float2 screenSize = _ScreenParams.xy;
                float screenAspect = screenSize.x / screenSize.y;
                 // Calculate the UV offset and scale based on the aspect ratios
                float2 offset = float2(0, 0);
                float2 scale = float2(1, 1);
                if (texAspect > screenAspect)
                {
                    // Crop the texture horizontally
                    scale.x = screenAspect / texAspect;
                    offset.x = (1 - scale.x) / 2;
                }
                else if (texAspect < screenAspect)
                {
                    // Crop the texture vertically
                    scale.y = texAspect / screenAspect;
                    offset.y = (1 - scale.y) / 2;
                }

                // Apply the offset and scale to the UV coordinates
                i.uv = i.uv * scale + offset;

                // Sample the texture and return the color
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
           
            ENDCG
        }
    }
    Fallback "Diffuse"
}