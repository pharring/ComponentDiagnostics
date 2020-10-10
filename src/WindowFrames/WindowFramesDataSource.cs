using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.ObjectModel;
using System.Linq;

#nullable enable

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    internal sealed class WindowFramesDataSource
    {
        const int DefaultMaxEventCount = 100;
        public bool ShowDocumentWindowFrames = true;
        public bool ShowToolWindowFrames = true;

        public WindowFramesDataSource()
        {
            _entries = default!;
            _events = default!;

            ResetEntries();
        }

        public bool TryFindEntry(WindowFrameInfo windowFrameInfo, out WindowFrameEntry windowFrameEntry)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            windowFrameEntry = Entries.FirstOrDefault(_ => string.Equals(_.DocumentPath, windowFrameInfo.DocumentPath, StringComparison.OrdinalIgnoreCase) &&
                                                           _.EditorType.Equals(windowFrameInfo.EditorType) &&
                                                           string.Equals(_.PhysicalView, windowFrameInfo.PhysicalView, StringComparison.OrdinalIgnoreCase));

            return windowFrameEntry != null;
        }

        public bool GetOrAddEntry(WindowFrameInfo windowFrameInfo, out WindowFrameEntry? windowFrameEntry)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (TryFindEntry(windowFrameInfo, out windowFrameEntry))
            {
                return true;
            }

            if (WindowFrameCanBeShown(windowFrameInfo))
            {
                windowFrameEntry = new WindowFrameEntry(windowFrameInfo);
                Entries.Add(windowFrameEntry);
            }

            windowFrameEntry = null;
            return false;
        }

        public void AddOrUpdateEntry(IVsWindowFrame vsWindowFrame)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            if (vsWindowFrame == null)
            {
                return;
            }

            var windowFrameInfo = new WindowFrameInfo(vsWindowFrame);

            if (GetOrAddEntry(windowFrameInfo, out WindowFrameEntry? windowFrameEntry))
            {
                windowFrameEntry?.Update(windowFrameInfo);
            }
        }

        public void SetVisibilityOfEntries(bool showDocumentWindowFrames, bool showToolWindowFrames)
        {
            this.ShowDocumentWindowFrames = showDocumentWindowFrames;
            this.ShowToolWindowFrames = showToolWindowFrames;

            ResetEntries();
        }

        /// <summary>
        /// Records a Window Frame event in the events collection
        /// </summary>
        /// <param name="windowFrameEvent"></param>
        public void RecordEvent(WindowFrameEvent windowFrameEvent)
        {
            Events.Add(windowFrameEvent);

            if (Events.Count > MaxEventCount)
                Events.RemoveAt(0);
        }

        /// <summary>
        /// Empties the Events collection
        /// </summary>
        public void ClearEvents()
        {
            Events.Clear();
        }

        private void ResetEntries()
        {
            Entries.Clear();

            foreach (WindowFrameInfo windowFrameInfo in new WindowFramesCollection(ShowDocumentWindowFrames, ShowToolWindowFrames))
            {
                Entries.Add(new WindowFrameEntry(windowFrameInfo));
            }
        }

        private bool WindowFrameCanBeShown(WindowFrameInfo windowFrameInfo)
        {
            Shell.ThreadHelper.ThrowIfNotOnUIThread();
            return (windowFrameInfo.Type == __WindowFrameTypeFlags.WINDOWFRAMETYPE_Document) ? ShowDocumentWindowFrames :
                (windowFrameInfo.Type == __WindowFrameTypeFlags.WINDOWFRAMETYPE_Tool) && ShowToolWindowFrames;
        }

        /// <summary>
        /// The maximum size of the Events collection
        /// </summary>
        public int MaxEventCount
        {
            get
            {
                return _maxEventCount;
            }
            set
            {
                if (_maxEventCount != value)
                {
                    _maxEventCount = value;

                    // trim the oldest events to get under the size limit
                    while (Events.Count > _maxEventCount)
                        Events.RemoveAt(0);
                }
            }
        }
        private int _maxEventCount = DefaultMaxEventCount;

        /// <summary>
        /// The collection of Window Frame entries
        /// </summary>
        public ObservableCollection<WindowFrameEntry> Entries => _entries ??= new ObservableCollection<WindowFrameEntry>();
        private ObservableCollection<WindowFrameEntry> _entries;


        /// <summary>
        /// The most recent Window Frame events.  Its size is limited to MaxEventCount
        /// </summary>
        public ObservableCollection<WindowFrameEvent> Events => _events ??= new ObservableCollection<WindowFrameEvent>();
        private ObservableCollection<WindowFrameEvent> _events;
    }
}
