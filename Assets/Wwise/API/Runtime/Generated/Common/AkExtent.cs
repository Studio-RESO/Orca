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


public class AkExtent : global::System.IDisposable {
  private global::System.IntPtr swigCPtr;
  protected bool swigCMemOwn;

  internal AkExtent(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = cPtr;
  }

  internal static global::System.IntPtr getCPtr(AkExtent obj) {
    return (obj == null) ? global::System.IntPtr.Zero : obj.swigCPtr;
  }

  internal virtual void setCPtr(global::System.IntPtr cPtr) {
    Dispose();
    swigCPtr = cPtr;
  }

  ~AkExtent() {
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
          AkSoundEnginePINVOKE.CSharp_delete_AkExtent(swigCPtr);
        }
        swigCPtr = global::System.IntPtr.Zero;
      }
      global::System.GC.SuppressFinalize(this);
    }
  }

  public AkExtent() : this(AkSoundEnginePINVOKE.CSharp_new_AkExtent__SWIG_0(), true) {
  }

  public AkExtent(float in_halfWidth, float in_halfHeight, float in_halfDepth) : this(AkSoundEnginePINVOKE.CSharp_new_AkExtent__SWIG_1(in_halfWidth, in_halfHeight, in_halfDepth), true) {
  }

  public float halfWidth { set { AkSoundEnginePINVOKE.CSharp_AkExtent_halfWidth_set(swigCPtr, value); }  get { return AkSoundEnginePINVOKE.CSharp_AkExtent_halfWidth_get(swigCPtr); } 
  }

  public float halfHeight { set { AkSoundEnginePINVOKE.CSharp_AkExtent_halfHeight_set(swigCPtr, value); }  get { return AkSoundEnginePINVOKE.CSharp_AkExtent_halfHeight_get(swigCPtr); } 
  }

  public float halfDepth { set { AkSoundEnginePINVOKE.CSharp_AkExtent_halfDepth_set(swigCPtr, value); }  get { return AkSoundEnginePINVOKE.CSharp_AkExtent_halfDepth_get(swigCPtr); } 
  }

}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.