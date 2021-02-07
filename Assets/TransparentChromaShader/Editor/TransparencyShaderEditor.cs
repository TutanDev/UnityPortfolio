using UnityEditor;
using UnityEngine;

public class TransparencyShaderEditor : ShaderGUI
{
    MaterialEditor m_MaterialEditor;

    MaterialProperty videoMap = null;

    MaterialProperty removeColor = null;
    float hue, saturation, value;

    MaterialProperty hueTolerace = null;
    float htol;

    MaterialProperty hueblend = null;
    float hblend;

    MaterialProperty SatValRanges = null;
    float minSat, maxSat, minVal, maxVal;

    Texture2D hueText = new Texture2D(100, 1);
    Texture2D satText = new Texture2D(100, 1);
    Texture2D valText = new Texture2D(100, 1);

    void FindProperties(MaterialProperty[] properties)
    {
        videoMap = FindProperty("_MainTex", properties);
        removeColor = FindProperty("_RemoveColor", properties);
        hueTolerace = FindProperty("_HueTolerance", properties);
        hueblend = FindProperty("_HueBlend", properties);
        SatValRanges = FindProperty("_SaturationValueRanges", properties, false);
    }

    void ReadValues(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_MaterialEditor = materialEditor;
        Material material = materialEditor.target as Material;

        FindProperties(properties);
        htol = hueTolerace.floatValue * 100;
        hblend = hueblend.floatValue * 100;
        Color.RGBToHSV(removeColor.colorValue, out hue, out saturation, out value);
        minSat = Mathf.Clamp(SatValRanges.vectorValue.x, 0, saturation - 0.01f);
        maxSat = Mathf.Clamp(SatValRanges.vectorValue.y, saturation + 0.01f, 1);
        minVal = Mathf.Clamp(SatValRanges.vectorValue.z, 0, value - 0.01f);
        maxVal = Mathf.Clamp(SatValRanges.vectorValue.w, value + 0.01f, 1);
    }

    void Writevalues()
    {
        hueTolerace.floatValue = htol * 0.01f;
        hueblend.floatValue = hblend * 0.01f;
        SatValRanges.vectorValue = new Vector4(minSat, maxSat, minVal, maxVal);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        ReadValues(materialEditor, properties);

        using(EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
        {
            EditorGUIUtility.labelWidth = 0;
            m_MaterialEditor.TextureProperty(videoMap, "Video Texture");

            EditorGUILayout.Space(10);
            DrawColorToRemove();

            EditorGUI.indentLevel++;
            EditorGUILayout.Space(10);
            DrawHueControls();

            EditorGUILayout.Space();
            DrawSatValRanges();
            EditorGUI.indentLevel++;
            if (check.changed)
            {
                Writevalues();
            }
        }
    }



    private void DrawColorToRemove()
    {
        EditorGUIUtility.labelWidth = 110;
        m_MaterialEditor.ColorProperty(removeColor, "Color to Remove");
        EditorGUIUtility.labelWidth = 0;
        //DrawHSVDebug();
    }

    private void DrawHueControls()
    {
        HueGradient(new Rect(32, 150, EditorGUIUtility.currentViewWidth - 36, 12));
        var blendRect = new Rect(32, 170, EditorGUIUtility.currentViewWidth - 36, 12);
        HueGradient(blendRect);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("", GUILayout.Width(5));
            GUILayout.Label("Hue Range:", GUILayout.Width(70));

            EditorGUIUtility.labelWidth = 75;
            htol = Mathf.Clamp(EditorGUILayout.FloatField("Tolerance", htol), 0, 20);

            EditorGUIUtility.labelWidth = 50;
            hblend = Mathf.Clamp(EditorGUILayout.FloatField("Blend", hblend), 0, 10);
            EditorGUIUtility.labelWidth = 0;
        }

        EditorGUILayout.Space(1);
        HueRangeSlider(hueTolerace.floatValue);
        HueRangeSlider(hueTolerace.floatValue + hueblend.floatValue);

        GUILayout.BeginArea(blendRect);
        float width = blendRect.width * htol * 0.02f;
        Rect blackRect = new Rect(blendRect.width * hue - width *0.5f + Mathf.Cos(Mathf.PI * hue) * 6, 1, width, 10);
        EditorGUI.DrawRect(blackRect, new Color(0.1f, 0.1f, 0.1f, 0.9f));
        GUILayout.EndArea();
    }
    void HueRangeSlider(float range)
    {
        float min = hue - range;
        float max = hue + range;
        EditorGUILayout.MinMaxSlider(ref min, ref max, 0, 1);
    }
    void HueGradient(Rect rect)
    {
        Color color;
        for (int i = 0; i < hueText.width; i++)
        {
            color = Color.HSVToRGB((float)i / 99, 1, 0.8f);
            hueText.SetPixel(i, 0, color);
        }
        hueText.Apply();
        GUI.DrawTexture(rect, hueText);
    }
    

    private void DrawSatValRanges()
    {
        SatGradient(new Rect(32, 220, EditorGUIUtility.currentViewWidth - 36, 14));
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("", GUILayout.Width(5));
            GUILayout.Label("Saturation Range:", GUILayout.Width(110));

            EditorGUIUtility.labelWidth = 35;
            minSat = EditorGUILayout.FloatField("Min", minSat);

            EditorGUIUtility.labelWidth = 50;
            maxSat = EditorGUILayout.FloatField("Max", maxSat);
        }
        EditorGUILayout.Space(1);
        EditorGUILayout.MinMaxSlider(ref minSat, ref maxSat, 0, 1);

        EditorGUILayout.Space();

        ValGradient(new Rect(32, 271, EditorGUIUtility.currentViewWidth - 36, 14));
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("", GUILayout.Width(5));
            GUILayout.Label("Value Range:", GUILayout.Width(110));

            EditorGUIUtility.labelWidth = 35;
            minVal = EditorGUILayout.FloatField("Min", minVal);

            EditorGUIUtility.labelWidth = 50;
            maxVal = EditorGUILayout.FloatField("Max", maxVal);
            EditorGUIUtility.labelWidth = 0;
        }
        EditorGUILayout.Space(1);
        EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, 0, 1);
    } 
    void SatGradient(Rect rect)
    {
        Color color;
        for (int i = 0; i < hueText.width; i++)
        {
            color = Color.HSVToRGB(hue, (float)i / 99, 0.9f);
            color.a = 1;
            satText.SetPixel(i, 0, color);
        }
        satText.Apply();
        GUI.DrawTexture(rect, satText);
    }
    void ValGradient(Rect rect)
    {
        Color color;
        for (int i = 0; i < hueText.width; i++)
        {
            color = Color.HSVToRGB(hue, 1, (float)i / 99);
            color.a = 1;
            valText.SetPixel(i, 0, color);
        }
        valText.Apply();
        GUI.DrawTexture(rect, valText);
    }








    void DrawAdvancedOptions()
    {
        EditorGUILayout.Space();
        GUILayout.Label("Advanced Options", EditorStyles.boldLabel);
        m_MaterialEditor.EnableInstancingField();
        m_MaterialEditor.RenderQueueField();
        m_MaterialEditor.DoubleSidedGIField();
    }
}
