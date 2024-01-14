using System;
using Autodesk.Revit.UI;
using NutsonApp;
using TagsGadgets;

namespace TagGadgets
{
    public static class RibbonSetting
    {
        private static readonly string PanelName = "Tags gadgets";
        public static void AddCommandToRibbon(RibbonBuilder ribbonBuilder,Type proxyCommandType)
        {
            RibbonPanel ribbonPanel=ribbonBuilder.GetPanel(PanelName);
            var ButtonsDictionary = RibbonBuilder.GetRibbonItemDictionary(ribbonPanel);

            #region Доработка марок

            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(EditTag));

            PushButtonData buttonDataEditTag = new PushButtonData(
                            nameof(EditTag),
                            "Доработка\nмарок",
                           proxyCommandType.Assembly.Location,
                           proxyCommandType.FullName);

            buttonDataEditTag.ToolTip = "Переводит марку в режим \"Со свободным концом\"\n" +
                "Выноска указывает в геометрический центр объекта";
            PushButton buttonEditTag = ribbonPanel.AddItem(buttonDataEditTag) as PushButton;
            buttonEditTag.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.arrow);
            buttonEditTag.Image= RibbonBuilder.ConvertFromBitmap(Resource.arrow16);

            buttonEditTag.ToolTip = "Переводит марку в режим \"Со свободным концом\"\n" +
                                    "Выноска указывает в геометрический центр объекта\n"+
                                    "Добавляется маленький участок выноски, что бы выноска начиналась сбоку текста"
                                      +  "\n\nРазработчик: Орешкин А.О."
                                      +  "\nversion: " + typeof(EditTag).Assembly.GetName().Version;
            #endregion

            #region Создание марок
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(CreateTags));

            PushButtonData buttonDataGroupTagging = new PushButtonData(
                            nameof(CreateTags),
                            "Создание\nмарок",
                           proxyCommandType.Assembly.Location,
                           proxyCommandType.FullName);
            buttonDataGroupTagging.ToolTip = "";
            PushButton buttonGroupTagging = ribbonPanel.AddItem(buttonDataGroupTagging) as PushButton;
            buttonGroupTagging.LargeImage =RibbonBuilder.ConvertFromBitmap(Resource.road);
            buttonGroupTagging.Image=RibbonBuilder.ConvertFromBitmap(Resource.road16);
            buttonGroupTagging.ToolTip = "Создает марки для выделенных элементов, в том числе для элементов в группах" 
                                              + "\n\nРазработчик: Орешкин А.О."
                                              + "\nversion: " + typeof(CreateTags).Assembly.GetName().Version;
            #endregion
        }
    }
}
