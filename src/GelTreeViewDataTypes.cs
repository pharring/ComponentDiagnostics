﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

// This namespace contains types that are used in the data model for the TreeView in the Gel view control.
namespace GelTreeViewDataTypes
{
    static class Utilities
    {
        /// <summary>
        /// Given a property value from a Gel data source, create the appropriate
        /// tree-view node.
        /// </summary>
        /// <param name="gelType">The Gel type of the object</param>
        /// <param name="value">The object's value</param>
        /// <returns>The newly created treeview node</returns>
        public static object CreateAppropriateNode(string gelType, object value)
        {
            switch (gelType)
            {
                case VsUIType.DataSource:
                    IVsUIDataSource ds = value as IVsUIDataSource;
                    if (ds == null) return new NullDataSource();
                    return new DataSourceNode(ds);

                case VsUIType.Collection:
                    IVsUICollection coll = value as IVsUICollection;
                    if (coll == null) return new NullCollection();
                    return new CollectionNode(coll);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Given an unknown data model, create the appropriate root node - either
        /// a collection or a data source.
        /// </summary>
        /// <param name="value">The data model</param>
        /// <returns>The appropriate treeview node or null if it's not a Gel object</returns>
        public static object CreateAppropriateRootNode(object value)
        {
            IVsUIDataSource ds = value as IVsUIDataSource;
            if (ds != null)
            {
                return new DataSourceNode(ds);
            }

            IVsUICollection coll = value as IVsUICollection;
            if (coll != null)
            {
                return new CollectionNode(coll);
            }

            return null;
        }
    }

    /// <summary>
    /// Distinct type to represent a null Gel IVsUIDataSource
    /// </summary>
    class NullDataSource
    {
    }

    /// <summary>
    /// Distinct type to represent a null Gel IVsUICollection
    /// </summary>
    class NullCollection
    {
    }

    /// <summary>
    /// Node representing the verbs in a Gel data source or collection
    /// </summary>
    class VerbList
    {
        IVsUISimpleDataSource ds;

        public VerbList(IVsUISimpleDataSource ds)
        {
            this.ds = ds;
        }

        List<string> verbs;
        [Description("The list of verbs in this data source.")]
        public IList<string> Verbs
        {
            get
            {
                if (verbs == null)
                {
                    IVsUIEnumDataSourceVerbs enumVerbs;
                    Marshal.ThrowExceptionForHR(this.ds.EnumVerbs(out enumVerbs));
                    verbs = new List<string>(ComUtilities.EnumerableFrom(enumVerbs));
                    this.ds = null;
                }

                return verbs;
            }
        }

        [Description("The number of verbs in this data source.")]
        public int Count
        {
            get
            {
                return this.Verbs.Count;
            }
        }
    }

    /// <summary>
    /// Node representing a single property in a Gel datasource
    /// </summary>
    class Property
    {
        public Property(VsUIPropertyDescriptor desc, __VSUIDATAFORMAT format, object value)
        {
            this.Name = desc.name;
            this.Type = desc.type;
            this.Format = format;
            this.Value = value;
        }

        [Description("The name of this property.")]
        public string Name { get; private set; }
        [Description("The Gel type of this property.")]
        public string Type { get; private set; }
        [Description("The Gel data format of the current value of this property.")]
        public __VSUIDATAFORMAT Format { get; private set; }
        [Description("The value of this property.")]
        public object Value { get; private set; }

        /// <summary>
        /// Most properties are 'leaf nodes', but DataSource and Collection have children.
        /// To cope with that, the "Property" type is bound to a HierarchicalDataTemplate
        /// which may have empty "Items". If the item is a simple built-in type like "bool,
        /// int, string, etc.", then the Children enumerator is empty. Otherwise, we have
        /// a single child node containing the root of the property's Gel object value.
        /// </summary>
        [Browsable(false)]
        public IEnumerable Children
        {
            get
            {
                object node = Utilities.CreateAppropriateNode(this.Type, this.Value);
                if(node!=null)
                {
                    yield return node;
                }
            }
        }
    }

    /// <summary>
    /// Node representing all the properties in a Gel data source
    /// </summary>
    class PropertyList
    {
        IVsUIDataSource ds;

        public PropertyList(IVsUIDataSource ds)
        {
            this.ds = ds;
        }

        List<Property> properties;
        [Description("The list of properties in this data source.")]
        public IList<Property> Properties
        {
            get
            {
                if (properties == null)
                {
                    IVsUIEnumDataSourceProperties enumProperties;
                    Marshal.ThrowExceptionForHR(this.ds.EnumProperties(out enumProperties));
                    properties = new List<Property>();
                    foreach (VsUIPropertyDescriptor desc in ComUtilities.EnumerableFrom(enumProperties))
                    {
                        uint[] formats = new uint[1];
                        object[] values = new object[1];
                        Marshal.ThrowExceptionForHR(this.ds.QueryValue(desc.name, null /*types*/, formats, values));

                        Property prop = new Property(desc, (__VSUIDATAFORMAT)formats[0], values[0]);
                        properties.Add(prop);
                    }
                }

                return properties;
            }
        }

