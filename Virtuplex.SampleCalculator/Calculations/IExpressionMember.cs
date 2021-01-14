using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Virtuplex.SampleCalculator.Calculations
{
    public interface IExpressionMember
    {
        BigInteger GetValue();
    }
}
