using UnityEditor;
using UnityEngine;

namespace Klak.TestTools {

[CustomEditor(typeof(ImageSource))]
sealed class ImageSourceEditor : Editor
{
    static class Labels
    {
        public static Label Asset = "Asset";
        public static Label DeviceName = "Device Name";
        public static Label FrameRate = "Frame Rate";
        public static Label Resolution = "Resolution";
        public static Label FullScreenResolution = "Full Screen Resolution";
        public static Label Select = "Select";
        public static Label URL = "URL";
    }




    AutoProperty _webcamName;
    AutoProperty _webcamResolution;
    AutoProperty _webcamFrameRate;
    AutoProperty _fullScreenResolution;
    AutoProperty _outputTexture;
    AutoProperty _outputResolution;

    void OnEnable() => AutoProperty.Scan(this);

    void ChangeWebcam(string name)
    {
        serializedObject.Update();
        _webcamName.Target.stringValue = name;
        serializedObject.ApplyModifiedProperties();
    }

    void ShowDeviceSelector(Rect rect)
    {
        var menu = new GenericMenu();

        foreach (var device in WebCamTexture.devices)
            menu.AddItem(new GUIContent(device.name), false,
                         () => ChangeWebcam(device.name));

        menu.DropDown(rect);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_webcamName, Labels.DeviceName);
            var rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(60));
            if (EditorGUI.DropdownButton(rect, Labels.Select, FocusType.Keyboard))
            {
                ShowDeviceSelector(rect);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(_webcamFrameRate, Labels.FrameRate);
            EditorGUILayout.PropertyField(_webcamResolution, Labels.Resolution);
        EditorGUI.indentLevel--;
       EditorGUILayout.PropertyField(_fullScreenResolution, Labels.FullScreenResolution);

       if (_fullScreenResolution.Target.boolValue == false)
       {
           EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_outputTexture);
       if (_outputTexture.Target.objectReferenceValue == null)
       {
           EditorGUILayout.PropertyField(_outputResolution);
       }
       }

       EditorGUI.indentLevel--;
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }
}

} // namespace Klak.TestTools