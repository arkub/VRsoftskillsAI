# Unity Project Settings Configuration

This file contains the essential ProjectSettings configurations for Quest 2 & Quest 3 development.

## Player Settings (ProjectSettings/ProjectSettings.asset)

### Android Platform Configuration
```
productName: VR Conversation Experience
packageName: com.yourcompany.vrconversation
bundleVersion: 1.0
bundleVersionCode: 1
targetSdkVersion: 30
minSdkVersion: 23
targetArchitectures: ARM64
```

### XR Settings
```
xrSupported: true
xrProviders:
  - Oculus
  - OpenXR
virtualRealitySupported: true
```

### Graphics Settings
```
graphicsJobs: true
graphicsAPI: 
  - Vulkan
  - OpenGLES3
colorSpace: Linear
lightmapEncodingQuality: Normal
lightmapCompressionQuality: Normal
```

### Audio Settings
```
defaultSpeakerMode: Stereo
sampleRate: 48000
dspBufferSize: 1024
virtualVoiceCount: 512
realVoiceCount: 32
spatializerPlugin: Oculus Spatializer
```

### Physics Settings
```
fixedTimestep: 0.02
maximumAllowedTimestep: 0.333333
solverIterations: 6
solverVelocityIterations: 1
bounceThreshold: 2
sleepThreshold: 0.005
defaultContactOffset: 0.01
```

### Quality Settings
```
levels:
  - name: Quest Performance
    pixelLightCount: 1
    textureQuality: 0
    anisotropicTextures: 1
    antiAliasing: 2
    softParticles: false
    realtimeReflectionProbes: false
    billboardsFaceCameraPosition: false
    maximumLODLevel: 0
    particleRaycastBudget: 16
    asyncUploadTimeSlice: 2
    asyncUploadBufferSize: 16
    asyncUploadPersistentBuffer: true
    resolutionScalingFixedDPIFactor: 1
    shadows: 1
    shadowResolution: 1
    shadowProjection: 1
    shadowCascades: 1
    shadowDistance: 20
    shadowNearPlaneOffset: 3
    shadowCascade2Split: 0.33333334
    shadowCascade4Split: [0.06666667, 0.2, 0.46666667]
    shadowmaskMode: 0
    blendWeights: 2
    vSyncCount: 0
    lodBias: 0.7
    maximumParticleSystems: 1000
```

## XR Management Settings

### Provider Configuration
```
providers:
  - Oculus XR Plugin:
      enabled: true
      renderMode: SinglePassInstanced
      sharedDepthBuffer: true
      dashSupport: true
      v2Signing: false
      foveatedRenderingMethod: EyeTracked
      
  - OpenXR Plugin:
      enabled: true
      renderMode: SinglePassInstanced
      depthSubmissionMode: Depth24Bit
```

## Build Settings

### Android Manifest Permissions
```xml
<uses-permission android:name="android.permission.RECORD_AUDIO" />
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="com.oculus.permission.HAND_TRACKING" />
```

### Android Manifest Features
```xml
<uses-feature android:name="android.hardware.vr.headtracking" android:version="1" android:required="true" />
<uses-feature android:name="oculus.software.handtracking" android:required="false" />
<uses-feature android:name="android.hardware.microphone" android:required="true" />
```

### Oculus Configuration
```xml
<meta-data android:name="com.oculus.supportedDevices" android:value="quest|quest2" />
<meta-data android:name="com.oculus.vr.focusaware" android:value="true" />
<meta-data android:name="com.oculus.handtracking.frequency" android:value="LOW" />
<meta-data android:name="com.oculus.handtracking.support" android:value="optional" />
```

## Input System Configuration

### Action Maps
```
VR Conversation Actions:
  - Select: <XRController>{LeftHand}/selectPressed, <XRController>{RightHand}/selectPressed
  - Activate: <XRController>{LeftHand}/activatePressed, <XRController>{RightHand}/activatePressed
  - UI Press: <XRController>{LeftHand}/uiPressPressed, <XRController>{RightHand}/uiPressPressed
  - Menu: <XRController>{LeftHand}/menuPressed, <XRController>{RightHand}/menuPressed
  - Primary Button: <XRController>{LeftHand}/primaryButton, <XRController>{RightHand}/primaryButton
  - Secondary Button: <XRController>{LeftHand}/secondaryButton, <XRController>{RightHand}/secondaryButton
  - Grip: <XRController>{LeftHand}/gripPressed, <XRController>{RightHand}/gripPressed
  - Position: <XRController>{LeftHand}/devicePosition, <XRController>{RightHand}/devicePosition
  - Rotation: <XRController>{LeftHand}/deviceRotation, <XRController>{RightHand}/deviceRotation
```

## Performance Optimization Settings

### Oculus Specific
```
- Enable GPU Skinning: true
- Static Batching: true
- Dynamic Batching: false (use GPU Instancing instead)
- Use Fixed Foveated Rendering: true
- Symmetric Projection: true
- Optimize Buffer Discards: true
- Phase Sync: false
```

### Unity Rendering
```
- Scripting Backend: IL2CPP
- Api Compatibility Level: .NET Standard 2.1
- Target Device Family: Universal
- Texture Compression: ASTC
- Normal Map Encoding: XYZ
- Lightmap Encoding: HDR
```

## Memory Management
```
- Texture Streaming: true
- Texture Streaming Memory Budget: 512MB (Quest 2), 1024MB (Quest 3)
- Audio Memory: 32MB
- Managed Heap Size: 256MB
- Graphics Memory: Auto
```

## Network Settings
```
- Network Timeout: 60000ms
- Max Fragment Size: 1400
- Packet Loss Simulation: false
- Latency Simulation: false
```