using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    internal sealed class WindowFramesCollection : IEnumerable<WindowFrameInfo>
    {
        private readonly IEnumerable<IVsWindowFrame> _vsWindowFrames = new List<IVsWindowFrame>();

        public WindowFramesCollection(bool showDocumentWindowFrames, bool showToolWindowFrames)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (Package.GetGlobalService(typeof(SVsUIShell)) is IVsUIShell uiShell)
            {
                if (showDocumentWindowFrames)
                {
                    int hr = uiShell.GetDocumentWindowEnum(out IEnumWindowFrames enumWindowFrames);
                    if (ErrorHandler.Succeeded(hr))
                    {
                        _vsWindowFrames = ComUtilities.EnumerableFrom(enumWindowFrames);
                    }
                }

                if (showToolWindowFrames)
                {
                    int hr = uiShell.GetToolWindowEnum(out IEnumWindowFrames enumWindowFrames);
                    if (ErrorHandler.Succeeded(hr))
                    {
                        _vsWindowFrames = _vsWindowFrames.Union(ComUtilities.EnumerableFrom(enumWindowFrames));
                    }
                }
            }
        }

        public IEnumerator<WindowFrameInfo> GetEnumerator()
        {
            foreach (IVsWindowFrame vsWindowFrame in _vsWindowFrames)
            {
                yield return new WindowFrameInfo(vsWindowFrame);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
