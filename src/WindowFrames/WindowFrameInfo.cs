using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;

#nullable enable

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    public struct WindowFrameInfo
    {
        private readonly IVsWindowFrame _windowFrame;
        private const int VSFPROPID_IsWaitFrame = -5035; // Use __VSFPROPID8 once it is made public

        public WindowFrameInfo(IVsWindowFrame windowFrame)
        {
            this._windowFrame = windowFrame ?? throw new ArgumentNullException(nameof(windowFrame));
        }

        public string? Caption
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_Caption, out object value);

                return ErrorHandler.Succeeded(hr) ? (string)value : default;
            }
        }

        public Guid CmdUIGuid
        {
            get
            {
                int hr = _windowFrame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_CmdUIGuid, out Guid value);

                return ErrorHandler.Succeeded(hr) ? value : default;
            }
        }

        public string? CreateWinFlags
        {
            get
            {
                __VSFPROPID property = (this.Type == __WindowFrameTypeFlags.WINDOWFRAMETYPE_Document) ? __VSFPROPID.VSFPROPID_CreateDocWinFlags : __VSFPROPID.VSFPROPID_CreateToolWinFlags;

                int hr = _windowFrame.GetProperty((int)property, out object value);

                if (ErrorHandler.Succeeded(hr))
                {
                    if (this.Type == __WindowFrameTypeFlags.WINDOWFRAMETYPE_Document)
                    {
                        return GetDocWindowCreateEnum((int)value);
                    }
                    else
                    {
                        return GetToolWindowCreateEnum((int)value);
                    }
                }

                return default;
            }
        }

        public int DocCookie
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocCookie, out object value);

                return ErrorHandler.Succeeded(hr) ? (int)value : default;
            }
        }

        public string? DocData
        {
            get
            {
                string? docDataAsString = null;

                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out object value);

                if (ErrorHandler.Succeeded(hr))
                {
                    docDataAsString = value?.ToString();

                    IVsTextBuffer? buffer = value as IVsTextBuffer;
                    if (buffer == null)
                    {
                        if (value is IVsTextBufferProvider bufferProvider)
                        {
                            hr = bufferProvider.GetTextBuffer(out IVsTextLines lines);
                            if (ErrorHandler.Succeeded(hr))
                            {
                                buffer = lines;
                            }
                        }
                    }

                    if (buffer != null)
                    {
                        hr = buffer.GetSize(out int length);
                        if (ErrorHandler.Succeeded(hr))
                        {
                            docDataAsString += $"{Environment.NewLine}Size={length}";
                        }

                        hr = buffer.GetLanguageServiceID(out Guid languageServiceId);
                        if (ErrorHandler.Succeeded(hr))
                        {
                            docDataAsString += $"{Environment.NewLine}LanguageServiceID={languageServiceId}";
                        }

                        hr = buffer.GetStateFlags(out uint flags);
                        if (ErrorHandler.Succeeded(hr) && (flags != 0))
                        {
                            docDataAsString += $"{Environment.NewLine}StateFlags={flags:X} ";
                            if ((flags & (uint)BUFFERSTATEFLAGS.BSF_FILESYS_READONLY) != 0)
                            {
                                docDataAsString += "BSF_FILESYS_READONLY ";
                            }
                            if ((flags & (uint)BUFFERSTATEFLAGS.BSF_MODIFIED) != 0)
                            {
                                docDataAsString += "BSF_MODIFIED ";
                            }
                            if ((flags & (uint)BUFFERSTATEFLAGS.BSF_SUPPRESS_UI) != 0)
                            {
                                docDataAsString += "BSF_SUPPRESS_UI ";
                            }
                            if ((flags & (uint)BUFFERSTATEFLAGS.BSF_USER_READONLY) != 0)
                            {
                                docDataAsString += "BSF_USER_READONLY ";
                            }
                        }
                    }
                }

                return docDataAsString;
            }
        }

        public string? DocumentPath
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out object value);

                return ErrorHandler.Succeeded(hr) ? (string)value : default;
            }
        }

        public string? DocView
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object value);

                return ErrorHandler.Succeeded(hr) ? value?.ToString() : default;
            }
        }

        /// <summary>
        /// For document window frames, this is the identifier of the Editor Factory that created the document.
        /// For tool window frames, this is the unique identifier of the tool window type.
        /// </summary>
        public Guid EditorType
        {
            get
            {
                __VSFPROPID property = (this.Type == __WindowFrameTypeFlags.WINDOWFRAMETYPE_Document) ? __VSFPROPID.VSFPROPID_guidEditorType : __VSFPROPID.VSFPROPID_GuidPersistenceSlot;

                int hr = _windowFrame.GetGuidProperty((int)property, out Guid value);

                return ErrorHandler.Succeeded(hr) ? value : default;
            }
        }

        public VsFrameMode? FrameMode
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_FrameMode, out object value);

                return ErrorHandler.Succeeded(hr) ? (VsFrameMode)value : default;
            }
        }

        public string? Hierarchy
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_Hierarchy, out object value);

                return ErrorHandler.Succeeded(hr) ? value?.ToString() : default;
            }
        }

        public bool IsVisible
        {
            get
            {
                return _windowFrame.IsVisible() == VSConstants.S_OK;
            }
        }

        public bool IsWaitFrame
        {
            get
            {
                int hr = _windowFrame.GetProperty(VSFPROPID_IsWaitFrame, out object value);

                return ErrorHandler.Succeeded(hr) && (bool)value;
            }
        }

        public __VSTABBEDMODE IsWindowTabbed
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_IsWindowTabbed, out object value);

                return ErrorHandler.Succeeded(hr) ? (__VSTABBEDMODE)(short)value : default;
            }
        }

        public int ItemId
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ItemID, out object value);

                return ErrorHandler.Succeeded(hr) ? (int)value : default;
            }
        }

        public bool PendingInitialization
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID6.VSFPROPID_PendingInitialization, out object value);

                return ErrorHandler.Succeeded(hr) && (bool)value;
            }
        }

        public string? PhysicalView
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_pszPhysicalView, out object value);

                return ErrorHandler.Succeeded(hr) ? (string)value : default;
            }
        }

        public string? RDTDocData
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_RDTDocData, out object value);

                return ErrorHandler.Succeeded(hr) ? value?.ToString() : default;
            }
        }

        public string? ShortCaption
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_ShortCaption, out object value);

                return ErrorHandler.Succeeded(hr) ? (string)value : default;
            }
        }

        public __WindowFrameTypeFlags Type
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_Type, out object value);

                return ErrorHandler.Succeeded(hr) ? (__WindowFrameTypeFlags)(int)value : default;
            }
        }

        public VSWINDOWSTATE WindowState
        {
            get
            {
                int hr = _windowFrame.GetProperty((int)__VSFPROPID.VSFPROPID_WindowState, out object value);

                return ErrorHandler.Succeeded(hr) ? (VSWINDOWSTATE)value : default;
            }
        }

        private string GetDocWindowCreateEnum(int createDocWinEnum)
        {
            string enumValue = $"[{createDocWinEnum:X}] ";

            if ((createDocWinEnum & (int)CreateDocWinEnum.CDW_fAltDocData) != 0)
            {
                enumValue += CreateDocWinEnum.CDW_fAltDocData.ToString() + " ";
            }
            if ((createDocWinEnum & (int)CreateDocWinEnum.CDW_fCreateNewWindow) != 0)
            {
                enumValue += CreateDocWinEnum.CDW_fCreateNewWindow.ToString() + " ";
            }
            if ((createDocWinEnum & (int)CreateDocWinEnum.CDW_fDockable) != 0)
            {
                enumValue += CreateDocWinEnum.CDW_fDockable.ToString() + " ";
            }
            if ((createDocWinEnum & (int)CreateDocWinEnum.CDW_REPLACE_WAIT_WINDOW) != 0)
            {
                enumValue += CreateDocWinEnum.CDW_REPLACE_WAIT_WINDOW.ToString();
            }

            return enumValue;
        }

        private string GetToolWindowCreateEnum(int createToolWinEnum)
        {
            string enumValue = $"[{createToolWinEnum:X}] ";

            if ((createToolWinEnum & (int)CreateToolWinEnum.CTW_fActivateWithDocument) != 0)
            {
                enumValue += CreateToolWinEnum.CTW_fActivateWithDocument.ToString() + " ";
            }
            if ((createToolWinEnum & (int)CreateToolWinEnum.CTW_fActivateWithProject) != 0)
            {
                enumValue += CreateToolWinEnum.CTW_fActivateWithProject.ToString() + " ";
            }
            if ((createToolWinEnum & (int)CreateToolWinEnum.CTW_fDocumentLikeTool) != 0)
            {
                enumValue += CreateToolWinEnum.CTW_fDocumentLikeTool.ToString() + " ";
            }
            if ((createToolWinEnum & (int)CreateToolWinEnum.CTW_fForceCreate) != 0)
            {
                enumValue += CreateToolWinEnum.CTW_fForceCreate.ToString() + " ";
            }
            if ((createToolWinEnum & (int)CreateToolWinEnum.CTW_fHasBorder) != 0)
            {
                enumValue += CreateToolWinEnum.CTW_fHasBorder.ToString() + " ";
            }
            if ((createToolWinEnum & (int)CreateToolWinEnum.CTW_fInitNew) != 0)
            {
                enumValue += CreateToolWinEnum.CTW_fInitNew.ToString() + " ";
            }
            if ((createToolWinEnum & (int)CreateToolWinEnum.CTW_fMultiInstance) != 0)
            {
                enumValue += CreateToolWinEnum.CTW_fMultiInstance.ToString() + " ";
            }
            if ((createToolWinEnum & (int)CreateToolWinEnum.CTW_fToolbarHost) != 0)
            {
                enumValue += CreateToolWinEnum.CTW_fToolbarHost.ToString();
            }

            return enumValue;
        }
    }
}
