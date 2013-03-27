using System;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Used to indicate that a view could not be created
    /// </summary>
    [Serializable]
    class ViewCreationException : Exception
    {
        public ViewCreationException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
    }
}
