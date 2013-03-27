using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.Shell;

using VSCOOKIE = System.UInt32;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    class RdtDiagnosticsDataSource
    {
        const int DefaultMaxEventCount = 100;

        public RdtDiagnosticsDataSource()
        {
            RdtEntry.OnFinalUnlock += OnRdtEntryFinalUnlock;

            foreach (RunningDocumentInfo info in new RunningDocumentTable())
            {
                Entries.Add (new RdtEntry(info));
            }
        }

        /// <summary>
        /// RdtEntry.OnFinalUnlock event handler
        /// </summary>
        void OnRdtEntryFinalUnlock(object sender, EventArgs e)
        {
            Entries.Remove ((RdtEntry) sender);
        }

        /// <summary>
        /// Returns the RdtEntry for <paramref name="cookie"/>, or null if there's no matching entry.
        /// </summary>
        public RdtEntry FindEntry (VSCOOKIE cookie)
        {
            return Entries.FirstOrDefault (e => e.Cookie == cookie);
        }

        /// <summary>
        /// Records an RDT event in the events collection
        /// </summary>
        /// <param name="evt"></param>
        public void RecordEvent (RdtEvent evt)
        {
            Events.Add (evt);

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
        int _maxEventCount = DefaultMaxEventCount;

        /// <summary>
        /// The collection RDT entries
        /// </summary>
        public ObservableCollection<RdtEntry> Entries
        {
            get
            {
                if (_entries == null)
                    _entries = new ObservableCollection<RdtEntry>();

                return _entries;
            }
        }
        ObservableCollection<RdtEntry> _entries;

        /// <summary>
        /// The most recent RDT events.  Its size is limited to MaxEventCount
        /// </summary>
        public ObservableCollection<RdtEvent> Events
        {
            get
            {
                if (_events == null)
                    _events = new ObservableCollection<RdtEvent>();

                return _events;
            }
        }
        ObservableCollection<RdtEvent> _events;
    }
}
