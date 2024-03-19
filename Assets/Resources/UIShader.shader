// Made with Amplify Shader Editor v1.9.2.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UIShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _OutlineColor("OutlineColor", Color) = (0,0,0,1)
        _OutlineThickness("OutlineThickness", Range( 0 , 25)) = 1
        [HideInInspector] _texcoord( "", 2D ) = "white" {}

    }

    SubShader
    {
		LOD 0

        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }

        Stencil
        {
        	Ref [_Stencil]
        	ReadMask [_StencilReadMask]
        	WriteMask [_StencilWriteMask]
        	Comp [_StencilComp]
        	Pass [_StencilOp]
        }


        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        
        Pass
        {
            Name "Default"
        CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #define ASE_NEEDS_FRAG_COLOR


            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4  mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
                
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;

            uniform float _OutlineThickness;
            uniform float4 _OutlineColor;

            
            v2f vert(appdata_t v )
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                

                v.vertex.xyz +=  float3( 0, 0, 0 ) ;

                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.texcoord = v.texcoord;
                OUT.mask = float4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN ) : SV_Target
            {
                //Round up the alpha color coming from the interpolator (to 1.0/256.0 steps)
                //The incoming alpha could have numerical instability, which makes it very sensible to
                //HDR color transparency blend, when it blends with the world's texture.
                const half alphaPrecision = half(0xff);
                const half invAlphaPrecision = half(1.0/alphaPrecision);
                IN.color.a = round(IN.color.a * alphaPrecision)*invAlphaPrecision;

                float2 uv_MainTex = IN.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                float4 tex2DNode1 = tex2D( _MainTex, uv_MainTex );
                float temp_output_228_0 = ( _OutlineThickness * 0.01 );
                float4 appendResult13_g6 = (float4(temp_output_228_0 , 0.0 , 0.0 , 0.0));
                float4 break4_g6 = appendResult13_g6;
                float4 appendResult5_g6 = (float4(break4_g6.r , break4_g6.g , 0.0 , 0.0));
                float2 texCoord8_g6 = IN.texcoord.xy * float2( 1,1 ) + appendResult5_g6.rg;
                float4 appendResult3_g6 = (float4(0.0 , temp_output_228_0 , 0.0 , 0.0));
                float4 break6_g6 = appendResult3_g6;
                float4 appendResult7_g6 = (float4(break6_g6.r , break6_g6.g , 0.0 , 0.0));
                float2 texCoord11_g6 = IN.texcoord.xy * float2( 1,1 ) + appendResult7_g6.rg;
                float2 texCoord18_g6 = IN.texcoord.xy * float2( 1,1 ) + -appendResult5_g6.rg;
                float2 texCoord17_g6 = IN.texcoord.xy * float2( 1,1 ) + -appendResult7_g6.rg;
                float temp_output_204_0 = ( _OutlineThickness * -0.01 );
                float4 appendResult13_g5 = (float4(temp_output_204_0 , 0.0 , 0.0 , 0.0));
                float4 break4_g5 = appendResult13_g5;
                float4 appendResult5_g5 = (float4(break4_g5.r , break4_g5.g , 0.0 , 0.0));
                float2 texCoord8_g5 = IN.texcoord.xy * float2( 1,1 ) + appendResult5_g5.rg;
                float4 appendResult3_g5 = (float4(0.0 , temp_output_204_0 , 0.0 , 0.0));
                float4 break6_g5 = appendResult3_g5;
                float4 appendResult7_g5 = (float4(break6_g5.r , break6_g5.g , 0.0 , 0.0));
                float2 texCoord11_g5 = IN.texcoord.xy * float2( 1,1 ) + appendResult7_g5.rg;
                float2 texCoord18_g5 = IN.texcoord.xy * float2( 1,1 ) + -appendResult5_g5.rg;
                float2 texCoord17_g5 = IN.texcoord.xy * float2( 1,1 ) + -appendResult7_g5.rg;
                float clampResult173 = clamp( ( ( tex2D( _MainTex, texCoord8_g6 ).a + tex2D( _MainTex, texCoord11_g6 ).a + tex2D( _MainTex, texCoord18_g6 ).a + tex2D( _MainTex, texCoord17_g6 ).a ) + ( tex2D( _MainTex, texCoord8_g5 ).a + tex2D( _MainTex, texCoord11_g5 ).a + tex2D( _MainTex, texCoord18_g5 ).a + tex2D( _MainTex, texCoord17_g5 ).a ) ) , 0.0 , 1.0 );
                

                half4 color = ( ( IN.color * ( tex2DNode1 * tex2DNode1.a ) ) + ( ( clampResult173 - tex2DNode1.a ) * _OutlineColor ) );

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color.rgb *= color.a;

                return color;
            }
        ENDCG
        }
    }
    CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19202
