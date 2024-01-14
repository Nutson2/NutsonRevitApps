using Autodesk.Revit.DB;
using System;


namespace CopyParametersGadgets
{
    public class DataParametersM
    {
        public string    StorageType { get; set; }
        public Guid      Guid        { get; set; }
        public string    Name        { get; set; }
        public bool      AppendValue { get; set; }
        public Parameter Parameter   { get; set; }

        public DataParametersM()
        {
            AppendValue = false;
        }
    }
}
