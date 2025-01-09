using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FamilyParameterEditor.EditFamiliesParameters.ViewModel
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AddParametersToFamily : IExternalCommand
    {
        private const BuiltInParameterGroup pG_TEXT = BuiltInParameterGroup.PG_TEXT;

        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements
        )
        {
            var famDoc = commandData.Application.ActiveUIDocument.Document;
            if (!famDoc.IsFamilyDocument)
                return Result.Cancelled;

            var FM = famDoc.FamilyManager;

            var famParamName = new string[]
            {
                "LT_SPE",
                "id_SystemName",
                "id_category",
                "SystemName",
                "category",
            };
            var famParamType = new ParameterType[]
            {
                ParameterType.Text,
                ParameterType.Integer,
                ParameterType.Integer,
                ParameterType.Text,
                ParameterType.Text,
            };
            List<FamilyParameter> newparams = new List<FamilyParameter>();

            var sharedParamNames = new string[]
            {
                "SPE_Code_Category",
                "SPE_Code_Classification",
                "SPE_Code_Description",
                "SPE_Code_SystemType",
            };
            var extDef = GetExternalDefinitions(famDoc, sharedParamNames);

            using (var tr = new Transaction(famDoc, "AddParam"))
            {
                tr.Start();
                for (int i = 0; i < famParamName.Length; i++)
                {
                    newparams.Add(FM.AddParameter(famParamName[i], pG_TEXT, famParamType[i], true));
                }

                for (int i = 0; i < extDef.Length; i++)
                {
                    newparams.Add(FM.AddParameter(extDef[i], pG_TEXT, true));
                }

                tr.Commit();
            }

            CreateSizeTable(famDoc);

            var dictFormulas = new Dictionary<string, string>()
            {
                { "LT_SPE", "\"Классификатор\"" },
                {
                    "SystemName",
                    "size_lookup(LT_SPE, \"SystemName\", \"Нет в каталоге\", id_SystemName, id_category)"
                },
                {
                    "category",
                    "size_lookup(LT_SPE, \"category\", \"Нет в каталоге\", id_SystemName, id_category)"
                },
                {
                    "SPE_Code_Category",
                    "size_lookup(LT_SPE, \"SPE_Code_Category\", \"Нет в каталоге\", id_SystemName, id_category)"
                },
                {
                    "SPE_Code_Classification",
                    "size_lookup(LT_SPE, \"SPE_Code_Classification\", \"Нет в каталоге\", id_SystemName, id_category)"
                },
                {
                    "SPE_Code_Description",
                    "size_lookup(LT_SPE, \"SPE_Code_Description\", \"Нет в каталоге\", id_SystemName, id_category)"
                },
                {
                    "SPE_Code_SystemType",
                    "size_lookup(LT_SPE, \"SPE_Code_SystemType\", \"Нет в каталоге\", id_SystemName, id_category)"
                },
            };

            using (var tr = new Transaction(famDoc, "addFormulas"))
            {
                tr.Start();
                foreach (var fparam in newparams)
                {
                    if (!dictFormulas.ContainsKey(fparam.Definition.Name))
                        continue;
                    FM.SetFormula(fparam, dictFormulas[fparam.Definition.Name]);
                }
                tr.Commit();
            }

            return Result.Succeeded;
        }

        private static void CreateSizeTable(Document famDoc)
        {
            System.Windows.Forms.OpenFileDialog fod = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "csv files | *.csv",
            };
            if (fod.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            var path = fod.FileName;

            try
            {
                using (var tr = new Transaction(famDoc, "addLT"))
                {
                    tr.Start();
                    FamilySizeTableManager.CreateFamilySizeTableManager(
                        famDoc,
                        famDoc.OwnerFamily.Id
                    );
                    var famSizeTablemanager = FamilySizeTableManager.GetFamilySizeTableManager(
                        famDoc,
                        famDoc.OwnerFamily.Id
                    );
                    FamilySizeTableErrorInfo errorInfo = new FamilySizeTableErrorInfo();

                    famSizeTablemanager.ImportSizeTable(famDoc, path, errorInfo);

                    tr.Commit();
                }
            }
            catch (Exception) { }
        }

        private ExternalDefinition[] GetExternalDefinitions(Document document, string[] paramNames)
        {
            var SHF = document.Application.OpenSharedParameterFile();
            var gr = SHF.Groups.Where(x => x.Name == "16 Классификатор").FirstOrDefault();
            if (gr == null)
                return null;
            return gr
                .Definitions.Where(x => paramNames.Contains(x.Name))
                .Cast<ExternalDefinition>()
                .ToArray();
        }
    }
}
