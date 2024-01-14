using Autodesk.Revit.UI;
using CopyParametersGadgets.Command;
using NutsonApp;
using System;

namespace CopyParametersGadgets
{
    public class RibbonSetting
    {
        private static readonly string PanelName = "Copy parameters value";
        public static void AddCommandToRibbon(RibbonBuilder ribbonBuilder, Type proxyCommandType)
        {
            RibbonPanel ribbonPanel = ribbonBuilder.GetPanel(PanelName);
            var ButtonsDictionary = RibbonBuilder.GetRibbonItemDictionary(ribbonPanel);

            #region Копирование в изоляцию
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(CopyParametersToIsolation));

            PushButtonData buttonDataCopyParametersIsolation = new PushButtonData(
                                                                    nameof(CopyParametersToIsolation),
                                                                    "Копировать \nиз элемента \nв изоляцию",
                                                                    proxyCommandType.Assembly.Location,
                                                                    proxyCommandType.FullName);
            PushButton buttonCopyParametersIsolation = ribbonPanel.AddItem(buttonDataCopyParametersIsolation) as PushButton;
            buttonCopyParametersIsolation.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.tubes);
            buttonCopyParametersIsolation.Image      = RibbonBuilder.ConvertFromBitmap(Resource.tubes16);
            buttonCopyParametersIsolation.ToolTip    = "Позволяет скопировать значения параметров из элемента в его изоляцию"
                                                    + "\n\nРазработчик: Орешкин А.О."
                                                    + "\nversion: " + typeof(CopyParametersToIsolation).Assembly.GetName().Version;
            #endregion

            #region Копировать во вложенные
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(CopyParametersToSubFamilies));

            PushButtonData buttonDataCopyParametersToSubFamilies = new PushButtonData(
                                                                    nameof(CopyParametersToSubFamilies),
                                                                    "Копировать \nво вложеные \nсемейства",
                                                                    proxyCommandType.Assembly.Location,
                                                                    proxyCommandType.FullName);
            PushButton buttonCopyParametersToSubFamilies = ribbonPanel.AddItem(buttonDataCopyParametersToSubFamilies) as PushButton;
            buttonCopyParametersToSubFamilies.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.matryoshka);
            buttonCopyParametersToSubFamilies.Image = RibbonBuilder.ConvertFromBitmap(Resource.matryoshka16);
            buttonCopyParametersToSubFamilies.ToolTip = "Позволяет скопировать значения параметров из элемента в вложенные семейства"
                                                    + "\n\nРазработчик: Орешкин А.О."
                                                    + "\nversion: " + typeof(CopyParametersToSubFamilies).Assembly.GetName().Version;

            #endregion

            #region Копировать в элементы внутри габаритов
            //ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary,PanelName, nameof(CopyParametersToSelectedElement));

            //PushButtonData buttonDataCopyParametersToSelectedElement = new PushButtonData(
            //                                                        nameof(CopyParametersToSelectedElement),
            //                                                        "Копировать параметры\nв элементы внутри",
            //                                                        proxyCommandType.Assembly.Location,
            //                                                        proxyCommandType.FullName);
            //PushButton buttonCopyParametersToSelectedElement = ribbonPanel.AddItem(buttonDataCopyParametersToSelectedElement) as PushButton;
            //buttonCopyParametersToSelectedElement.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.CopyToElementInside);
            //buttonCopyParametersToSelectedElement.Image = RibbonBuilder.ConvertFromBitmap(Resource.CopyToElementInside16);
            //buttonCopyParametersToSelectedElement.ToolTip = "Определяет элементы находящиеся внутри геометрии выбранного элемента и копирует в них значения выбранных параметров"
            //                                        + "\n\nРазработчик: Орешкин А.О."
            //                                        + "\nversion: " + typeof(CopyParametersToSelectedElement).Assembly.GetName().Version;
            #endregion

            #region Копировать значения для спецификаций
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(WriteValueForSchedule));

            PushButtonData buttonDataWriteValueForSchedule = new PushButtonData(
                                                                    nameof(WriteValueForSchedule),
                                                                    "Копировать значения\nдля спецификаций",
                                                                    proxyCommandType.Assembly.Location,
                                                                    proxyCommandType.FullName);
            PushButton buttonWriteValueForSchedule = ribbonPanel.AddItem(buttonDataWriteValueForSchedule) as PushButton;
            buttonWriteValueForSchedule.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.schedule);
            buttonWriteValueForSchedule.Image = RibbonBuilder.ConvertFromBitmap(Resource.schedule16);
            buttonWriteValueForSchedule.ToolTip = "---"
                                                    + "\n\nРазработчик: Орешкин А.О."
                                                    + "\nversion: " + typeof(WriteValueForSchedule).Assembly.GetName().Version;
            #endregion

            #region Копировать номера листов в элементы
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(WriteIntoElementSheetOwner));

            PushButtonData buttonDataWriteElementSheetOwner = new PushButtonData(
                                                                    nameof(WriteIntoElementSheetOwner),
                                                                    "Вписать в элементы\nлисты размещения",
                                                                    proxyCommandType.Assembly.Location,
                                                                    proxyCommandType.FullName);
            PushButton buttonWriteElementSheetOwner = ribbonPanel.AddItem(buttonDataWriteElementSheetOwner) as PushButton;
            buttonWriteElementSheetOwner.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.technical_drawing);
            buttonWriteElementSheetOwner.Image = RibbonBuilder.ConvertFromBitmap(Resource.technical_drawing16);
            buttonWriteElementSheetOwner.ToolTip = "---"
                                                    + "\n\nРазработчик: Орешкин А.О."
                                                    + "\nversion: " + typeof(WriteIntoElementSheetOwner).Assembly.GetName().Version;
            #endregion

            #region Заполнить параметр Формула расчета
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(WriteCalculationFormula));

            PushButtonData buttonDataWriteCalculationFormula = new PushButtonData(
                                                                    nameof(WriteCalculationFormula),
                                                                    "Заполнить\nО_Формула расчета",
                                                                    proxyCommandType.Assembly.Location,
                                                                    proxyCommandType.FullName);
            PushButton buttonWriteCalculationFormula = ribbonPanel.AddItem(buttonDataWriteCalculationFormula) as PushButton;
            buttonWriteCalculationFormula.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.calculation);
            buttonWriteCalculationFormula.Image = RibbonBuilder.ConvertFromBitmap(Resource.calculation16);
            buttonWriteCalculationFormula.ToolTip = "---"
                                                    + "\n\nРазработчик: Орешкин А.О."
                                                    + "\nversion: " + typeof(WriteCalculationFormula).Assembly.GetName().Version;
            #endregion

            #region Заполнить параметры из справочника
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(FillParametersFromCatalog));

            PushButtonData buttonDataFillParametersFromCatalog = new PushButtonData(
                                                                    nameof(FillParametersFromCatalog),
                                                                    "Заполнить из\nсправочника",
                                                                    proxyCommandType.Assembly.Location,
                                                                    proxyCommandType.FullName);
            PushButton buttonFillParametersFromCatalog = ribbonPanel.AddItem(buttonDataFillParametersFromCatalog) as PushButton;

            buttonFillParametersFromCatalog.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.handbook);
            buttonFillParametersFromCatalog.Image      = RibbonBuilder.ConvertFromBitmap(Resource.handbook16);
            buttonFillParametersFromCatalog.ToolTip    = "---"
                                                        + "\n\nРазработчик: Орешкин А.О."
                                                        + "\nversion: " + typeof(FillParametersFromCatalog).Assembly.GetName().Version;
            #endregion

        }

    }
}
