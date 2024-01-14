using Autodesk.Revit.DB;

namespace NRPUtils.Extentions
{
    public static class ElementExtention
    {
        public static bool CopyValueBetweenParameters(this Element el, string FromParamName, string ToParamName)
        {
            var ToParam = el.LookupParameter(ToParamName);
            if (ToParam==null || ToParam.IsReadOnly) return false;

            var FromParam = el.LookupParameter(FromParamName);
            if (FromParam==null) return false;

            ToParam.Set(FromParam.AsString());
            return true;
        }
        public static bool CopyValueBetweenParameters(this Element el, BuiltInParameter FromParamName, string ToParamName)
        {
            var ToParam = el.LookupParameter(ToParamName);
            if (ToParam == null || ToParam.IsReadOnly) return false;

            var FromParam = el.get_Parameter(FromParamName);
            if (FromParam == null) return false;

            ToParam.Set(FromParam.AsString());
            return true;
        }
        public static bool CopyValueBetweenElements(this Element el,Element FromElement, string FromParamName, string ToParamName)
        {
            var ToParam = el.LookupParameter(ToParamName);
            if (ToParam==null || ToParam.IsReadOnly) return false;

            var FromParam = FromElement.LookupParameter(FromParamName);
            if (FromParam==null) return false;

            ToParam.Set(FromParam.AsString());
            return true;
        }
        public static bool CopyValueBetweenElements(this Element el, Element FromElement, BuiltInParameter FromParamName, string ToParamName)
        {
            var ToParam = el.LookupParameter(ToParamName);
            if (ToParam == null || ToParam.IsReadOnly) return false;

            var FromParam = FromElement.get_Parameter(FromParamName);
            if (FromParam == null) return false;

            ToParam.Set(FromParam.AsString());
            return true;
        }

    }
}
