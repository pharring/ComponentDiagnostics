using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    struct ProviderRegistration
    {
        public Guid guid;
        public string name;
        public Guid package;
        public uint version;
        public VsUIElementDescriptor viewDescriptor;
    }
}
