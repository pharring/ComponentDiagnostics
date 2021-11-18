using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;

#nullable enable

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    [Guid (GuidList.WindowFramesDiagnosticsProviderString)]
    public sealed class WindowFramesDiagnosticsProvider : IVsDiagnosticsProvider
    {
        private WindowFramesDataSource? _windowFramesDataSource;

        uint IVsDiagnosticsProvider.Version => 1;

        object IVsDiagnosticsProvider.DataModel
        {
            get
            {
                if (_windowFramesDataSource is null)
                {
                    _windowFramesDataSource = new WindowFramesDataSource();
                }

                return _windowFramesDataSource;
            }
        }
    }
}
