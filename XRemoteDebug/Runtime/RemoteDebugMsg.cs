
namespace XRemoteDebug
{
    public enum RemoteDebugMsg
    {
        ConnectServer,
        Heartbeat,
        BaseInfo,
        Error,
        Hierarchy_RootObjects,
        Hierarchy_SubObjects,
        Patch_LocalUploadStart,
        Patch_LocalUpload,
        Patch_LocalUploadEnd,
        Patch_RemoteFiles,
        Patch_RemoteDelete,
        Patch_RemoteRequire,
        Patch_RemoteOpenFolder,
        Patch_RemoteBack,
        Patch_RemoteCurrentFolder,
        Patch_RemoteSearch,
    }
}