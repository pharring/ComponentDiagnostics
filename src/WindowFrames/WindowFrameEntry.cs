using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Windows;

#nullable enable

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    internal sealed class WindowFrameEntry : DependencyObject
    {
        public WindowFrameEntry(WindowFrameInfo windowFrameInfo)
        {
            Caption = windowFrameInfo.Caption;
            CmdUIGuid = windowFrameInfo.CmdUIGuid;
            CreateWinFlags = windowFrameInfo.CreateWinFlags;
            DocCookie = windowFrameInfo.DocCookie;
            DocData = windowFrameInfo.DocData;
            DocumentPath = windowFrameInfo.DocumentPath;
            DocView = windowFrameInfo.DocView;
            EditorType = windowFrameInfo.EditorType;
            FrameMode = windowFrameInfo.FrameMode;
            Hierarchy = windowFrameInfo.Hierarchy;
            IsVisible = windowFrameInfo.IsVisible;
            IsWaitFrame = windowFrameInfo.IsWaitFrame;
            IsWindowTabbed = windowFrameInfo.IsWindowTabbed;
            ItemId = windowFrameInfo.ItemId;
            PhysicalView = windowFrameInfo.PhysicalView;
            RDTDocData = windowFrameInfo.RDTDocData;
            ShortCaption = windowFrameInfo.ShortCaption;
            FrameType = windowFrameInfo.Type;
            WindowState = windowFrameInfo.WindowState;
        }

        public static readonly DependencyProperty CaptionProperty =
           DependencyProperty.Register(nameof(Caption), typeof(string), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public string? Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty CmdUIGuidProperty =
          DependencyProperty.Register("CmdUIGuid", typeof(Guid), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public Guid CmdUIGuid
        {
            get { return (Guid)GetValue(CmdUIGuidProperty); }
            set { SetValue(CmdUIGuidProperty, value); }
        }

        public static readonly DependencyProperty CreateWinFlagsProperty =
          DependencyProperty.Register("CreateWinFlags", typeof(string), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public string? CreateWinFlags
        {
            get { return (string)GetValue(CreateWinFlagsProperty); }
            set { SetValue(CreateWinFlagsProperty, value); }
        }

        public static readonly DependencyProperty DocCookieProperty =
          DependencyProperty.Register("DocCookie", typeof(int), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public int DocCookie
        {
            get { return (int)GetValue(DocCookieProperty); }
            set { SetValue(DocCookieProperty, value); }
        }

        public static readonly DependencyProperty DocDataProperty =
          DependencyProperty.Register("DocData", typeof(string), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public string? DocData
        {
            get { return (string)GetValue(DocDataProperty); }
            set { SetValue(DocDataProperty, value); }
        }

        public static readonly DependencyProperty DocumentPathProperty =
          DependencyProperty.Register("DocumentPath", typeof(string), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public string? DocumentPath
        {
            get { return (string)GetValue(DocumentPathProperty); }
            set { SetValue(DocumentPathProperty, value); }
        }

        public static readonly DependencyProperty DocViewProperty =
          DependencyProperty.Register("DocView", typeof(string), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public string? DocView
        {
            get { return (string)GetValue(DocViewProperty); }
            set { SetValue(DocViewProperty, value); }
        }

        public static readonly DependencyProperty EditorTypeProperty =
          DependencyProperty.Register("EditorType", typeof(Guid), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public Guid EditorType
        {
            get { return (Guid)GetValue(EditorTypeProperty); }
            set { SetValue(EditorTypeProperty, value); }
        }

        public static readonly DependencyProperty FrameModeProperty =
          DependencyProperty.Register("FrameMode", typeof(VsFrameMode), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public VsFrameMode? FrameMode
        {
            get { return (VsFrameMode)GetValue(FrameModeProperty); }
            set { SetValue(FrameModeProperty, value); }
        }

        public static readonly DependencyProperty HierarchyProperty =
          DependencyProperty.Register("Hierarchy", typeof(string), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public string? Hierarchy
        {
            get { return (string)GetValue(HierarchyProperty); }
            set { SetValue(HierarchyProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleProperty =
          DependencyProperty.Register("IsVisible", typeof(bool), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsWaitFrameProperty =
          DependencyProperty.Register("IsWaitFrame", typeof(bool), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public bool IsWaitFrame
        {
            get { return (bool)GetValue(IsWaitFrameProperty); }
            set { SetValue(IsWaitFrameProperty, value); }
        }

        public static readonly DependencyProperty IsWindowTabbedProperty =
          DependencyProperty.Register("IsWindowTabbed", typeof(__VSTABBEDMODE), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public __VSTABBEDMODE IsWindowTabbed
        {
            get { return (__VSTABBEDMODE)GetValue(IsWindowTabbedProperty); }
            set { SetValue(IsWindowTabbedProperty, value); }
        }

        public static readonly DependencyProperty ItemIdProperty =
          DependencyProperty.Register("ItemId", typeof(int), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public int ItemId
        {
            get { return (int)GetValue(ItemIdProperty); }
            set { SetValue(ItemIdProperty, value); }
        }

        public static readonly DependencyProperty PhysicalViewProperty =
          DependencyProperty.Register("PhysicalView", typeof(string), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public string? PhysicalView
        {
            get { return (string)GetValue(PhysicalViewProperty); }
            set { SetValue(PhysicalViewProperty, value); }
        }

        public static readonly DependencyProperty RDTDocDataProperty =
          DependencyProperty.Register("RDTDocData", typeof(string), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public string? RDTDocData
        {
            get { return (string)GetValue(RDTDocDataProperty); }
            set { SetValue(RDTDocDataProperty, value); }
        }

        public static readonly DependencyProperty ShortCaptionProperty =
          DependencyProperty.Register("ShortCaption", typeof(string), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public string? ShortCaption
        {
            get { return (string)GetValue(ShortCaptionProperty); }
            set { SetValue(ShortCaptionProperty, value); }
        }

        public static readonly DependencyProperty FrameTypeProperty =
          DependencyProperty.Register("FrameType", typeof(__WindowFrameTypeFlags), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public __WindowFrameTypeFlags FrameType
        {
            get { return (__WindowFrameTypeFlags)GetValue(FrameTypeProperty); }
            set { SetValue(FrameTypeProperty, value); }
        }

        public static readonly DependencyProperty WindowStateProperty =
          DependencyProperty.Register("WindowState", typeof(VSWINDOWSTATE), typeof(WindowFrameEntry), new UIPropertyMetadata(null));

        public VSWINDOWSTATE WindowState
        {
            get { return (VSWINDOWSTATE)GetValue(WindowStateProperty); }
            set { SetValue(WindowStateProperty, value); }
        }

        internal void Update(WindowFrameInfo newFrameInfo)
        {
            this.Caption = newFrameInfo.Caption;
            this.CmdUIGuid = newFrameInfo.CmdUIGuid;
            this.CreateWinFlags = newFrameInfo.CreateWinFlags;
            this.DocCookie = newFrameInfo.DocCookie;
            this.DocData = newFrameInfo.DocData;
            this.DocumentPath = newFrameInfo.DocumentPath;
            this.DocView = newFrameInfo.DocView;
            this.EditorType = newFrameInfo.EditorType;
            this.FrameMode = newFrameInfo.FrameMode;
            this.Hierarchy = newFrameInfo.Hierarchy;
            this.IsVisible = newFrameInfo.IsVisible;
            this.IsWaitFrame = newFrameInfo.IsWaitFrame;
            this.IsWindowTabbed = newFrameInfo.IsWindowTabbed;
            this.ItemId = newFrameInfo.ItemId;
            this.PhysicalView = newFrameInfo.PhysicalView;
            this.RDTDocData = newFrameInfo.RDTDocData;
            this.ShortCaption = newFrameInfo.ShortCaption;
            this.WindowState = newFrameInfo.WindowState;
        }
    }
}
