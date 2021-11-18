#nullable enable

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// __VSCREATEDOCWIN, __VSCREATEDOCWIN2
    /// </summary>
    public enum CreateDocWinEnum
    {
        CDW_RDTFLAGS_MASK = 0x000FFFFF,
        CDW_fDockable = 0x00100000,
        CDW_fAltDocData = 0x00200000,
        CDW_fCreateNewWindow = 0x00400000,  // allow an additional window   to be created for Window.NewWindow support

        CDW_REPLACE_WAIT_WINDOW = 0x00800000, // Wait window should be replaced
    }

    /// <summary>
    /// __VSCREATETOOLWIN, __VSCREATETOOLWIN2
    /// </summary>
    public enum CreateToolWinEnum
    {
        CTW_RESERVED_MASK = 0x0000FFFF, // reserved bits
        CTW_fInitNew = 0x00010000,
        CTW_fActivateWithProject = 0x00020000,
        CTW_fActivateWithDocument = 0x00040000,
        CTW_fForceCreate = 0x00080000,
        CTW_fHasBorder = 0x00100000,
        CTW_fMultiInstance = 0x00200000,
        CTW_fToolbarHost = 0x00400000,

        CTW_fDocumentLikeTool = 0x00800000 // A tool window that behaves and has a lifetime like a document
    };
}
