using Autodesk.Revit.DB;
using NRPUtils.MVVMBase;
namespace mmOrderMarking.Models
{
    public class ExtParameter : NotifyObject
    {
        private bool _isInstanceParameter;

        public ExtParameter(Parameter parameter)
        {
            if (parameter == null)
            {
                Name = "no";
                Parameter =  null;
                IsVisibleDescription = false;
            }
            else
            {
                Name = parameter.Definition.Name;
                Parameter = parameter;
                IsNumeric = parameter.StorageType != StorageType.String;
                IsVisibleDescription = true;
            }
        }

        public string Name { get; }

        public bool IsVisibleDescription { get; }

        public bool IsInstanceParameter
        {
            get => _isInstanceParameter;
            set
            {
                if (_isInstanceParameter == value) return;

                _isInstanceParameter = value;
                OnPropertyChanged(nameof(IsInstanceParameter));
                OnPropertyChanged("IsTypeParameter");
            }
        }

        public bool IsTypeParameter => !this.IsInstanceParameter;

        public string Description
        {
            get
            {
                return nameof(Description);
            }
        }

        public bool IsNumeric { get; }

        public Parameter Parameter { get; }

        public bool IsMatchBuiltInParameter(BuiltInParameter builtInParameter) => this.Parameter.Definition is InternalDefinition definition &&
                                                                                definition.BuiltInParameter == builtInParameter;

        public static bool IsValid(Parameter parameter) => !parameter.IsReadOnly &&
                                                            (parameter.StorageType == StorageType.String ||
                                                             parameter.StorageType == StorageType.Integer &&
                                                             parameter.Definition.ParameterType != ParameterType.YesNo ||
                                                             parameter.StorageType == StorageType.Double);

        public static bool IsValidForPrefixAndSuffix(Parameter parameter) => parameter.StorageType != StorageType.None;

        public Parameter GetSameParameter(Element element)
        {
            if (IsInstanceParameter)
                return GetSameParameter(element, Parameter);
            Element element1 = element.Document.GetElement(element.GetTypeId());
            return element1 != null ? GetSameParameter(element1, Parameter) : null;
        }

        private static Parameter GetSameParameter(Element element, Parameter parameter)
        {
            if (parameter.IsShared)
                return element.get_Parameter(parameter.GUID);
            return parameter.Definition is InternalDefinition definition && definition.BuiltInParameter != BuiltInParameter.INVALID ?
                                                                        element.get_Parameter(definition.BuiltInParameter) :
                                                                        element.LookupParameter(parameter.Definition.Name);
        }
    }
}
