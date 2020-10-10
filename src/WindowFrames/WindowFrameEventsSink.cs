using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.CompilerServices;

#nullable enable

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    internal sealed class WindowFrameEventsSink : IVsWindowFrameEvents2
    {
        private readonly WindowFramesDataSource _windowFramesDataSource;
        private uint _cookie;

        public WindowFrameEventsSink(WindowFramesDataSource windowFramesDataSource)
        {
            _windowFramesDataSource = windowFramesDataSource;
        }

        public void Advise()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            IVsUIShell7 uiShell = (IVsUIShell7)Package.GetGlobalService(typeof(SVsUIShell));
            _cookie = uiShell.AdviseWindowFrameEvents(this);
        }

        public void Unadvise()
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            IVsUIShell7 uiShell = (IVsUIShell7)Package.GetGlobalService(typeof(SVsUIShell));
            uiShell.UnadviseWindowFrameEvents(_cookie);
        }

        public void OnFrameCreated(IVsWindowFrame frame)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var frameInfo = new WindowFrameInfo(frame);

            RecordEvent($"{frameInfo.DocumentPath}");

            if (frameInfo.ItemId != 0)
            {
                _windowFramesDataSource.AddOrUpdateEntry(frame);
            }
        }

        public void OnFrameDestroyed(IVsWindowFrame frame)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var frameInfo = new WindowFrameInfo(frame);

            RecordEvent($"{frameInfo.DocumentPath}");

            if (_windowFramesDataSource.TryFindEntry(frameInfo, out WindowFrameEntry windowFrameEntry))
            {
                _windowFramesDataSource.Entries.Remove(windowFrameEntry);
            }
        }

        public void OnFrameIsVisibleChanged(IVsWindowFrame frame, bool newIsVisible)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var frameInfo = new WindowFrameInfo(frame);

            RecordEvent($"{frameInfo.Caption}. IsVisible={newIsVisible}");

            if (_windowFramesDataSource.GetOrAddEntry(frameInfo, out WindowFrameEntry? windowFrameEntry) && (windowFrameEntry != null))
            {
                windowFrameEntry.IsVisible = newIsVisible;
            }
        }

        public void OnFrameIsOnScreenChanged(IVsWindowFrame frame, bool newIsOnScreen)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var frameInfo = new WindowFrameInfo(frame);

            RecordEvent($"{frameInfo.Caption}. IsOnScreen={newIsOnScreen}");
        }

        public void OnActiveFrameChanged(IVsWindowFrame oldFrame, IVsWindowFrame newFrame)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            string? oldFrameCaption = (oldFrame != null) ? new WindowFrameInfo(oldFrame).Caption : null;
            string? newFrameCaption = (newFrame != null) ? new WindowFrameInfo(newFrame).Caption : null;

            RecordEvent($"Old frame = {oldFrameCaption}, New frame = {newFrameCaption}");

            if (newFrame != null)
            {
                _windowFramesDataSource.AddOrUpdateEntry(newFrame);
            }
        }

        public void OnFrameViewReplaced(IVsWindowFrame frame)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var frameInfo = new WindowFrameInfo(frame);

            RecordEvent($"{frameInfo.Caption}. New view = {frameInfo.DocView}");

            if (_windowFramesDataSource.TryFindEntry(frameInfo, out WindowFrameEntry windowFrameEntry))
            {
                windowFrameEntry.DocView = frameInfo.DocView;
            }
        }

        private void RecordEvent(string data, [CallerMemberName] string? callerName = null)
        {
            var frameEvent = new WindowFrameEvent($"{callerName}: {data}");
            _windowFramesDataSource.RecordEvent(frameEvent);
        }
    }
}
