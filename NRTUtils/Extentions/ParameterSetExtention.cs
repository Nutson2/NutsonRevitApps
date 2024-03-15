using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace NRPUtils.Extentions
{
    public static class ParameterSetExtention
    {
        public static List<Parameter> ToList(this ParameterSet paramSet)
        {
            int UpperBound = 10000;
            int i = default;
            List<Parameter> result = new List<Parameter>();
            var enumerator = paramSet.GetEnumerator();

            while (enumerator.MoveNext() && i < UpperBound)
            {
                result.Add(enumerator.Current as Parameter);
                i++;
            }
            return result;
        }
    }

    public static class StringExtention
    {
        public static int ToInt(this string str)
        {
            int.TryParse(str, out var result);
            return result;
        }
    }
}
