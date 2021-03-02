using XCommon.Editor;

namespace XRemoteDebug
{
    public interface IRemoteDebugPanel : ITianGlyphPanel
    {
        void OnEnable();
        void Update();
    }
}