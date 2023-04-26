Shader "Custom/Web Camera Shader"
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
                return tex2D( _MainTex, i.uv );
            }
           
            ENDCG
        }
    }
    Fallback "Diffuse"
}