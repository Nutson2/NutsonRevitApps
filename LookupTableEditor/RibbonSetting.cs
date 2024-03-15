using Autodesk.Revit.UI;
using NutsonApp;
using System;

namespace LookupTableEditor
{
    public static class RibbonSetting
    {
        private static readonly string PanelName = "Families gadgets";
        public static void AddCommandToRibbon(RibbonBuilder ribbonBuilder, Type proxyCommandType)
        {
            RibbonPanel ribbonPanel = ribbonBuilder.GetPanel(PanelName);
            var ButtonsDictionary = RibbonBuilder.GetRibbonItemDictionary(ribbonPanel);

            #region Редактор таблицы выбора
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(LookupTableEditorECommand));

            PushButtonData buttonDataLookupTableEditor = new PushButtonData(
                                                            nameof(LookupTableEditorECommand),
                                                            "Редактирование\nтаблицы выбора",
                                                            proxyCommandType.Assembly.Location,
                                                            proxyCommandType.FullName);
            buttonDataLookupTableEditor.ToolTip = "";
            PushButton buttonLookupTableEditor = ribbonPanel.AddItem(buttonDataLookupTableEditor) as PushButton;
            buttonLookupTableEditor.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.LookupTableEditor);
            buttonLookupTableEditor.Image = RibbonBuilder.ConvertFromBitmap(Resource.LookupTableEditor16);
            buttonLookupTableEditor.ToolTip = "Позволяет создавать и редактировать таблицы выбора."
                                      + "\n\nРазработчик: Орешкин А.О."
                                      + "\nversion: " + typeof(LookupTableEditorECommand).Assembly.GetName().Version;
            #endregion
        }
    }
}
