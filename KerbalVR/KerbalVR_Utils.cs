using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace KerbalVR
{
    /// <summary>
    /// A class to contain general utility functions.
    /// </summary>
    public class Utils
    {
        public static Component GetOrAddComponent<T>(GameObject obj) where T : Component {
            Component c = obj.GetComponent<T>();
            if (c == null) {
                c = obj.AddComponent<T>();
            }
            return c;
        }

        public static void Log(object obj) {
            Debug.Log(Globals.LOG_PREFIX + obj);
        }

        public static void LogWarning(object obj) {
            Debug.LogWarning(Globals.LOG_PREFIX + obj);
        }

        public static void LogError(object obj) {
            Debug.LogError(Globals.LOG_PREFIX + obj);
        }

        public static bool Is64BitProcess {
            get { return (IntPtr.Size == 8); }
        }

        public static GameObject CreateGizmo() {
            GameObject gizmo = new GameObject("gizmo");
            gizmo.transform.localScale = Vector3.one;

            GameObject gizmoX = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(gizmoX.GetComponent<BoxCollider>());
            gizmoX.transform.SetParent(gizmo.transform);
            gizmoX.transform.localScale = new Vector3(.1f, .01f, .01f);
            gizmoX.transform.localPosition = new Vector3(.05f, 0f, 0f);
            gizmoX.GetComponent<MeshRenderer>().material.color = Color.red;
            gizmoX.layer = 20;

            GameObject gizmoY = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(gizmoY.GetComponent<BoxCollider>());
            gizmoY.transform.SetParent(gizmo.transform);
            gizmoY.transform.localScale = new Vector3(.01f, .1f, .01f);
            gizmoY.transform.localPosition = new Vector3(.0f, .05f, 0f);
            gizmoY.GetComponent<MeshRenderer>().material.color = Color.green;
            gizmoY.layer = 20;

            GameObject gizmoZ = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(gizmoZ.GetComponent<BoxCollider>());
            gizmoZ.transform.SetParent(gizmo.transform);
            gizmoZ.transform.localScale = new Vector3(.01f, .01f, .1f);
            gizmoZ.transform.localPosition = new Vector3(.0f, 0f, .05f);
            gizmoZ.GetComponent<MeshRenderer>().material.color = Color.blue;
            gizmoZ.layer = 20;

            GameObject gizmoPivot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            UnityEngine.Object.Destroy(gizmoPivot.GetComponent<SphereCollider>());
            gizmoPivot.transform.SetParent(gizmo.transform);
            gizmoPivot.transform.localScale = new Vector3(.02f, .02f, .02f);
            gizmoPivot.transform.localPosition = Vector3.zero;
            gizmoPivot.GetComponent<MeshRenderer>().material.color = Color.gray;
            gizmoPivot.layer = 20;

            return gizmo;
        }

        public static GameObject CreateGizmoAtPosition(Transform location) {
            return CreateGizmoAtPosition(location.position, location.rotation);
        }

        public static GameObject CreateGizmoAtPosition(Vector3 position) {
            return CreateGizmoAtPosition(position, Quaternion.identity);
        }

        public static GameObject CreateGizmoAtPosition(Vector3 position, Quaternion rotation) {
            GameObject gizmo = CreateGizmo();
            gizmo.transform.position = position;
            gizmo.transform.rotation = rotation;
            return gizmo;
        }

        public static float CalculatePredictedSecondsToPhotons() {
            float secondsSinceLastVsync = 0f;
            ulong frameCounter = 0;
            OpenVR.System.GetTimeSinceLastVsync(ref secondsSinceLastVsync, ref frameCounter);

            float displayFrequency = GetFloatTrackedDeviceProperty(ETrackedDeviceProperty.Prop_DisplayFrequency_Float);
            float vsyncToPhotons = GetFloatTrackedDeviceProperty(ETrackedDeviceProperty.Prop_SecondsFromVsyncToPhotons_Float);
            float frameDuration = 1f / displayFrequency;

            return frameDuration - secondsSinceLastVsync + vsyncToPhotons;
        }

        public static float GetFloatTrackedDeviceProperty(ETrackedDeviceProperty property, uint device = OpenVR.k_unTrackedDeviceIndex_Hmd) {
            ETrackedPropertyError propertyError = ETrackedPropertyError.TrackedProp_Success;
            float value = OpenVR.System.GetFloatTrackedDeviceProperty(device, property, ref propertyError);
            if (propertyError != ETrackedPropertyError.TrackedProp_Success) {
                throw new Exception("Failed to obtain tracked device property \"" +
                    property + "\", error: (" + (int)propertyError + ") " + propertyError.ToString());
            }
            return value;
        }

        public static void PrintAllCameras() {
            Utils.Log("Scene: " + HighLogic.LoadedScene);
            for (int i = 0; i < Camera.allCamerasCount; i++) {
                string logMsg = "Camera: " + Camera.allCameras[i].name + ", depth = " + Camera.allCameras[i].depth + ", mask = [";
                int[] cullingMaskLayers = Int32MaskToArray(Camera.allCameras[i].cullingMask);
                string[] cullingMaskLayersStr = new string[cullingMaskLayers.Length];
                for (int j = 0; j < cullingMaskLayers.Length; j++) {
                    cullingMaskLayersStr[j] = cullingMaskLayers[j].ToString();
                }
                logMsg += String.Join(",", cullingMaskLayersStr);
                logMsg += "], clip = (" + Camera.allCameras[i].nearClipPlane.ToString("F3");
                logMsg += "," + Camera.allCameras[i].farClipPlane.ToString("F3") + ")";
                Utils.Log(logMsg);
            }
        }

        public static void PrintGameObject(GameObject go) {
            Log("GameObject: " + go.name + " (layer: " + go.layer + ")");
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++) {
                Log("Component: " + components[i].ToString());
            }
        }

        public static void PrintGameObjectTree(GameObject go) {
            PrintGameObject(go);
            for (int i = 0; i < go.transform.childCount; i++) {
                Log(go.name + " child " + i);
                PrintGameObjectTree(go.transform.GetChild(i).gameObject);
            }
        }

        public static void PrintDebug() {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

            foreach (GameObject GO in allObjects) {
                if (GO.activeInHierarchy) {
                    Collider GO_coll = GO.GetComponent<Collider>();

                    if (GO_coll != null) {
                        Utils.Log("GO.name = " + GO.name);
                        Utils.Log("GO.layer = " + GO.layer);
                        Utils.Log("GO.collider = " + GO_coll);
                    }
                }
                
            }
        }

        public static void PrintAllLayers() {
            for (int i = 0; i < 32; i++) {
                Utils.Log("Layer " + i + ": " + LayerMask.LayerToName(i));
            }
        }

        public static int[] Int32MaskToArray(int mask) {
            List<int> maskBits = new List<int>(32);
            for (int i = 0; i < 32; i++) {
                int checkMask = 1 << i;
                if ((mask & checkMask) > 0) {
                    maskBits.Add(i);
                }
            }
            return maskBits.ToArray();
        }

    } // class Utils
} // namespace KerbalVR
