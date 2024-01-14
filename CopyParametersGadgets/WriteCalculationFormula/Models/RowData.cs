
using System.Collections.Generic;

namespace mmOrderMarking.Models
{
  public class RowData
  {
    public RowData(int rowNumber, string rowMatchValue)
    {
      RowNumber = rowNumber;
      RowMatchValue = rowMatchValue;
      Items = new List<string>();
    }

    public string RowMatchValue { get; }

    public int RowNumber { get; }

    public List<string> Items { get; }
  }
}
