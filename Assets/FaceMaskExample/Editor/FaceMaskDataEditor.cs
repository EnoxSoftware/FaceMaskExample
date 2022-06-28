using UnityEditor;
using UnityEngine;

namespace FaceMaskExample
{
    [CustomEditor(typeof(FaceMaskData))]
    public class FaceMaskDataEditor : Editor
    {
        SerializedProperty image;
        SerializedProperty isDynamicMode;
        SerializedProperty enableColorCorrection;
        SerializedProperty faceRect;
        SerializedProperty landmarkPoints;

        bool isDrag = false;
        int currentPointID = -1;

        private void OnEnable()
        {
            image = serializedObject.FindProperty("_image");
            isDynamicMode = serializedObject.FindProperty("isDynamicMode");
            enableColorCorrection = serializedObject.FindProperty("enableColorCorrection");
            faceRect = serializedObject.FindProperty("_faceRect");
            landmarkPoints = serializedObject.FindProperty("_landmarkPoints");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Texture2D tex = image.objectReferenceValue as Texture2D;

            // Draw image.
            if (tex != null)
            {
                GUILayout.Box(GUIContent.none, GUILayout.Width(tex.width), GUILayout.Height(tex.height));
                Rect imageRect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(imageRect, tex);

                if (!isDynamicMode.boolValue)
                {
                    // Draw face rect.
                    DrawFaceRect(imageRect, faceRect.rectValue, Color.red);

                    // Draw landmark points.
                    DrawFaceLandmark(imageRect, landmarkPoints, Color.green, Color.blue);

                    // Update mouse cursor.
                    for (int i = 0; i < landmarkPoints.arraySize; i++)
                    {
                        Vector2 pt = landmarkPoints.GetArrayElementAtIndex(i).vector2Value;
                        pt.x += imageRect.x;
                        pt.y += imageRect.y;
                        Rect r = new Rect(pt.x - 4, pt.y - 4, 8, 8);
                        EditorGUIUtility.AddCursorRect(r, MouseCursor.MoveArrow);
                    }

                    // Mouse event.
                    if (Event.current.type == EventType.MouseDown)
                    {
                        Rect mousePosRect = new Rect(Event.current.mousePosition.x - 4, Event.current.mousePosition.y - 4, 8, 8);
                        int id = GetPointID(imageRect, landmarkPoints, mousePosRect);
                        if (id >= 0)
                        {
                            isDrag = true;
                            currentPointID = id;
                        }

                        Repaint();
                    }

                    if (Event.current.type == EventType.MouseDrag)
                    {
                        if (isDrag && currentPointID >= 0)
                        {
                            Vector2 newPt = new Vector2(Event.current.mousePosition.x - imageRect.x, Event.current.mousePosition.y - imageRect.y);
                            newPt.x = Mathf.Clamp(newPt.x, 0, tex.width);
                            newPt.y = Mathf.Clamp(newPt.y, 0, tex.height);
                            landmarkPoints.GetArrayElementAtIndex(currentPointID).vector2Value = newPt;

                            if (!imageRect.Contains(Event.current.mousePosition))
                            {
                                isDrag = false;
                                currentPointID = -1;
                            }
                        }

                        Repaint();
                    }

                    if (Event.current.type == EventType.MouseUp)
                    {
                        if (isDrag && currentPointID >= 0)
                        {
                            Vector2 newPt = new Vector2(Event.current.mousePosition.x - imageRect.x, Event.current.mousePosition.y - imageRect.y);
                            newPt.x = Mathf.Clamp(newPt.x, 0, tex.width);
                            newPt.y = Mathf.Clamp(newPt.y, 0, tex.height);
                            landmarkPoints.GetArrayElementAtIndex(currentPointID).vector2Value = newPt;
                        }
                        isDrag = false;
                        currentPointID = -1;

                        Repaint();
                    }

                    if (currentPointID > -1 && currentPointID < landmarkPoints.arraySize)
                    {
                        Vector2 pt = landmarkPoints.GetArrayElementAtIndex(currentPointID).vector2Value;
                        pt.x += imageRect.x;
                        pt.y += imageRect.y;
                        Handles.color = Color.yellow;
                        Handles.DrawSolidDisc(pt, Vector3.forward, 3f);
                    }
                }
            }

            // Display input field.
            EditorGUILayout.PropertyField(image);
            EditorGUILayout.PropertyField(isDynamicMode);
            EditorGUILayout.PropertyField(enableColorCorrection);
            EditorGUILayout.PropertyField(faceRect);
            EditorGUILayout.PropertyField(landmarkPoints, true);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFaceRect(Rect imageRect, Rect faceRect, Color color)
        {
            faceRect.x += imageRect.x;
            faceRect.y += imageRect.y;
            Handles.color = color;
            Handles.DrawSolidRectangleWithOutline(faceRect, new Color(0, 0, 0, 0), Color.white);
        }

        private void DrawFaceLandmark(Rect imageRect, SerializedProperty landmarkPoints, Color lineColor, Color pointColor)
        {
            if (landmarkPoints.isArray && landmarkPoints.arraySize == 68)
            {

                Handles.color = lineColor;

                for (int i = 1; i <= 16; ++i)
                    DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(i).vector2Value, landmarkPoints.GetArrayElementAtIndex(i - 1).vector2Value);

                for (int i = 28; i <= 30; ++i)
                    DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(i).vector2Value, landmarkPoints.GetArrayElementAtIndex(i - 1).vector2Value);

                for (int i = 18; i <= 21; ++i)
                    DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(i).vector2Value, landmarkPoints.GetArrayElementAtIndex(i - 1).vector2Value);
                for (int i = 23; i <= 26; ++i)
                    DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(i).vector2Value, landmarkPoints.GetArrayElementAtIndex(i - 1).vector2Value);
                for (int i = 31; i <= 35; ++i)
                    DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(i).vector2Value, landmarkPoints.GetArrayElementAtIndex(i - 1).vector2Value);
                DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(30).vector2Value, landmarkPoints.GetArrayElementAtIndex(35).vector2Value);

                for (int i = 37; i <= 41; ++i)
                    DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(i).vector2Value, landmarkPoints.GetArrayElementAtIndex(i - 1).vector2Value);
                DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(36).vector2Value, landmarkPoints.GetArrayElementAtIndex(41).vector2Value);

                for (int i = 43; i <= 47; ++i)
                    DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(i).vector2Value, landmarkPoints.GetArrayElementAtIndex(i - 1).vector2Value);
                DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(42).vector2Value, landmarkPoints.GetArrayElementAtIndex(47).vector2Value);

                for (int i = 49; i <= 59; ++i)
                    DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(i).vector2Value, landmarkPoints.GetArrayElementAtIndex(i - 1).vector2Value);
                DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(48).vector2Value, landmarkPoints.GetArrayElementAtIndex(59).vector2Value);

                for (int i = 61; i <= 67; ++i)
                    DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(i).vector2Value, landmarkPoints.GetArrayElementAtIndex(i - 1).vector2Value);
                DrawLine(imageRect, landmarkPoints.GetArrayElementAtIndex(60).vector2Value, landmarkPoints.GetArrayElementAtIndex(67).vector2Value);

                // Draw Points.
                Handles.color = pointColor;
                for (int i = 0; i < landmarkPoints.arraySize; i++)
                {
                    Vector2 pt = landmarkPoints.GetArrayElementAtIndex(i).vector2Value;
                    pt.x += imageRect.x;
                    pt.y += imageRect.y;
                    Handles.DrawSolidDisc(pt, Vector3.forward, 2f);
                }
            }
        }

        private void DrawLine(Rect imageRect, Vector2 pt1, Vector2 pt2)
        {
            pt1.x += imageRect.x;
            pt1.y += imageRect.y;
            pt2.x += imageRect.x;
            pt2.y += imageRect.y;
            Handles.DrawLine(pt1, pt2);
        }

        private int GetPointID(Rect imageRect, SerializedProperty landmarkPoints, Rect rect)
        {
            if (landmarkPoints.isArray && landmarkPoints.arraySize == 68)
            {
                for (int i = 0; i < landmarkPoints.arraySize; i++)
                {
                    Vector2 pt = landmarkPoints.GetArrayElementAtIndex(i).vector2Value;
                    pt.x += imageRect.x;
                    pt.y += imageRect.y;
                    if (rect.Contains(pt))
                        return i;
                }

            }
            return -1;
        }
    }
}