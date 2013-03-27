using System;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// The WinForms property grid cannot bind to ICustomTypeProvider, so this adapter is used
    /// to adapt ICustomTypeProvider to ICustomTypeDescriptor.
    /// This is useful because, in Dev11, Gel DataModels now implement ICustomTypeProvider.
    /// </summary>
    class CustomTypeProviderAdapter : ICustomTypeDescriptor, INotifyPropertyChanged
    {
        private readonly ICustomTypeProvider typeProvider;

        private Type CustomType
        {
            get
            {
                return this.typeProvider.GetCustomType();
            }
        }

        public CustomTypeProviderAdapter(ICustomTypeProvider typeProvider)
        {
            this.typeProvider = typeProvider;
        }

        public AttributeCollection GetAttributes()
        {
            return new AttributeCollection(null);
        }

        public string GetClassName()
        {
            return this.CustomType.Name;
        }

        public string GetComponentName()
        {
            return null;
        }

        public TypeConverter GetConverter()
        {
            return null;
        }

        public EventDescriptor GetDefaultEvent()
        {
            return null;
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        public object GetEditor(Type editorBaseType)
        {
            return null;
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return null;
        }

        public EventDescriptorCollection GetEvents()
        {
            return null;
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        public PropertyDescriptorCollection GetProperties()
        {
            var result = from p in this.CustomType.GetProperties() select new AdapterPropertyDescriptor(p);
            return new PropertyDescriptorCollection(result.ToArray<PropertyDescriptor>());
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this.typeProvider;
        }

        class AdapterPropertyDescriptor : PropertyDescriptor
        {
            PropertyInfo propertyInfo;

            public AdapterPropertyDescriptor(PropertyInfo propertyInfo) : base(propertyInfo.Name, null)
            {
                this.propertyInfo = propertyInfo;
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get { return propertyInfo.DeclaringType; }
            }

            public override object GetValue(object component)
            {
                return propertyInfo.GetValue(component);
            }

            public override bool IsReadOnly
            {
                get { return !propertyInfo.CanWrite; }
            }

            public override Type PropertyType
            {
                get { return propertyInfo.PropertyType; }
            }

            public override void ResetValue(object component)
            {
                throw new NotImplementedException();
            }

            public override void SetValue(object component, object value)
            {
                propertyInfo.SetValue(component, value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                INotifyPropertyChanged innerEventSource = this.typeProvider as INotifyPropertyChanged;
                if (innerEventSource != null)
                {
                    innerEventSource.PropertyChanged += value;
                }
            }
            remove
            {
                INotifyPropertyChanged innerEventSource = this.typeProvider as INotifyPropertyChanged;
                if (innerEventSource != null)
                {
                    innerEventSource.PropertyChanged -= value;
                }
            }
        }
    }
}
