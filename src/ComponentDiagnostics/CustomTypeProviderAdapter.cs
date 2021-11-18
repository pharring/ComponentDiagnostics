using System;
using System.ComponentModel;
using System.Reflection;
using System.Linq;

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

        private Type CustomType => typeProvider.GetCustomType();

        public CustomTypeProviderAdapter(ICustomTypeProvider typeProvider) => this.typeProvider = typeProvider;

        public AttributeCollection GetAttributes() => new AttributeCollection(null);

        public string GetClassName() => CustomType.Name;

        public string GetComponentName() => null;

        public TypeConverter GetConverter() => null;

        public EventDescriptor GetDefaultEvent() => null;

        public PropertyDescriptor GetDefaultProperty() => null;

        public object GetEditor(Type editorBaseType) => null;

        public EventDescriptorCollection GetEvents(Attribute[] attributes) => null;

        public EventDescriptorCollection GetEvents() => null;

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) => GetProperties();

        public PropertyDescriptorCollection GetProperties()
        {
            var result = from p in CustomType.GetProperties() select new AdapterPropertyDescriptor(p);
            return new PropertyDescriptorCollection(result.ToArray<PropertyDescriptor>());
        }

        public object GetPropertyOwner(PropertyDescriptor pd) => typeProvider;

        class AdapterPropertyDescriptor : PropertyDescriptor
        {
            private readonly PropertyInfo propertyInfo;

            public AdapterPropertyDescriptor(PropertyInfo propertyInfo) : base(propertyInfo.Name, null)
            {
                this.propertyInfo = propertyInfo;
            }

            public override bool CanResetValue(object component) => false;

            public override Type ComponentType => propertyInfo.DeclaringType;

            public override object GetValue(object component) => propertyInfo.GetValue(component);

            public override bool IsReadOnly => !propertyInfo.CanWrite;

            public override Type PropertyType => propertyInfo.PropertyType;

            public override void ResetValue(object component) => throw new NotImplementedException();

            public override void SetValue(object component, object value) => propertyInfo.SetValue(component, value);

            public override bool ShouldSerializeValue(object component) => false;
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (typeProvider is INotifyPropertyChanged innerEventSource)
                {
                    innerEventSource.PropertyChanged += value;
                }
            }
            remove
            {
                if (typeProvider is INotifyPropertyChanged innerEventSource)
                {
                    innerEventSource.PropertyChanged -= value;
                }
            }
        }
    }
}
