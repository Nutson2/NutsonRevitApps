using System;
using Autodesk.Revit.UI;
using NutsonApp;

namespace FamilyParameterEditor
{
    public class RibbonSetting
    {
        private static readonly string PanelName = "Families gadgets";
        public static void AddCommandToRibbon(RibbonBuilder ribbonBuilder, Type proxyCommandType)
        {
            RibbonPanel ribbonPanel = ribbonBuilder.GetPanel(PanelName);
            var ButtonsDictionary = RibbonBuilder.GetRibbonItemDictionary(ribbonPanel);

            #region Замена общих параметров в семействах в папках
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(FamilyParamEditor));

            PushButtonData buttonDataFamEditor = new PushButtonData(nameof(FamilyParamEditor),
                                                                    "Обработка\nсемейств в папке",
                            proxyCommandType.Assembly.Location,
                            proxyCommandType.FullName);
            PushButton buttonFamEditor = ribbonPanel.AddItem(buttonDataFamEditor) as PushButton;
            buttonFamEditor.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.cleaning);
            buttonFamEditor.Image      = RibbonBuilder.ConvertFromBitmap(Resource.cleaning16); 
            #endregion

            #region Обработка параметров семейств загруженных в модель

            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(EditorFamiliesParameters));

            PushButtonData buttonDataEditFamiliesParameters = new PushButtonData(nameof(EditorFamiliesParameters),
                            "Обработка\nпараметров семейств",
                            proxyCommandType.Assembly.Location,
                            proxyCommandType.FullName);

            PushButton buttonEditFamiliesParameters = ribbonPanel.AddItem(buttonDataEditFamiliesParameters) as PushButton;

            buttonEditFamiliesParameters.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.house_design);
            buttonEditFamiliesParameters.Image      = RibbonBuilder.ConvertFromBitmap(Resource.house_design16);
            #endregion

            #region Добавление в семейство параметров классификатора
            //ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(AddParametersToFamily));
            //PushButtonData buttonDataAddParametersToFamily = new PushButtonData(
            //                nameof(AddParametersToFamily),
            //                "Добавить параметры\nклассификатора",
            //                proxyCommandType.Assembly.Location,
            //                proxyCommandType.FullName);
            //PushButton buttonAddParametersToFamily = ribbonPanel.AddItem(buttonDataAddParametersToFamily) as PushButton;
            //buttonAddParametersToFamily.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.menu);
            //buttonAddParametersToFamily.Image = RibbonBuilder.ConvertFromBitmap(Resource.menu16);

            #endregion
        }

    }
}
