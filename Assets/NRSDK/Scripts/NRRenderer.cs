﻿/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal
{
    using AOT;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary>
    /// NRNativeRender operate rendering-related things, provides the feature of optimized rendering
    /// and low latency. </summary>
    [ScriptOrder(NativeConstants.NRRENDER_ORDER)]
    public class NRRenderer : MonoBehaviour
    {
        /// <summary> Renders the event delegate described by eventID. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        private delegate void RenderEventDelegate(int eventID);
        /// <summary> Handle of the render thread. </summary>
        private static RenderEventDelegate RenderThreadHandle = new RenderEventDelegate(RunOnRenderThread);
        /// <summary> The render thread handle pointer. </summary>
        private static IntPtr RenderThreadHandlePtr = Marshal.GetFunctionPointerForDelegate(RenderThreadHandle);

        private const int SETRENDERTEXTUREEVENT = 0x0001;
        private const int STARTNATIVERENDEREVENT = 0x0002;
        private const int RESUMENATIVERENDEREVENT = 0x0003;
        private const int PAUSENATIVERENDEREVENT = 0x0004;
        private const int STOPNATIVERENDEREVENT = 0x0005;

        public enum Eyes
        {
            /// <summary> Left Display. </summary>
            Left = 0,
            /// <summary> Right Display. </summary>
            Right = 1,
            Count = 2
        }

        public Camera leftCamera;
        public Camera rightCamera;
        /// <summary> Gets or sets the native renderring. </summary>
        /// <value> The m native renderring. </value>
        private static NativeRenderring m_NativeRenderring;
        static NativeRenderring NativeRenderring
        {
            get
            {
                if (NRSessionManager.Instance.NativeAPI != null)
                {
                    m_NativeRenderring = NRSessionManager.Instance.NativeAPI.NativeRenderring;
                }
                else if (m_NativeRenderring == null)
                {
                    m_NativeRenderring = new NativeRenderring();
                }

                return m_NativeRenderring;
            }
            set
            {
                m_NativeRenderring = value;
            }
        }

        /// <summary> The scale factor. </summary>
        public static float ScaleFactor = 1f;
        private const float m_DefaultFocusDistance = 1.4f;
        private float m_FocusDistance = 1.4f;

        private static int _TextureBufferSize = 4;
        /// <summary> Number of eye textures. </summary>
        private static int EyeTextureCount = _TextureBufferSize * (int)Eyes.Count;
        /// <summary> The eye textures. </summary>
        private RenderTexture[] eyeTextures;
        /// <summary> Dictionary of rights. </summary>
        private Dictionary<RenderTexture, IntPtr> m_RTDict = new Dictionary<RenderTexture, IntPtr>();

        /// <summary> Values that represent renderer states. </summary>
        public enum RendererState
        {
            UnInitialized,
            Initialized,
            Running,
            Paused,
            Destroyed
        }

        private bool m_IsTrackingLost = false;
        private RenderTexture m_HijackRenderTexture;

        /// <summary> The current state. </summary>
        private RendererState m_CurrentState = RendererState.UnInitialized;
        /// <summary> Gets the current state. </summary>
        /// <value> The current state. </value>
        public RendererState CurrentState
        {
            get
            {
                return m_CurrentState;
            }
        }

        public NRTrackingModeChangedListener TrackingLostListener
        {
            get
            {
                return NRSessionManager.Instance.TrackingLostListener;
            }
        }

#if !UNITY_EDITOR
        private int currentEyeTextureIdx = 0;
        private int nextEyeTextureIdx = 0;
#endif

        /// <summary> Gets a value indicating whether this object is linear color space. </summary>
        /// <value> True if this object is linear color space, false if not. </value>
        public static bool isLinearColorSpace
        {
            get
            {
                return QualitySettings.activeColorSpace == ColorSpace.Linear;
            }
        }

        /// <summary> Initialize the render pipleline. </summary>
        /// <param name="leftcamera">  Left Eye.</param>
        /// <param name="rightcamera"> Right Eye.</param>
        ///
        /// ### <param name="poseprovider"> provide the pose of camera every frame.</param>
        public void Initialize(Camera leftcamera, Camera rightcamera)
        {
            NRDebugger.Info("[NRRender] Initialize");
            if (m_CurrentState != RendererState.UnInitialized)
            {
                return;
            }

            leftCamera = leftcamera;
            rightCamera = rightcamera;

#if !UNITY_EDITOR
            leftCamera.depthTextureMode = DepthTextureMode.None;
            rightCamera.depthTextureMode = DepthTextureMode.None;
            leftCamera.rect = new Rect(0, 0, 1, 1);
            rightCamera.rect = new Rect(0, 0, 1, 1);
            leftCamera.enabled = false;
            rightCamera.enabled = false;
            m_CurrentState = RendererState.Initialized;
            StartCoroutine(StartUp());
#endif

            if (TrackingLostListener != null)
            {
                TrackingLostListener.OnTrackStateChanged += OnTrackStateChanged;
            }
        }

        private void OnTrackStateChanged(bool onSwitchingMode, RenderTexture rt)
        {
            if (onSwitchingMode)
            {
                m_IsTrackingLost = true;
                m_HijackRenderTexture = rt;
            }
            else
            {
                m_IsTrackingLost = false;
                m_HijackRenderTexture = null;
            }
        }

        /// <summary> Prepares this object for use. </summary>
        /// <returns> An IEnumerator. </returns>
        private IEnumerator StartUp()
        {
            var virtualDisplay = GameObject.FindObjectOfType<NRVirtualDisplayer>();
            while (virtualDisplay == null || !virtualDisplay.Subsystem.running)
            {
                NRDebugger.Info("[NRRender] Wait virtual display ready...");
                yield return new WaitForEndOfFrame();
                if (virtualDisplay == null)
                {
                    virtualDisplay = GameObject.FindObjectOfType<NRVirtualDisplayer>();
                }
            }

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            NRDebugger.Info("[NRRender] StartUp");
            CreateRenderTextures();
#if !UNITY_EDITOR
            NativeRenderring.Create();
            StartCoroutine(RenderCoroutine());
#endif
            m_CurrentState = RendererState.Running;
            GL.IssuePluginEvent(RenderThreadHandlePtr, STARTNATIVERENDEREVENT);
        }

        /// <summary> Pause render. </summary>
        public void Pause()
        {
            NRDebugger.Info("[NRRender] Pause");
            if (m_CurrentState != RendererState.Running)
            {
                return;
            }
            m_CurrentState = RendererState.Paused;
            GL.IssuePluginEvent(RenderThreadHandlePtr, PAUSENATIVERENDEREVENT);
        }

        /// <summary> Resume render. </summary>
        public void Resume()
        {
            Invoke("DelayResume", 0.3f);
        }

        /// <summary> Delay resume. </summary>
        private void DelayResume()
        {
            NRDebugger.Info("[NRRender] Resume");
            if (m_CurrentState != RendererState.Paused)
            {
                return;
            }
            m_CurrentState = RendererState.Running;
            GL.IssuePluginEvent(RenderThreadHandlePtr, RESUMENATIVERENDEREVENT);
        }

#if !UNITY_EDITOR
        void Update()
        {
            if (m_CurrentState == RendererState.Running && !m_IsTrackingLost)
            {
                leftCamera.targetTexture = eyeTextures[currentEyeTextureIdx];
                rightCamera.targetTexture = eyeTextures[currentEyeTextureIdx + 1];
                currentEyeTextureIdx = nextEyeTextureIdx;
                nextEyeTextureIdx = (nextEyeTextureIdx + 2) % EyeTextureCount;
                leftCamera.enabled = true;
                rightCamera.enabled = true;
            }
            else if(m_IsTrackingLost) 
            {
                leftCamera.enabled = false;
                rightCamera.enabled = false;
            }
        }
#endif

        /// <summary> Generates a render texture. </summary>
        /// <param name="width">  The width.</param>
        /// <param name="height"> The height.</param>
        /// <returns> The render texture. </returns>
        private RenderTexture GenRenderTexture(int width, int height)
        {
            return UnityExtendedUtility.CreateRenderTexture((int)(width * ScaleFactor), (int)(height * ScaleFactor), 24, RenderTextureFormat.Default);
        }

        /// <summary> Creates render textures. </summary>
        private void CreateRenderTextures()
        {
            var config = NRSessionManager.Instance.NRSessionBehaviour?.SessionConfig;

            if (config != null && config.UseMultiThread)
            {
                _TextureBufferSize = 5;
            }
            else
            {
                _TextureBufferSize = 4;
            }
            NRDebugger.Info("[NRRender] Texture buffer size:{0}", _TextureBufferSize);
            EyeTextureCount = _TextureBufferSize * (int)Eyes.Count;
            eyeTextures = new RenderTexture[EyeTextureCount];

            var resolution = NRDevice.Subsystem.GetDeviceResolution(NativeDevice.LEFT_DISPLAY);
            NRDebugger.Info("[CreateRenderTextures] Resolution :" + resolution.ToString());

            for (int i = 0; i < EyeTextureCount; i++)
            {
                eyeTextures[i] = GenRenderTexture(resolution.width, resolution.height);
                m_RTDict.Add(eyeTextures[i], eyeTextures[i].GetNativeTexturePtr());
            }
        }

        /// <summary> Renders the coroutine. </summary>
        /// <returns> An IEnumerator. </returns>
        private IEnumerator RenderCoroutine()
        {
            WaitForEndOfFrame delay = new WaitForEndOfFrame();
            yield return delay;

            while (true)
            {
                yield return delay;

                if (m_CurrentState != RendererState.Running)
                {
                    continue;
                }

                NativeMat4f apiPose;
                Pose unityPose = NRFrame.HeadPose;
                ConversionUtility.UnityPoseToApiPose(unityPose, out apiPose);
                FrameInfo info = new FrameInfo(IntPtr.Zero, IntPtr.Zero, apiPose, new Vector3(0, 0, -m_FocusDistance),
                       Vector3.forward, NRFrame.CurrentPoseTimeStamp, m_FrameChangedType, NRTextureType.NR_TEXTURE_2D);
                if (!m_IsTrackingLost)
                {
                    IntPtr left_target, right_target;
                    if (!m_RTDict.TryGetValue(leftCamera.targetTexture, out left_target)) continue;
                    if (!m_RTDict.TryGetValue(rightCamera.targetTexture, out right_target)) continue;
                    info.leftTex = left_target;
                    info.rightTex = right_target;
                }
                else
                {
                    info.leftTex = m_HijackRenderTexture.GetNativeTexturePtr();
                    info.rightTex = m_HijackRenderTexture.GetNativeTexturePtr();
                }
                SetRenderFrameInfo(info);
                // reset focuse distance and frame changed type to default value every frame.
                m_FocusDistance = m_DefaultFocusDistance;
                m_FrameChangedType = NRFrameFlags.NR_FRAME_CHANGED_NONE;
            }
        }

        private NRFrameFlags m_FrameChangedType = NRFrameFlags.NR_FRAME_CHANGED_NONE;
        /// <summary> Sets the focus plane for render thread. </summary>
        /// <param name="distance"> The distance from plane to center camera.</param>
        public void SetFocusDistance(float distance)
        {
            m_FocusDistance = distance;
            m_FrameChangedType = NRFrameFlags.NR_FRAME_CHANGED_FOCUS_PLANE;
        }

        /// <summary> Executes the 'on render thread' operation. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        [MonoPInvokeCallback(typeof(RenderEventDelegate))]
        private static void RunOnRenderThread(int eventID)
        {
            if (eventID == STARTNATIVERENDEREVENT)
            {
                NativeRenderring?.Start();
            }
            else if (eventID == RESUMENATIVERENDEREVENT)
            {
                NativeRenderring?.Resume();
            }
            else if (eventID == PAUSENATIVERENDEREVENT)
            {
                NativeRenderring?.Pause();
            }
            else if (eventID == STOPNATIVERENDEREVENT)
            {
                NativeRenderring?.Destroy();
                NativeRenderring = null;
                NRDevice.Instance.Destroy();
            }
            else if (eventID == SETRENDERTEXTUREEVENT)
            {
                NativeRenderring?.DoExtendedRenderring();
            }
        }

        /// <summary> Sets render frame information. </summary>
        /// <param name="frame"> The frame.</param>
        private static void SetRenderFrameInfo(FrameInfo frame)
        {
            NativeRenderring.WriteFrameData(frame);
            GL.IssuePluginEvent(RenderThreadHandlePtr, SETRENDERTEXTUREEVENT);
        }

        public void Destroy()
        {
            if (m_CurrentState == RendererState.Destroyed)
            {
                return;
            }
            m_CurrentState = RendererState.Destroyed;
            //GL.IssuePluginEvent(RenderThreadHandlePtr, STOPNATIVERENDEREVENT);

            NativeRenderring?.Destroy();
            NativeRenderring = null;
        }

        private void OnDestroy()
        {
            this.Destroy();
        }
    }
}
