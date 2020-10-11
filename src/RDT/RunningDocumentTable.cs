using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    public class RunningDocumentTable : IEnumerable<RunningDocumentInfo>
    {
        readonly IVsRunningDocumentTable _rdt;

        public RunningDocumentTable()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            _rdt = (IVsRunningDocumentTable)Package.GetGlobalService(typeof(SVsRunningDocumentTable));
        }

        public RunningDocumentInfo GetDocumentInfo(uint docCookie)
        {
            if (_rdt == null)
                return default;

            return new RunningDocumentInfo(_rdt, docCookie);
        }

        public IEnumerator<RunningDocumentInfo> GetEnumerator()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (_rdt == null)
                yield break;

            if (ErrorHandler.Failed(_rdt.GetRunningDocumentsEnum(out var docEnum)) || (docEnum == null))
                yield break;

            const int count = 10;
            uint[] cookies = new uint[count];
            int hr;

            while (ErrorHandler.Succeeded(hr = docEnum.Next((uint)cookies.Length, cookies, out var fetched)))
            {
                for (int i = 0; i < (int)fetched; i++)
                {
                    yield return GetDocumentInfo(cookies[i]);
                }

                if (hr == VSConstants.S_FALSE)
                    break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            return GetEnumerator();
        }
    }
}
