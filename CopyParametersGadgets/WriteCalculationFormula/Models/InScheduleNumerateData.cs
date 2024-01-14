using mmOrderMarking.Enums;

namespace mmOrderMarking.Models
{
  public class InScheduleNumerateData : NumerateData
  {
    public InScheduleNumerateData(
      ExtParameter parameter,
      string startValue,
      double step,
      PrefixSuffixSource prefixSuffixSource,
      ExtParameter prefixParameter,
      ExtParameter suffixParameter,
      string prefixParameterDelimiter,
      string suffixParameterDelimiter,
      string prefix,
      string suffix,
      OrderDirection orderDirection,
      bool processSelectedRows)
      : base(parameter, startValue, step, prefixSuffixSource, prefixParameter, suffixParameter, prefixParameterDelimiter, suffixParameterDelimiter, prefix, suffix, orderDirection)
    {
      this.ProcessSelectedRows = processSelectedRows;
    }

    public bool ProcessSelectedRows { get; }
  }
}
