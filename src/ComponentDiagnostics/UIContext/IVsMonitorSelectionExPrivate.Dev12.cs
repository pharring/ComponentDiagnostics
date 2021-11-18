using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.Internal.VisualStudio.Shell.Interop.Dev12
{
    [CompilerGenerated, Guid("DDB6BC47-A898-412B-B9B8-1B79C379836E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), TypeIdentifier]
    [ComImport]
    public interface IVsMonitorSelectionExPrivate
    {
        void Reserved01();
        void Reserved02();
        void Reserved03();
        void Reserved04();
        void Reserved05();
        void Reserved06();
        void Reserved07();
        void Reserved08();
        void Reserved09();
        void Reserved10();
        void Reserved11();
        void Reserved12();
        void Reserved13(); // Added in the Dev12 version of the internal interop
        [PreserveSig]
        int GetContextOfElement([In] uint elementid, [MarshalAs(UnmanagedType.Interface)] out IVsTrackSelectionEx ppVsSelCtx);
        [PreserveSig]
        int GetContextOfSelection([MarshalAs(UnmanagedType.Interface)] out IVsTrackSelectionEx ppHierarchyCtx, [MarshalAs(UnmanagedType.Interface)] out IVsTrackSelectionEx ppSelectionContainerCtx);
    }
}