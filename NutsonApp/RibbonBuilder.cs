using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using AdskWind = Autodesk.Windows;

namespace NutsonApp
{
    public class RibbonBuilder
    {
        public readonly UIControlledApplication UICApp;
        public AdskWind.RibbonTab Tab { get; set; }
        public RibbonBuilder(UIControlledApplication Revit, string TabName)
        {
            UICApp = Revit;

            Tab = AdskWind.ComponentManager.Ribbon.Tabs.FirstOrDefault(x => x.Name == TabName);
            if (Tab == null) Revit.CreateRibbonTab(TabName);
            Tab = AdskWind.ComponentManager.Ribbon.Tabs.FirstOrDefault(x => x.Name == TabName);
        }

        public RibbonPanel GetPanel(string Name)
        {
            RibbonPanel panel=null;
            var panels = UICApp.GetRibbonPanels(Tab.Name);

            if (panels.Count > 0)
                panel = panels.FirstOrDefault(x => x.Name == Name);

            return panel ?? UICApp.CreateRibbonPanel(Tab.Name, Name);
        }
        public AdskWind.RibbonPanelSource TryGetPanelSource(string RibbonName)
        {
            if (Tab == null || Tab.Panels.Count == 0) return null;
            var ribP = Tab.Panels.Where(x => x.Source.Name == RibbonName).Select(x => x.Source).FirstOrDefault();
            if (ribP == null || ribP.Items.Count == 0) return null;
            return ribP;
        }

        public static AdskWind.RibbonItem TryGetPanelItem(string ButtonName, AdskWind.RibbonPanelSource ribP)
        {
            return ribP?.Items.Where(x => x.Id.Contains(ButtonName)).FirstOrDefault();
        }

        public static BitmapSource ConvertFromBitmap(Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        public static Dictionary<string, RibbonItem> GetRibbonItemDictionary(RibbonPanel ribbonPanel)
        {
            return ribbonPanel.GetType()
                               .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                               .FirstOrDefault(x => x.Name == "m_ItemsNameDictionary")
                               .GetValue(ribbonPanel) as Dictionary<string, RibbonItem>;
        }

        public void CompleteRemoveExistButton(Dictionary<string, RibbonItem> ribbonItems, 
                                                                    string panelName,
                                                                    string buttonName)
        {
            var panelSource=TryGetPanelSource(panelName );
            var item=TryGetPanelItem(buttonName, panelSource);
            if (item != null) panelSource.Items.Remove(item);

            if (ribbonItems.ContainsKey(buttonName))
                ribbonItems.Remove(buttonName);
        }
    }

}
