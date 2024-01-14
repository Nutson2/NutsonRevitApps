using Autodesk.Revit.DB;

namespace NRPUtils.Extentions
{
    public static class ParameterExtention
    {
        public static bool TrySetValue(this Parameter parameter, ElementId value) 
        {
            if (parameter==null || parameter.IsReadOnly) return false;
            parameter.Set(value);
            return true;
        }
        public static bool TrySetValue(this Parameter parameter, double value) 
        {
            if (parameter==null || parameter.IsReadOnly) return false;
            parameter.Set(value);
            return true;
        }
        public static bool TrySetValue(this Parameter parameter, int value) 
        {
            if (parameter==null || parameter.IsReadOnly) return false;
            parameter.Set(value);
            return true;
        }
        public static bool TrySetValue(this Parameter parameter, string value) 
        {
            if (parameter==null || parameter.IsReadOnly) return false;
            parameter.Set(value);
            return true;
        }
        public static string TryAsString(this Parameter parameter)
        {
            string res = string.Empty;
            if (parameter == null) return res;
            res=parameter.AsValueString();

            if (!string.IsNullOrEmpty(res)) return res;

            switch (parameter.StorageType)
            {
                case StorageType.Integer:
                    res=parameter.AsInteger().ToString();
                    break;
                case StorageType.Double:
                    res=parameter.AsDouble().ToString();
                    break;
                case StorageType.String:
                    res=parameter.AsString();
                    break;
                case StorageType.ElementId:
                    res=parameter.Element.Document.GetElement(parameter.AsElementId()).ToString();
                    break;
                default:
                    break;
            }
            return res;
        }

        private const string DELIMETER = "|";
        public static void CopyParameterValue(Parameter curParam, Parameter donorParam, bool append = false)
        {
            switch (donorParam.StorageType.ToString())
            {
                case "String":
                {
                    string valueToWrite;
                    valueToWrite = donorParam.AsString();

                    string existValue = string.Empty;
                    if (append)
                    {
                        existValue = curParam.AsString();
                        if (!string.IsNullOrWhiteSpace(existValue)) { existValue += DELIMETER; }
                    }
                    var newValue = existValue + valueToWrite;
                    curParam.Set(newValue);
                    break;
                }

                case "Double":
                    curParam.Set(donorParam.AsDouble());
                    break;
                case "Integer":
                    curParam.Set(donorParam.AsInteger());
                    break;
                case "ElementId":
                    curParam.Set(donorParam.AsElementId());
                    break;
            }
        }

    }
}
