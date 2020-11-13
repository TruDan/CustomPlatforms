using IPA.Utilities;

using System;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;


namespace CustomFloorPlugin
{


    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Too old to change")]
    public class TubeLight : MonoBehaviour, NotifyOnEnableOrDisable
    {
        public enum LightsID
        {
            Static = 0,
            BackLights = 1,
            BigRingLights = 2,
            LeftLasers = 3,
            RightLasers = 4,
            TrackAndBottom = 5,
            Unused5 = 6,
            Unused6 = 7,
            Unused7 = 8,
            RingsRotationEffect = 9,
            RingsStepEffect = 10,
            Unused10 = 11,
            Unused11 = 12,
            RingSpeedLeft = 13,
            RingSpeedRight = 14,
            Unused14 = 15,
            Unused15 = 16
        }

        public float width = 0.5f;
        public float length = 1f;
        [Range(0, 1)]
        public float center = 0.5f;
        public Color color = Color.white;
        public LightsID lightsID = LightsID.Static;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Called by Unity")]
        private void OnDrawGizmos()
        {
            Gizmos.color = color;
            Gizmos.matrix = transform.localToWorldMatrix;
            Vector3 cubeCenter = Vector3.up * (0.5f - center) * length;
            Gizmos.DrawCube(cubeCenter, new Vector3(2 * width, length, 2 * width));
        }


        private static TubeBloomPrePassLight _Prefab;
        internal static TubeBloomPrePassLight Prefab
        {
            get
            {
                if (_Prefab == null)
                {
                    try
                    {
                        _Prefab =
                            SceneManager
                            .GetSceneByName("MenuEnvironment")
                            .GetRootGameObjects()
                            .First<GameObject>(x => x.name == "MenuEnvironment")
                            .transform
                            .Find("DefaultEnvironment/Laser (1)")
                            .GetComponent<TubeBloomPrePassLight>();
                    }
                    catch (InvalidOperationException)
                    {
                        _Prefab =
                            SceneManager
                            .GetSceneByName("MenuEnvironment")
                            .GetRootGameObjects()
                            .First<GameObject>(x => x.name == "RootContainer")
                            .transform
                            .Find("MenuEnvironment/DefaultEnvironment/Laser (1)")
                            .GetComponent<TubeBloomPrePassLight>();
                    }
                }
                return _Prefab;
            }
        }
        private TubeBloomPrePassLight tubeBloomLight;
        private GameObject iHeartBeatSaber;


        internal void GameAwake(LightWithIdManager lightManager)
        {
            GetComponent<MeshRenderer>().enabled = false;
            if (GetComponent<MeshFilter>().mesh.vertexCount == 0)
            {
                tubeBloomLight = Instantiate(Prefab);
                tubeBloomLight.transform.parent = transform;
                PlatformManager.SpawnedObjects.Add(tubeBloomLight.gameObject);

                tubeBloomLight.transform.localRotation = Quaternion.identity;
                tubeBloomLight.transform.localPosition = Vector3.zero;
                tubeBloomLight.transform.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);

                tubeBloomLight.gameObject.SetActive(false);

                TubeBloomPrePassLightWithId lightWithId = tubeBloomLight.GetComponent<TubeBloomPrePassLightWithId>();
                if (lightWithId)
                {
                    lightWithId.SetField("_tubeBloomPrePassLight", tubeBloomLight);
                    ((LightWithIdMonoBehaviour)lightWithId).SetField("_ID", (int)lightsID);
                    ((LightWithIdMonoBehaviour)lightWithId).SetField("_lightManager", lightManager);
                }

                tubeBloomLight.SetField("_width", width * 2);
                tubeBloomLight.SetField("_length", length);
                tubeBloomLight.SetField("_center", center);
                tubeBloomLight.SetField("_transform", tubeBloomLight.transform);
                tubeBloomLight.SetField("_maxAlpha", 0.1f);
                tubeBloomLight.SetField("_bloomFogIntensityMultiplier", 0.1f);

                var parabox = tubeBloomLight.GetComponentInChildren<ParametricBoxController>();
                tubeBloomLight.SetField("_parametricBoxController", parabox);

                var parasprite = tubeBloomLight.GetComponentInChildren<Parametric3SliceSpriteController>();
                tubeBloomLight.SetField("_dynamic3SliceSprite", parasprite);
                parasprite.Init();
                parasprite.GetComponent<MeshRenderer>().enabled = false;

                SetColorToDefault();
                tubeBloomLight.gameObject.SetActive(true);

            }
            else if (PlatformManager.Heart != null)
            {
                // swap for <3
                iHeartBeatSaber = Instantiate(PlatformManager.Heart);
                PlatformManager.SpawnedObjects.Add(iHeartBeatSaber);
                iHeartBeatSaber.transform.parent = transform;
                iHeartBeatSaber.transform.position = transform.position;
                iHeartBeatSaber.transform.localScale = Vector3.one;
                iHeartBeatSaber.transform.rotation = transform.rotation;
                InstancedMaterialLightWithId lightWithId = iHeartBeatSaber.GetComponent<InstancedMaterialLightWithId>();
                ((LightWithIdMonoBehaviour)lightWithId).SetField("_ID", (int)lightsID);
                ((LightWithIdMonoBehaviour)lightWithId).SetField("_lightManager", lightManager);
                lightWithId.SetField("_minAlpha", 0f);
                iHeartBeatSaber.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
                iHeartBeatSaber.SetActive(true);
            }
        }

        private void SetColorToDefault()
        {
            tubeBloomLight.color = color * 0.9f;
            tubeBloomLight.Refresh();
        }

        void NotifyOnEnableOrDisable.PlatformEnabled()
        {
            PlatformManager.SpawnQueue += GameAwake;
        }

        void NotifyOnEnableOrDisable.PlatformDisabled()
        {
            PlatformManager.SpawnQueue -= GameAwake;
        }
    }
}