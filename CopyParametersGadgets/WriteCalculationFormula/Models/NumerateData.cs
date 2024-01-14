using mmOrderMarking.Enums;
using System;
using System.Globalization;

namespace mmOrderMarking.Models
{
    public abstract class NumerateData
    {
        protected NumerateData(
          ExtParameter       parameter,
          string             startValue,
          double             step,
          PrefixSuffixSource prefixSuffixSource,
          ExtParameter       prefixParameter,
          ExtParameter       suffixParameter,
          string             prefixParameterDelimiter,
          string             suffixParameterDelimiter,
          string             prefix,
          string             suffix,
          OrderDirection     orderDirection)
        {
            Parameter                = parameter;
            StartValue               = startValue;
            Step                     = step;
            PrefixSuffixSource       = prefixSuffixSource;
            PrefixParameter          = prefixParameter;
            SuffixParameter          = suffixParameter;
            PrefixParameterDelimiter = prefixParameterDelimiter;
            SuffixParameterDelimiter = suffixParameterDelimiter;

            StartValueDouble         = double.Parse(startValue.Replace(',', '.'), NumberStyles.Number, (IFormatProvider)CultureInfo.InvariantCulture);

            int num = 0;
            string str1 = startValue;
            for (int index = 0; index < str1.Length && str1[index] == '0'; ++index)
                ++num;
            int count1 = num + 1;

            #region Finding delimeter
            int count2 = 0;
            if (startValue.Contains(".") && !startValue.EndsWith("."))
            {
                count2 = startValue.Substring(startValue.IndexOf('.') + 1).Length;
                this.DecimalSeparator = ".";
            }
            else if (startValue.Contains(",") && !startValue.EndsWith(","))
            {
                count2 = startValue.Substring(startValue.IndexOf(',') + 1).Length;
                this.DecimalSeparator = ",";
            }
            else
                this.DecimalSeparator = ".";

            #endregion

            if (Step < 1.0)
            {
                string str2 = Step.ToString(CultureInfo.InvariantCulture);
                int length = str2.Substring(str2.IndexOf('.') + 1).Length;
                if (length > count2)
                    count2 = length;
            }

            string str3;
            if (count2 != 0)
                str3 = "{0:" + new string('0', count1) + "." + new string('0', count2) + "}";
            else
                str3 = "{0:" + new string('0', count1) + "}";

            this.Format = str3;
            this.Prefix = prefix;
            this.Suffix = suffix;
            this.OrderDirection = orderDirection;
        }
        /// <summary>
        /// Целевой параметр для заполнения
        /// </summary>
        public ExtParameter Parameter { get; }

        public string StartValue { get; }

        public double StartValueDouble { get; }

        public double Step { get; }

        public PrefixSuffixSource PrefixSuffixSource { get; }

        public ExtParameter PrefixParameter { get; }

        public ExtParameter SuffixParameter { get; }

        public string PrefixParameterDelimiter { get; }

        public string SuffixParameterDelimiter { get; }

        public string Prefix { get; }

        public string Suffix { get; }

        public OrderDirection OrderDirection { get; }

        public string Format { get; set; }

        public string DecimalSeparator { get; set; }
    }
}
