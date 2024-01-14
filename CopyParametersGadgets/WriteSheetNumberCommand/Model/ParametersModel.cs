using Autodesk.Revit.DB;
using NRPUtils.MVVMBase;
using NRPUtils.Extentions;

namespace CopyParametersGadgets.Model
{
    public class ParametersModel : NotifyObject
    {
        private Parameter parameter;
        protected string  name;
        protected string  _value;
        protected string  suffix;
        protected string  prefix;

        public string Owner  { get; set; }
        public string Name   { get => name; set { name = value; OnPropertyChanged(); } }
        public string Value  { get => _value; set { _value = value; OnPropertyChanged(); } }
        public string Suffix { get => suffix; set { suffix = value; OnPropertyChanged(); } }
        public string Prefix { get => prefix; set { prefix = value; OnPropertyChanged(); } }

        public Parameter Parameter { get => parameter; set 
                                                            {
                                                                parameter=value;
                                                                Name=parameter.Definition.Name;
                                                                Value=parameter.TryAsString();
                                                                OnPropertyChanged();
                                                            } }

        public ParametersModel(Parameter _parameter)
        {
            Parameter= _parameter;
        }

    }
}
