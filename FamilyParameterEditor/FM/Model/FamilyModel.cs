using System.IO;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FamilyParameterEditor.FM.Model
{
    public partial class FamilyModel : ObservableObject
    {
        [ObservableProperty]
        private string _name;
        private string path;
        private Document familyDocument;

        public readonly Document document;
        public readonly FamilyStorageType storageType;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }
        public Family Family { get; set; }
        public Document FamilyDocument
        {
            get => GetFamDocument();
            set => familyDocument = value;
        }

        public FamilyModel(Document document, string path)
        {
            Path = path;
            Family = null;
            this.document = document;
            Name = new FileInfo(Path).Name;
            storageType = FamilyStorageType.InDirectory;
        }

        public FamilyModel(Family family)
        {
            Family = family;
            Name = Family.Name;
            storageType = FamilyStorageType.InDocument;
            document = family.Document;
        }

        public FamilyModel(Document famDocument, Family family)
        {
            Family = family;
            FamilyDocument = famDocument;
            Name = Family.Name;
            document = family.Document;
            storageType = FamilyStorageType.Opened;
        }

        private Document GetFamDocument()
        {
            familyDocument ??= FamiliesLoader.LoadFamily(this);
            return familyDocument;
        }
    }

    public enum FamilyStorageType
    {
        InDocument,
        InDirectory,
        Opened,
    }
}