Node;AmplifyShaderEditor.RangedFloatNode;172;-3091.534,2167.91;Inherit;False;Property;_OutlineThickness;OutlineThickness;2;0;Create;True;0;0;0;False;0;False;1;0;0;25;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;113;-2963.125,2245.38;Inherit;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;0;False;0;False;0.01;0.025;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;208;-2760.611,2378.794;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;228;-2524.03,2201.085;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;204;-2525.884,2310.208;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;165;224.7644,2187.039;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;221;552.6519,2188.049;Float;False;True;-1;2;ASEMaterialInspector;0;3;UIShader;5056123faa0c79b47ab6ad7e8bf059a4;True;Default;0;0;Default;2;False;True;3;1;False;;10;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;True;True;True;True;True;0;True;_ColorMask;False;False;False;False;False;False;False;True;True;0;True;_Stencil;255;True;_StencilReadMask;255;True;_StencilWriteMask;0;True;_StencilComp;0;True;_StencilOp;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;2;False;;True;0;True;unity_GUIZTestMode;False;True;5;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;CanUseSpriteAtlas=True;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;0;;0;0;Standard;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;207;-3336.49,1727;Inherit;False;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;159;-767.9586,2667.822;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;141;-1042.218,2762.243;Inherit;False;Property;_OutlineColor;OutlineColor;1;0;Create;True;0;0;0;False;0;False;0,0,0,1;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;222;-2148.403,2994.086;Inherit;False;2D_Outline;-1;;5;a5f9cdc6d1c90874c9ba2b9cf719842f;0;3;19;SAMPLER2D;0;False;20;FLOAT;0;False;21;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;223;-2150.516,2814.646;Inherit;False;2D_Outline;-1;;6;a5f9cdc6d1c90874c9ba2b9cf719842f;0;3;19;SAMPLER2D;0;False;20;FLOAT;0;False;21;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;202;-1868.899,2864.479;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;173;-1619.18,2862.76;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;120;-1040.157,2524.255;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-2095.715,2016.18;Inherit;True;Global;_main;main;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;181;-1675.377,2047.288;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;10;-1711.063,1800.223;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;252;-1356.056,2025.556;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
WireConnection;208;0;113;0
WireConnection;228;0;172;0
WireConnection;228;1;113;0
WireConnection;204;0;172;0
WireConnection;204;1;208;0
WireConnection;165;0;252;0
WireConnection;165;1;159;0
WireConnection;221;0;165;0
WireConnection;159;0;120;0
WireConnection;159;1;141;0
WireConnection;222;19;207;0
WireConnection;222;20;204;0
WireConnection;222;21;204;0
WireConnection;223;19;207;0
WireConnection;223;20;228;0
WireConnection;223;21;228;0
WireConnection;202;0;223;0
WireConnection;202;1;222;0
WireConnection;173;0;202;0
WireConnection;120;0;173;0
WireConnection;120;1;1;4
WireConnection;1;0;207;0
WireConnection;181;0;1;0
WireConnection;181;1;1;4
WireConnection;252;0;10;0
WireConnection;252;1;181;0
ASEEND*/
//CHKSM=8C747E9EDBF8C21CDFEFDD3A2933563DE011890A