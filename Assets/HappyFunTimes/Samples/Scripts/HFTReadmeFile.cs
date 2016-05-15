﻿using UnityEngine;
using System;
using System.Collections;

namespace HappyFunTimes {

    [ExecuteInEditMode]
    public class HFTReadmeFile : MonoBehaviour {

        public TextAsset file;

        #if UNITY_EDITOR
        private Rect m_windowRect;

        const int maxWidth = 640;
        const int maxHeight = 480;
        const int border = 20;

        Vector3 ScreenToWorld(float x, float y) {
            Camera camera = Camera.current;
            Vector3 s = camera.WorldToScreenPoint(transform.position);
            return camera.ScreenToWorldPoint(new Vector3(x, camera.pixelHeight - y, s.z));
        }

        Rect ScreenRect(int x, int y, int w, int h) {
            Vector3 tl = ScreenToWorld(x, y);
            Vector3 br = ScreenToWorld(x + w, y + h);
            return new Rect(tl.x, tl.y, br.x - tl.x, br.y - tl.y);
        }

        string GetText() {
            return file.text.Replace("\n\n", "--EOL--").Replace("\n", " ").Replace("--EOL--", "\n\n");
        }

        void OnDrawGizmos()
        {
            if (file == null || !enabled)
            {
                return;
            }

            GUIContent textContent = new GUIContent(GetText());
            GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Label"));

            Camera camera = Camera.current;
            int width = Mathf.Min(maxWidth, camera.pixelWidth - 20);
            int height = Mathf.Min(maxHeight, camera.pixelHeight - 20);

            int x = (camera.pixelWidth - width) / 2;
            int y = (camera.pixelHeight - height) / 2;
            Rect r = ScreenRect(x, y, width, height);

            UnityEditor.Handles.DrawSolidRectangleWithOutline(r, new Color(0f, 0f, 0f, 0.3f), Color.white);
            style.normal.textColor = Color.white;
            style.wordWrap = true;
            style.fixedWidth = width - border * 2;
            style.fontSize = 12;
            UnityEditor.Handles.Label(ScreenToWorld(x + border, y + border), textContent, style);
        }

        void OnGUI()
        {
            if (!Application.isEditor || !enabled)
            {
                return;
            }

            int width = Mathf.Min(maxWidth, Screen.width - 20);
            int height = Mathf.Min(maxHeight, Screen.height - 20);
            m_windowRect = new Rect(
                (Screen.width - width) / 2,
                (Screen.height - height) / 2,
                width,
                height);

            m_windowRect = GUI.Window(0, m_windowRect, WindowFunc, file.name);
        }

        void WindowFunc(int windowID)
        {

            if (file != null)
            {
                Rect l = new Rect(
                    border,
                    border,
                    m_windowRect.width - border * 2,
                    m_windowRect.height - border * 2);
                GUI.Label(l, GetText());
            }
        }
        #endif
    }
}

