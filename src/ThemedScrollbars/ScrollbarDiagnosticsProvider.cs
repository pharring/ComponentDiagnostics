using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    [Guid(GuidList.ScrollbarDiagnosticsProviderString)]
    class ScrollbarDiagnosticsProvider : IVsDiagnosticsProvider
    {
        const uint Version = 1;

        #region IVsDiagnosticsProvider stuff

        ScrollbarDiagnosticsDataSource _ds;
        object IVsDiagnosticsProvider.DataModel
        {
            get
            {
                if (_ds == null)
                    _ds = new ScrollbarDiagnosticsDataSource();

                return _ds;
            }
        }

        uint IVsDiagnosticsProvider.Version
        {
            get { return Version; }
        }

        #endregion IVsDiagnosticsProvider stuff
    }
}
