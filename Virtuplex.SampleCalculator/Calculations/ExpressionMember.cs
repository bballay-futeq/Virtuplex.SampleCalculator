using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Virtuplex.SampleCalculator.Calculations
{
    /// <summary>
    ///  Represents a part of mathematical expression
    /// </summary>
    public class ExpressionMember: IExpressionMember
    {
        /// <summary>
        /// Value of the expression.
        /// </summary>
        public string Value { get; protected set; }

        public ExpressionMember(string value)
        {
            Value = value;
        }

        public BigInteger GetValue()
        {
            return BigInteger.Parse(this.Value);
        }
    }
}
