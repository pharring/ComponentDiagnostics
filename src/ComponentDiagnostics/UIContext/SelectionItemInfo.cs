using System;
using System.Windows;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    public class SelectionItemInfo : DependencyObject
    {
        public static DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(String), typeof(SelectionItemInfo));
        public static DependencyProperty SelElemNameProperty = DependencyProperty.Register("SelElem", typeof(String), typeof(SelectionItemInfo));
        public static DependencyProperty ContextOwnerProperty = DependencyProperty.Register("ContextOwner", typeof(String), typeof(SelectionItemInfo));
        public static DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(DateTime), typeof(SelectionItemInfo));

        internal enum SpecialElement
        {
            Hierarchy = 1000,
            ItemID = 1001,
            SelectionContainer = 1002,
            MultiItemSelect = 1003
        }


        public SelectionItemInfo(VSConstants.SelectionElement selElemID, string selElemName, string description, string owner)
        {
            SelElemID = selElemID;
            SelElemName = selElemName;
            Description = description;
            ContextOwner = owner;
            TimeStamp = DateTime.Now;
        }

        public SelectionItemInfo(VSConstants.SelectionElement selElemID, string description, string owner)
            : this(selElemID, GetSelElemName(selElemID), description, owner)
        {
        }

        /// <summary>
        /// Get the name of a selection element.  The first 7 are avalable as constants
        /// in VSConstants.SelectionElement.
        /// </summary>
        /// <param name="selelem"></param>
        /// <returns></returns>
        static string GetSelElemName(VSConstants.SelectionElement selelem)
        {
            switch (selelem)
            {
                case VSConstants.SelectionElement.DocumentFrame:
                case VSConstants.SelectionElement.PropertyBrowserSID:
                case VSConstants.SelectionElement.StartupProject:
                case VSConstants.SelectionElement.UndoManager:
                case VSConstants.SelectionElement.UserContext:
                case VSConstants.SelectionElement.WindowFrame:
                case VSConstants.SelectionElement.ResultList:
                case VSConstants.SelectionElement.LastWindowFrame:
                    return selelem.ToString();

                // Special made up values to indicate the other items of selection
                case (VSConstants.SelectionElement)SpecialElement.Hierarchy:
                    return "Hierarchy";
                case (VSConstants.SelectionElement)SpecialElement.ItemID:
                    return "ItemID";
                case (VSConstants.SelectionElement)SpecialElement.SelectionContainer:
                    return "SelectionContainer";
                case (VSConstants.SelectionElement)SpecialElement.MultiItemSelect:
                    return "MultiItemSelect";
                default:
                    return "VSIP Registered Context";

            }
        }

        public DateTime TimeStamp
        {
            get
            {
                return DateTime.Now;
            }
            set
            {
                SetValue(TimeProperty, value);
            }
        }


        public VSConstants.SelectionElement SelElemID { get; set; }
        public String SelElemName
        {
            get
            {
                return (string)GetValue(SelElemNameProperty);
            }
            set
            {
                SetValue(SelElemNameProperty, value);
            }
        }

        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }
            set
            {
                SetValue(DescriptionProperty, value);
            }
        }
        
        public string ContextOwner
        {
            get
            {
                return (string)GetValue(ContextOwnerProperty);
            }
            set
            {
                SetValue(ContextOwnerProperty, value);
            }
        }

    }
}
