using System;
using Autodesk.Revit.UI;
using MEPGadgets.Scheme;
using NutsonApp;

namespace MEPGadgets
{
    public static class RibbonSetting
    {
        private static readonly string PanelName = "MEP gadgets";
        public static void AddCommandToRibbon(RibbonBuilder ribbonBuilder,Type proxyCommandType)
        {
            RibbonPanel ribbonPanel=ribbonBuilder.GetPanel(PanelName);
            var ButtonsDictionary = RibbonBuilder.GetRibbonItemDictionary(ribbonPanel);

            #region Присоединение элементов

            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(ConnectTo));

            PushButtonData buttonDataConnectTo = new PushButtonData(nameof(ConnectTo),
                                                                    "Присоединение\nэлементов",
                                                                   proxyCommandType.Assembly.Location,
                                                                   proxyCommandType.FullName);

            buttonDataConnectTo.ToolTip = "Соединяет элементы коннекторами";
            PushButton buttonConnectTo = ribbonPanel.AddItem(buttonDataConnectTo) as PushButton;
            buttonConnectTo.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.ConnectTo);
            buttonConnectTo.Image= RibbonBuilder.ConvertFromBitmap(Resource.ConnectTo16);
            buttonConnectTo.ToolTip = "Первый элемент - элемент который будет смещен для присоединения;\n" + 
                                      "Второй элемент - элемент к которому будет присоединен первый элемент."
                                      +  "\n\nРазработчик: Орешкин А.О."
                                      +  "\nversion: " + typeof(ConnectTo).Assembly.GetName().Version;
            #endregion

            #region Отсоединить элементы
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(DisconnectElement));

            PushButtonData buttonDataDisconnectElement = new PushButtonData(nameof(DisconnectElement),
                                                                            "Отсоединить\nэлементы",
                                                                           proxyCommandType.Assembly.Location,
                                                                           proxyCommandType.FullName);
            buttonDataDisconnectElement.ToolTip = "Отсоединяет коннекторы элемента";
            PushButton buttonDisconnectElement = ribbonPanel.AddItem(buttonDataDisconnectElement) as PushButton;
            buttonDisconnectElement.LargeImage =RibbonBuilder.ConvertFromBitmap(Resource.Disconnect);
            buttonDisconnectElement.Image=RibbonBuilder.ConvertFromBitmap(Resource.Disconnect16);
            buttonDisconnectElement.ToolTip = "Отсоединяет все коннекторы у выбранных элементов;\n" 
                                              + "\n\nРазработчик: Орешкин А.О."
                                              + "\nversion: " + typeof(DisconnectElement).Assembly.GetName().Version;
            #endregion

            #region Соединение приборов с трубами
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(ConnectAppliances));

            PushButtonData buttonDataConnectAppliances= new PushButtonData(nameof(ConnectAppliances),
                                                                            "Присоединить\nприборы",
                                                                            proxyCommandType.Assembly.Location,
                                                                            proxyCommandType.FullName);
            buttonDataConnectAppliances.ToolTip = "Присоединить приборы к трубам";
            PushButton buttonConnectAppliances=ribbonPanel.AddItem(buttonDataConnectAppliances) as PushButton;
            buttonConnectAppliances.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.toilet);
            buttonConnectAppliances.Image= RibbonBuilder.ConvertFromBitmap(Resource.toilet16);
            buttonConnectAppliances.ToolTip="При активации необходимо выделить группу приборов и подводящих труб.\n" +
                                            "Коннекторы приборов будут соединены к ближайшей трубе того же типа системы."
                                              + "\n\nРазработчик: Орешкин А.О."
                                              + "\nversion: " + typeof(ConnectAppliances).Assembly.GetName().Version;
            #endregion

            #region Создание гидравлической схемы
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(HydraulicScheme));

            PushButtonData buttonDataHydraulicScheme = new PushButtonData(nameof(HydraulicScheme),
                                                                            "Гидравлическая\nсхема",
                                                                            proxyCommandType.Assembly.Location,
                                                                            proxyCommandType.FullName);
            buttonDataHydraulicScheme.ToolTip = "Построить гидравлическую схему";
            PushButton buttonHydraulicScheme = ribbonPanel.AddItem(buttonDataHydraulicScheme) as PushButton;
            buttonHydraulicScheme.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.tree);
            buttonHydraulicScheme.Image = RibbonBuilder.ConvertFromBitmap(Resource.tree16);
            buttonHydraulicScheme.ToolTip = "При активации необходимо выбрать ввод или выпуск системы\n" +
                                            "Убедитесь что система не имеет разрывов"
                                              + "\n\nРазработчик: Орешкин А.О."
                                              + "\nversion: " + typeof(HydraulicScheme).Assembly.GetName().Version;
            #endregion

            #region Создание видов с фильтрами по имени системы
            ribbonBuilder.CompleteRemoveExistButton(ButtonsDictionary, PanelName, nameof(CreateMEPSystemFilters));

            PushButtonData buttonDataCreateMEPSystemFilters = new PushButtonData(nameof(CreateMEPSystemFilters),
                                                                            "Создать виды\nпо системам",
                                                                            proxyCommandType.Assembly.Location,
                                                                            proxyCommandType.FullName);
            buttonDataHydraulicScheme.ToolTip = "Создает 3д виды с фильтрами по системам";
            PushButton buttonCreateMEPSystemFilters = ribbonPanel.AddItem(buttonDataCreateMEPSystemFilters) as PushButton;
            buttonCreateMEPSystemFilters.LargeImage = RibbonBuilder.ConvertFromBitmap(Resource.water_pipe);
            buttonCreateMEPSystemFilters.Image = RibbonBuilder.ConvertFromBitmap(Resource.water_pipe16);
            buttonCreateMEPSystemFilters.ToolTip = "Создает 3д виды с фильтрами по системам" 
                                              + "\n\nРазработчик: Орешкин А.О."
                                              + "\nversion: " + typeof(CreateMEPSystemFilters).Assembly.GetName().Version;
            #endregion

        }
    }
}
