#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (https://www.swig.org).
// Version 4.3.0
//
// Do not make changes to this file unless you know what you are doing - modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class AkOutputSettings : global::System.IDisposable {
  private global::System.IntPtr swigCPtr;
  protected bool swigCMemOwn;

  internal AkOutputSettings(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = cPtr;
  }

  internal static global::System.IntPtr getCPtr(AkOutputSettings obj) {
    return (obj == null) ? global::System.IntPtr.Zero : obj.swigCPtr;
  }

  internal virtual void setCPtr(global::System.IntPtr cPtr) {
    Dispose();
    swigCPtr = cPtr;
  }

  ~AkOutputSettings() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          AkSoundEnginePINVOKE.CSharp_delete_AkOutputSettings(swigCPtr);
        }
        swigCPtr = global::System.IntPtr.Zero;
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public AkOutputSettings() : this(AkSoundEnginePINVOKE.CSharp_new_AkOutputSettings__SWIG_0(), true) {
  }

  public AkOutputSettings(string in_szDeviceShareSet, uint in_idDevice, AkChannelConfig in_channelConfig, AkPanningRule in_ePanning) : this(AkSoundEnginePINVOKE.CSharp_new_AkOutputSettings__SWIG_1(in_szDeviceShareSet, in_idDevice, AkChannelConfig.getCPtr(in_channelConfig), (int)in_ePanning), true) {
  }

  public AkOutputSettings(string in_szDeviceShareSet, uint in_idDevice, AkChannelConfig in_channelConfig) : this(AkSoundEnginePINVOKE.CSharp_new_AkOutputSettings__SWIG_2(in_szDeviceShareSet, in_idDevice, AkChannelConfig.getCPtr(in_channelConfig)), true) {
  }

  public AkOutputSettings(string in_szDeviceShareSet, uint in_idDevice) : this(AkSoundEnginePINVOKE.CSharp_new_AkOutputSettings__SWIG_3(in_szDeviceShareSet, in_idDevice), true) {
  }

  public AkOutputSettings(string in_szDeviceShareSet) : this(AkSoundEnginePINVOKE.CSharp_new_AkOutputSettings__SWIG_4(in_szDeviceShareSet), true) {
  }

  public uint audioDeviceShareset { set { AkSoundEnginePINVOKE.CSharp_AkOutputSettings_audioDeviceShareset_set(swigCPtr, value); }  get { return AkSoundEnginePINVOKE.CSharp_AkOutputSettings_audioDeviceShareset_get(swigCPtr); } 
  }

  public uint idDevice { set { AkSoundEnginePINVOKE.CSharp_AkOutputSettings_idDevice_set(swigCPtr, value); }  get { return AkSoundEnginePINVOKE.CSharp_AkOutputSettings_idDevice_get(swigCPtr); } 
  }

  public AkPanningRule ePanningRule { set { AkSoundEnginePINVOKE.CSharp_AkOutputSettings_ePanningRule_set(swigCPtr, (int)value); }  get { return (AkPanningRule)AkSoundEnginePINVOKE.CSharp_AkOutputSettings_ePanningRule_get(swigCPtr); } 
  }

  public AkChannelConfig channelConfig { set { AkSoundEnginePINVOKE.CSharp_AkOutputSettings_channelConfig_set(swigCPtr, AkChannelConfig.getCPtr(value)); } 
    get {
      global::System.IntPtr cPtr = AkSoundEnginePINVOKE.CSharp_AkOutputSettings_channelConfig_get(swigCPtr);
      AkChannelConfig ret = (cPtr == global::System.IntPtr.Zero) ? null : new AkChannelConfig(cPtr, false);
      return ret;
    } 
  }

}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.