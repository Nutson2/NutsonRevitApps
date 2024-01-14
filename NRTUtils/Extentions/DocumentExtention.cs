using Autodesk.Revit.DB;
using System;

namespace MEPGadgets
{
    public static class DocumentExtention
    {
        public static void DoInTransaction(this Document document, string TransactionName, Action action )
        {
            using var tr = new Transaction(document, TransactionName);
            tr.Start();
            action.Invoke();
            tr.Commit();
        }
    }
}
