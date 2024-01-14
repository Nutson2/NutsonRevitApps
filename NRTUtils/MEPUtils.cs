using System;
using Autodesk.Revit.DB;

namespace NRPUtils.MEPUtils
{
    public static class MEPUtils
    {
        /// <summary>
        /// Возвращает ConnectorManager для заданного элемента, используя свойство MEPModel для экземпляра семейства
        /// или напрямую, если заданный элемент является трубой или воздуховодом
        /// </summary>

        public static ConnectorManager GetConnectorManager(Element e)
        {
            MEPCurve mc = e as MEPCurve;
            FamilyInstance fi = e as FamilyInstance;

            if (null == mc && null == fi) { return null; }

            return null == mc?
                  fi.MEPModel.ConnectorManager:
                  mc.ConnectorManager;
        }

        /// <summary>
        /// Возвращает ближайший к заданной точке соединитель из набора соединителей
        /// </summary>
        public static Connector GetConnectorClosestTo(ConnectorSet connectors, XYZ p)
        {
            Connector targetConnector = null;
            double minDist = double.MaxValue;

            foreach (Connector c in connectors)
            {
                double d = c.Origin.DistanceTo(p);

                if (d >= minDist || c.IsConnected) continue;
                targetConnector = c;
                minDist = d;
            }
            return targetConnector;
        }

        /// <summary>
        /// Соединяет два заданных элемента в точке p.
        /// </summary>
        /// <exception cref="ArgumentException">Возникает, если один из элементов не имеет соединителей
        /// </exception>
        public static void Connect(XYZ p, Element a, Element b)
        {
            ConnectorManager cm = GetConnectorManager(a);
            if (null == cm) throw new ArgumentException("Элемент А не имеет соединителей.");
            Connector ca = GetConnectorClosestTo(cm.Connectors, p);

            cm = GetConnectorManager(b);
            if (null == cm) throw new ArgumentException(" Элемент В не имеет соединителей.");
            Connector cb = GetConnectorClosestTo(cm.Connectors, p);

            ca.ConnectTo(cb);
            //cb.ConnectTo( ca );
        }

    }
}
