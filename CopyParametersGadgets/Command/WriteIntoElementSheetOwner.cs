using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CopyParametersGadgets.Model;
using CopyParametersGadgets.VM;
using CopyParametersGadgets.WriteSheetNumberCommand.View;
using NRPUtils.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CopyParametersGadgets.Command
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    public class WriteIntoElementSheetOwner : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var UIDoc = commandData.Application.ActiveUIDocument;

            var VM = new VMWriteSheetNumber(UIDoc.Document);
            var view = new ViewWriteSheetNumber(VM);
            LoadUserSettings(VM);
            view.ShowDialog();

            SaveUserSettings(VM);
            return Result.Succeeded;
        }
        #region Save/Load settings
        private void SaveUserSettings(VMWriteSheetNumber VM)
        {
            var settings=Properties.Settings.Default;
            settings.SheetOwnerCategories = new System.Collections.Specialized.StringCollection();
            VM.Categories.Where(x => x.Selected)
                .ToList()
                .ForEach(x => settings.SheetOwnerCategories.Add(x.Category.Name));

            if (VM.ParamForWrite != null) settings.SheetOwnerParameterForWrite = VM.ParamForWrite.Name;

            settings.SheetOwnerSheets = new System.Collections.Specialized.StringCollection();
            VM.Sheets.First()
                .GetSelectedSubNodes()
                .ForEach(x => settings.SheetOwnerSheets.Add(x.Name));

            settings.SheetOwnerParameters = new System.Collections.Specialized.StringCollection();
            VM.NewStringParts.ToList()
                .ForEach(x => settings.SheetOwnerParameters.Add(string.Join("/",
                    VM.NewStringParts.IndexOf(x),
                    x.Owner,
                    x.Parameter.Id,
                    x.Prefix,
                    x.Suffix)));

            settings.Save();
        }
        private void LoadUserSettings(VMWriteSheetNumber VM)
        {
            var settings=Properties.Settings.Default;
            if (settings.SheetOwnerCategories != null ||
                settings.SheetOwnerCategories?.Count > 0)
            {
                VM.Categories.
                    ForEach(x =>
                    {
                        if (settings.SheetOwnerCategories.Contains(x.Category.Name))
                            x.Selected = true;
                    });
            }
            if (!string.IsNullOrEmpty(settings.SheetOwnerParameterForWrite))
            {
                VM.ParamForWrite = VM.AllowedParametersFromSelectedCategories
                    .Where(x => x.Name == settings.SheetOwnerParameterForWrite)
                    .FirstOrDefault();
            }
            if (settings.SheetOwnerSheets != null || settings.SheetOwnerSheets?.Count > 0)
            {
                foreach (Node<ViewSheet> sheetNode in VM.Sheets.First())
                {
                    if (sheetNode == null) continue;
                    if (settings.SheetOwnerSheets.Contains(sheetNode.Name))
                        sheetNode.Selected = true;
                }
            }
            if (settings.SheetOwnerParameters != null || settings.SheetOwnerParameters?.Count > 0)
            {
                List<string[]> values= new List<string[]>();
                foreach (var row in settings.SheetOwnerParameters)
                {
                    var splitrow= row.Split("/".ToArray(), StringSplitOptions.None);
                    values.Add(splitrow);
                    Node < ParametersModel > matchNode=default;
                    foreach (Node<ParametersModel> node in VM.ProjectAndSheetParameters.First())
                    {
                        if (node.Item == null) continue;
                        if (node.Item.Owner + node.Item.Parameter.Id == splitrow[1] + splitrow[2])
                        {
                            matchNode = node;
                            break;
                        }

                    }
                    if (matchNode == null) continue;

                    matchNode.Item.Prefix = splitrow[3];
                    matchNode.Item.Suffix = splitrow[4];
                    VM.NewStringParts.Add(matchNode.Item);
                }
                if (VM.NewStringParts.Count <= 0) return;
                for (int i = 0; i < values.Count - 1; i++)
                {
                    var item = values[i];
                    if (item.Length < 2) continue;
                    var el=VM.NewStringParts.Where(x => x.Parameter.Id.ToString()==item[2]).FirstOrDefault();
                    VM.NewStringParts.Move(VM.NewStringParts.IndexOf(el), i);

                }
            }
        }
        #endregion
    }
}