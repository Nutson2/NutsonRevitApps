using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FamilyParameterEditor
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class FamilyParamEditor : IExternalCommand
    {
        private UIDocument uidoc;
        private Document doc;
        Config config;
        private FamilyParamEditorForm familyParamEditorForm;

        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements
        )
        {
            UIApplication application = commandData.Application;
            uidoc = application.ActiveUIDocument;
            doc = uidoc.Document;

            config = new Config();

            familyParamEditorForm = new FamilyParamEditorForm(doc);
            familyParamEditorForm.btnOk.Click += Ok_Click;
            familyParamEditorForm.ShowDialog();
            return (Result)0;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Transaction tr = new Transaction(doc);
            try
            {
                tr.Start("Обработка семейств");

                string selectedPath = familyParamEditorForm.tbx1.Text;
                List<string> familiesOnSelectedPath = Directory
                    .GetFiles(selectedPath, "*.rfa", SearchOption.AllDirectories)
                    .ToList();
                var num = 0;
                foreach (string familyFile in familiesOnSelectedPath)
                {
                    Document familyDoc = doc.Application.OpenDocumentFile(familyFile);
                    FamilyWork(familyDoc, familyDoc);

                    familyDoc = doc.Application.OpenDocumentFile(familyFile);
                    FamilyAddParameter(familyDoc, familyDoc);

                    familyDoc = doc.Application.OpenDocumentFile(familyFile);
                    Transaction subTransaction = new Transaction(familyDoc);

                    try
                    {
                        subTransaction.Start("Удаление общих параметров в семействе");
                        FilteredElementCollector collector = new FilteredElementCollector(
                            familyDoc
                        );
                        IList<Element> existSharedParameters = collector
                            .OfClass(typeof(SharedParameterElement))
                            .ToElements();

                        foreach (
                            ParameterElement existSharedParam in existSharedParameters.Cast<ParameterElement>()
                        )
                        {
                            ExternalDefinition parameters = GetValidSharedParam(
                                existSharedParam.Name
                            );

                            if (
                                parameters != null
                                || IsNeedsConvertToProjectParam(existSharedParam.Name)
                            )
                            {
                                try
                                {
                                    familyDoc.Delete(existSharedParam.Id);
                                }
                                catch { }
                            }
                        }
                        subTransaction.Commit();
                        familyDoc.Close(true);
                    }
                    finally
                    {
                        subTransaction.Dispose();
                    }

                    for (int i = 0; i < 9; i++)
                    {
                        var tmpFileName = familyFile.Replace(
                            ".rfa",
                            ".000" + i.ToString() + ".rfa"
                        );
                        if (!File.Exists(tmpFileName))
                            continue;
                        File.Delete(tmpFileName);
                    }
                    num++;
                    familyParamEditorForm.lb_Status.Text = string.Format(
                        "Обработано {0} семейств. Всего {1}\n" + "Текущий файл:{2}",
                        num,
                        familiesOnSelectedPath.Count,
                        Path.GetFileName(familyFile)
                    );
                }
                tr.Commit();
            }
            finally
            {
                tr.Dispose();
            }
        }

        private void FamilyAddParameter(Document familydoc, Document ParentFamilydoc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(familydoc);
            IList<Element> families = collector.OfClass(typeof(Family)).ToElements();

            foreach (Family family in families.Cast<Family>())
            {
                if (family.IsEditable)
                {
                    Document curFamilyDoc = familydoc.EditFamily(family);
                    FamilyWork(curFamilyDoc, ParentFamilydoc); // Дублирующий запуск?
                    DeleteOldSharedParameters(curFamilyDoc);
                    curFamilyDoc.LoadFamily(familydoc, new FamilyLoadOptions());
                }
            }
            if (ParentFamilydoc != familydoc)
                return;

            DeleteOldSharedParameters(familydoc);
            ParentFamilydoc.Close(true);
        }

        private void FamilyWork(Document familydoc, Document ParentFamilydoc)
        {
            ConvertAllSharedParametersToFamilyParameter(ref familydoc);
            FilteredElementCollector collector = new FilteredElementCollector(familydoc);
            IList<Element> families = collector.OfClass(typeof(Family)).ToElements();

            foreach (Family family in families.Cast<Family>())
            {
                if (!family.IsEditable)
                    continue;
                try
                {
                    Document subFamily = familydoc.EditFamily(family);
                    FamilyWork(subFamily, ParentFamilydoc);
                    subFamily.LoadFamily(familydoc, new FamilyLoadOptions());
                }
                catch { }
            }
            if (ParentFamilydoc != familydoc)
                return;
            ParentFamilydoc.Close(true);
        }

        private void ConvertAllSharedParametersToFamilyParameter(ref Document familyDoc)
        {
            FamilyManager familyManager = familyDoc.FamilyManager;
            FamilyParameterSet parameters = familyManager.Parameters;

            foreach (FamilyParameter sharedParameter in parameters)
            {
                if (!sharedParameter.IsShared)
                    continue;

                ExternalDefinition validSharedParam = GetValidSharedParam(
                    sharedParameter.Definition.Name
                );
                bool ConvertToProjectParam = IsNeedsConvertToProjectParam(
                    sharedParameter.Definition.Name
                );

                Transaction tr = new Transaction(familyDoc);
                try
                {
                    tr.Start("Замена параметров на параметры семейства");
                    if (validSharedParam != null || ConvertToProjectParam)
                    {
                        ConvertSharedToProjectParam(familyManager, sharedParameter);
                    }
                    tr.Commit();
                }
                finally
                {
                    tr.Dispose();
                }
            }
        }

        private static void ConvertSharedToProjectParam(
            FamilyManager familyManager,
            FamilyParameter sharedParameter
        )
        {
            bool isInstance = sharedParameter.IsInstance;
            BuiltInParameterGroup parameterGroup = sharedParameter.Definition.ParameterGroup;
            string name = sharedParameter.Definition.Name;

            FamilyParameter newFamParam;
            try
            {
                newFamParam = familyManager.ReplaceParameter(
                    sharedParameter,
                    name,
                    parameterGroup,
                    isInstance
                );
            }
            catch
            {
                newFamParam = familyManager.ReplaceParameter(
                    sharedParameter,
                    name + "_ЗАМЕНА",
                    parameterGroup,
                    isInstance
                );
                try
                {
                    familyManager.RenameParameter(newFamParam, name);
                }
                catch { }
            }
        }

        private void DeleteOldSharedParameters(Document familyDoc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(familyDoc);
            IList<Element> sharedParameters = collector
                .OfClass(typeof(SharedParameterElement))
                .ToElements();

            FamilyManager familyManager = familyDoc.FamilyManager;
            FamilyParameterSet parameters = familyManager.Parameters;

            foreach (ParameterElement existSharedParam in sharedParameters.Cast<ParameterElement>())
            {
                if (!existSharedParam.IsValidObject)
                    continue;
                if (IsNeedsConvertToProjectParam(existSharedParam.Name))
                {
                    DeleteSharedParam(familyDoc, existSharedParam);
                    continue;
                }

                int num = 0;
                foreach (FamilyParameter familyParam in parameters)
                {
                    if (familyParam.Definition.Name != existSharedParam.Name)
                        continue;

                    ExternalDefinition newSharedParam = GetValidSharedParam(existSharedParam.Name);
                    if (newSharedParam == null)
                        continue;
                    DeleteSharedParam(familyDoc, existSharedParam);

                    Transaction tr = new Transaction(familyDoc);
                    try
                    {
                        bool isInstance = familyParam.IsInstance;
                        BuiltInParameterGroup parameterGroup = familyParam
                            .Definition
                            .ParameterGroup;

                        tr.Start("Добавление нового параметра и запись формулы");
                        try
                        {
                            FamilyParameter newParam = familyManager.AddParameter(
                                newSharedParam,
                                parameterGroup,
                                isInstance
                            );
                            AddFormula(familyDoc, newParam, familyParam.Definition.Name);
                        }
                        catch { }
                        num = 1;
                        tr.Commit();
                    }
                    finally
                    {
                        ((IDisposable)tr)?.Dispose();
                    }
                    break;
                }

                if (
                    num == 0
                    && existSharedParam.IsValidObject
                    && IsNeedsConvertToProjectParam(existSharedParam.Name)
                )
                {
                    DeleteSharedParam(familyDoc, existSharedParam);
                }
            }
        }

        private void DeleteSharedParam(Document familyDoc, ParameterElement param)
        {
            Transaction tr = new Transaction(familyDoc);
            try
            {
                tr.Start("Удаление общих параметров в семействе");
                familyDoc.Delete(param.Id);
                familyDoc.Regenerate();
                tr.Commit();
            }
            finally
            {
                tr.Dispose();
            }
        }

        /// <summary>
        /// Возвращает общий параметр для замены существующего общего параметра
        /// </summary>
        /// <param name="oldParameter">Существующий общий параметр под замену</param>
        /// <returns></returns>
        private ExternalDefinition GetValidSharedParam(string oldParameter)
        {
            ExternalDefinition result = null;
            int count = familyParamEditorForm.dgv1.Rows.Count;
            for (int i = 0; i < count; i++)
            {
                string existParam = familyParamEditorForm.dgv1.Rows[i].Cells[0].Value as string;
                if (existParam != oldParameter)
                    continue;

                string newParamName = familyParamEditorForm.dgv1.Rows[i].Cells[1].Value as string;
                DefinitionFile SHF = doc.Application.OpenSharedParameterFile();
                foreach (DefinitionGroup group in SHF.Groups)
                {
                    foreach (Definition definition in group.Definitions)
                    {
                        if (definition.Name != newParamName)
                            continue;

                        Definition newParam = group.Definitions.get_Item(newParamName);
                        result = (ExternalDefinition)
                            (object)((newParam is ExternalDefinition) ? newParam : null);
                        config.Write(oldParameter, newParamName);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Возвращает единицу если общий параметр нужно конвертировать в параметр проекта
        /// </summary>
        /// <param name="oldParameter"></param>
        /// <returns></returns>
        private bool IsNeedsConvertToProjectParam(string oldParameter)
        {
            bool result = false;
            int count = familyParamEditorForm.dgv1.Rows.Count;
            for (int i = 0; i < count; i++)
            {
                object flagConvertToProjectParam = familyParamEditorForm
                    .dgv1
                    .Rows[i]
                    .Cells[2]
                    .Value;
                if (flagConvertToProjectParam == null)
                    continue;

                string sharedParam = familyParamEditorForm.dgv1.Rows[i].Cells[0].Value as string;
                if (sharedParam != oldParameter)
                    continue;

                result = true;
                break;
            }
            return result;
        }

        /// <summary>
        /// Добавляет формулу в параметр
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="familyParam"></param>
        /// <param name="formula"></param>
        public void AddFormula(Document doc, FamilyParameter familyParam, string formula)
        {
            doc.Regenerate();
            FamilyManager familyManager = doc.FamilyManager;
            FamilyType currentType = familyManager.CurrentType;
            FamilyTypeSet types = familyManager.Types;

            if (types.IsEmpty || currentType.Name == " ")
            {
                FamilyType currentType2 = familyManager.NewType(doc.Title);
                familyManager.CurrentType = currentType2;
                doc.Regenerate();
            }

            if (!familyParam.CanAssignFormula)
                return;
            if (formula == "")
            {
                familyManager.SetFormula(familyParam, "1");
            }
            else
            {
                familyManager.SetFormula(familyParam, "[" + formula + "]");
            }
            if (familyParam.Formula != null && familyParam.Formula.ToString().StartsWith("1"))
            {
                familyManager.SetFormula(familyParam, string.Empty);
            }
        }
    }
}
