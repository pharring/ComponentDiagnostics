using System;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    static class GuidList
    {
        public const string PackageString                      = "358da9fb-1568-3e9a-8ec9-e41a0d763fef";
        public const string ToolWindowString                   = "7d12284c-f8fd-49b0-b6d6-becdcd896e1f";
        public const string CommandSetString                   = "2112b0e0-26ed-4848-b325-f2e5192798b7";
        public const string UiFactoryString                    = "08ad9d3f-5023-4bf1-905e-aaa0ec595941";
        public const string RdtDiagnosticsProviderString       = "ced6ef6d-091f-4faf-9628-e1c67152f39e";
        public const string ScrollbarDiagnosticsProviderString = "e886170b-b0c4-4155-9b00-04745b300d91";

        public static readonly Guid CommandSet = new Guid(CommandSetString);
        public static readonly Guid UiFactory  = new Guid(UiFactoryString);
    }
}
