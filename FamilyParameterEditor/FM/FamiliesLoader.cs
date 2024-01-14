using Autodesk.Revit.DB;
using FamilyParameterEditor.FM.Model;
using System.Collections.Generic;

namespace FamilyParameterEditor.FM
{
    public static class FamiliesLoader
    {
        public static Queue<Document> OpenFamilies { get; set; } = new Queue<Document>();
        private static readonly int maxCapacity = 30;

        public static Document LoadFamily(FamilyModel familyModel)
        {
            Document document = null;
            if (familyModel.storageType==FamilyStorageType.InDirectory)
                document=familyModel.document.Application.OpenDocumentFile(familyModel.Path);

            if (familyModel.storageType == FamilyStorageType.InDocument)
                document=familyModel.document.EditFamily(familyModel.Family);

            OpenFamilies.Enqueue(document);

            if(OpenFamilies.Count > maxCapacity)
            {
                Document f=OpenFamilies.Dequeue();
                f.Close(true);
                f.Dispose();
            }

            return document;
        }

    }
}
