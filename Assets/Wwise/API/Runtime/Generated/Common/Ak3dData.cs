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


public class Ak3dData : global::System.IDisposable {
  private global::System.IntPtr swigCPtr;
  protected bool swigCMemOwn;

  internal Ak3dData(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = cPtr;
  }

  internal static global::System.IntPtr getCPtr(Ak3dData obj) {
    return (obj == null) ? global::System.IntPtr.Zero : obj.swigCPtr;
  }

  internal virtual void setCPtr(global::System.IntPtr cPtr) {
    Dispose();
    swigCPtr = cPtr;
  }

  ~Ak3dData() {
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
          AkSoundEnginePINVOKE.CSharp_delete_Ak3dData(swigCPtr);
        }
        swigCPtr = global::System.IntPtr.Zero;
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public Ak3dData() : this(AkSoundEnginePINVOKE.CSharp_new_Ak3dData(), true) {
  }

  public AkTransform xform { set { AkSoundEnginePINVOKE.CSharp_Ak3dData_xform_set(swigCPtr, AkTransform.getCPtr(value)); } 
    get {
      global::System.IntPtr cPtr = AkSoundEnginePINVOKE.CSharp_Ak3dData_xform_get(swigCPtr);
      AkTransform ret = (cPtr == global::System.IntPtr.Zero) ? null : new AkTransform(cPtr, false);
      return ret;
    } 
  }

  public float spread { set { AkSoundEnginePINVOKE.CSharp_Ak3dData_spread_set(swigCPtr, value); }  get { return AkSoundEnginePINVOKE.CSharp_Ak3dData_spread_get(swigCPtr); } 
  }

  public float focus { set { AkSoundEnginePINVOKE.CSharp_Ak3dData_focus_set(swigCPtr, value); }  get { return AkSoundEnginePINVOKE.CSharp_Ak3dData_focus_get(swigCPtr); } 
  }

  public uint uEmitterChannelMask { set { AkSoundEnginePINVOKE.CSharp_Ak3dData_uEmitterChannelMask_set(swigCPtr, value); }  get { return AkSoundEnginePINVOKE.CSharp_Ak3dData_uEmitterChannelMask_get(swigCPtr); } 
  }

}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.