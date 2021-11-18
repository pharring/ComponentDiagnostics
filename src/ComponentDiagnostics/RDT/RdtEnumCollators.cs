using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Collates _VSRDTFLAGS, _VSRDTFLAGS2 and _VSRDTFLAGS3 into a single enum
    /// </summary>
    [Flags]
    enum RdtFlags
    {
        None                   = 0,

        RDT_CanBuildFromMemory = _VSRDTFLAGS.RDT_CanBuildFromMemory,
        RDT_CantSave           = _VSRDTFLAGS.RDT_CantSave,
        RDT_CaseSensitive      = _VSRDTFLAGS.RDT_CaseSensitive,
        RDT_DOCMASK            = _VSRDTFLAGS.RDT_DOCMASK,
        RDT_DontAddToMRU       = _VSRDTFLAGS.RDT_DontAddToMRU,
        RDT_DontAutoOpen       = _VSRDTFLAGS.RDT_DontAutoOpen,
        RDT_DontSave           = _VSRDTFLAGS.RDT_DontSave,
        RDT_DontSaveAs         = _VSRDTFLAGS.RDT_DontSaveAs,
        RDT_EditLock           = _VSRDTFLAGS.RDT_EditLock,
        RDT_LOCKMASK           = _VSRDTFLAGS.RDT_LOCKMASK,
        //RDT_NoLock             = _VSRDTFLAGS.RDT_NoLock,
        RDT_NonCreatable       = _VSRDTFLAGS.RDT_NonCreatable,
        RDT_PlaceHolderDoc     = _VSRDTFLAGS.RDT_PlaceHolderDoc,
        RDT_ProjSlnDocument    = _VSRDTFLAGS.RDT_ProjSlnDocument,
        RDT_ReadLock           = _VSRDTFLAGS.RDT_ReadLock,
        RDT_RequestUnlock      = _VSRDTFLAGS.RDT_RequestUnlock,
        RDT_SAVEMASK           = _VSRDTFLAGS.RDT_SAVEMASK,
        RDT_Unlock_NoSave      = _VSRDTFLAGS.RDT_Unlock_NoSave,
        RDT_Unlock_PromptSave  = _VSRDTFLAGS.RDT_Unlock_PromptSave,
        RDT_Unlock_SaveIfDirty = _VSRDTFLAGS.RDT_Unlock_SaveIfDirty,
        RDT_VirtualDocument    = _VSRDTFLAGS.RDT_VirtualDocument,

        RDT_LOCKUNLOCKMASK     = _VSRDTFLAGS2.RDT_LOCKUNLOCKMASK,
        RDT_Lock_WeakEditLock  = _VSRDTFLAGS2.RDT_Lock_WeakEditLock,

        RDT_DontPollForState   = _VSRDTFLAGS3.RDT_DontPollForState,

        RDT_FullLockMask       = RDT_LOCKMASK | RDT_Lock_WeakEditLock,
    }

    /// <summary>
    /// Collates __VSRDTATTRIB and __VSRDTATTRIB, into a single enum
    /// </summary>
    [Flags]
    enum RdtAttributes
    {
        RDTA_Hierarchy              = __VSRDTATTRIB.RDTA_Hierarchy,
        RDTA_ItemID                 = __VSRDTATTRIB.RDTA_ItemID,
        RDTA_MkDocument             = __VSRDTATTRIB.RDTA_MkDocument,
        RDTA_DocDataIsDirty         = __VSRDTATTRIB.RDTA_DocDataIsDirty,
        RDTA_DocDataIsNotDirty      = __VSRDTATTRIB.RDTA_DocDataIsNotDirty,
        RDTA_DocDataReloaded        = __VSRDTATTRIB.RDTA_DocDataReloaded,
        RDTA_AltHierarchyItemID     = __VSRDTATTRIB.RDTA_AltHierarchyItemID,
        RDTA_NOTIFYDOCCHANGEDMASK   = __VSRDTATTRIB.RDTA_NOTIFYDOCCHANGEDMASK,

        RDTA_DocDataIsReadOnly      = __VSRDTATTRIB2.RDTA_DocDataIsReadOnly, 
        RDTA_DocDataIsNotReadOnly   = __VSRDTATTRIB2.RDTA_DocDataIsNotReadOnly, 
        RDTA_NOTIFYDOCCHANGEDEXMASK = __VSRDTATTRIB2.RDTA_NOTIFYDOCCHANGEDEXMASK,
    }
}