        [Description("The number of properties in this data source.")]
        public int Count
        {
            get
            {
                return this.Properties.Count;
            }
        }
    }

    /// <summary>
    /// Node representing the shape (Guid:DWORD) of a data source
    /// </summary>
    class Shape
    {
        [Description("The GUID part of the shape identifier.")]
        public Guid Guid { get; private set; }

        [Description("The DWORD part of the shape identifier.")]
        public uint Id { get; private set; }

        public Shape(IVsUIDataSource ds)
        {
            Guid guid;
            uint dword;
            Marshal.ThrowExceptionForHR(ds.GetShapeIdentifier(out guid, out dword));
            this.Guid = guid;
            this.Id = dword;
        }

        public override string ToString()
        {
            return string.Format("{0}:0x{1:x8}", this.Guid, this.Id);
        }
    }

    /// <summary>
    /// Node representing a data source
    /// </summary>
    class DataSourceNode
    {
        IVsUIDataSource ds;

        public DataSourceNode(IVsUIDataSource ds)
        {
            if (ds == null) throw new ArgumentNullException("ds");
            this.ds = ds;
        }

        [Description("The concrete managed type of the selected data source")]
        public string TypeName
        {
            get
            {
                if (Marshal.IsComObject(ds))
                {
                    return "IVsUIDataSource RCW";
                }
                return ds.GetType().ToString();
            }
        }

        [Browsable(false)]
        public IEnumerable Children
        {
            get
            {
                if (ds != null)
                {
                    yield return Shape;
                    
                    VerbList verbs = new VerbList(this.ds);
                    if (verbs.Count != 0)
                    {
                        yield return verbs;
                    }

                    PropertyList properties = new PropertyList(this.ds);
                    if (properties.Count != 0)
                    {
                        yield return properties;
                    }
                }
            }
        }


        private Shape _shape;

        [Description("The shape identifier uniquely identifies this arrangement of verbs and properties. All instances of data sources with this shape must have the same collection of verbs and properties. Expressed as a GUID:DWORD pair.")]
        public Shape Shape
        {
            get
            {
                if (_shape == null)
                {
                    _shape = new Shape(this.ds);
                }
                return _shape;
            }
        }
    }

    /// <summary>
    /// Node representing the items in a Gel collection
    /// </summary>
    class CollectionItems
    {
        IVsUICollection coll;
        public CollectionItems(IVsUICollection coll)
        {
            if (coll == null) throw new ArgumentNullException("coll");
            this.coll = coll;
        }

        [Description("The number of items in the collection.")]
        public uint Count
        {
            get
            {
                uint count;
                this.coll.get_Count(out count);
                return count;
            }
        }

        public IEnumerable Items
        {
            get
            {
                uint count;
                this.coll.get_Count(out count);
                for (uint i = 0; i != count; ++i)
                {
                    IVsUIDataSource item;
                    this.coll.GetItem(i, out item);
                    yield return new CollectionItem(i, item);
                }
            }
        }
    }

    /// <summary>
    /// Node representing a Gel collection
    /// </summary>
    class CollectionNode
    {
        IVsUICollection coll;

        public CollectionNode(IVsUICollection coll)
        {
            this.coll = coll;
        }

        [Description("The concrete managed type of this collection object")]
        public string TypeName
        {
            get
            {
                if (Marshal.IsComObject(this.coll))
                {
                    if (this.coll is IVsUIDynamicCollection)
                    {
                        return "IVsUIDynamicCollection RCW";
                    }

                    return "IVsUICollection RCW";
                }
                return this.coll.GetType().ToString();
            }
        }

        [Description("Whether the collection may be modified externally")]
        public bool IsDynamic
        {
            get
            {
                return this.coll is IVsUIDynamicCollection;
            }
        }

        [Browsable(false)]
        public IEnumerable Children
        {
            get
            {
                VerbList verbs = new VerbList(this.coll);
                if (verbs.Count != 0)
                {
                    yield return verbs;
                }

                yield return new CollectionItems(this.coll);
            }
        }
    }

    /// <summary>
    /// Node representing a single item from a Gel collection
    /// </summary>
    class CollectionItem : DataSourceNode
    {
        public CollectionItem(uint i, IVsUIDataSource ds) : base(ds)
        {
            this.Index = i;
        }

        [Description("The index of this item in the containing collection.")]
        public uint Index { get; private set; }
    }
}
