using System;
using System.Collections.Generic;
using System.Text;

namespace Virtuplex.SampleCalculator.Calculations
{
    public enum OperationType
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    /// <summary>
    /// Represents an arithmetic operation.
    /// </summary>
    public class ExpressionOperator : ExpressionMember
    {
        /// <summary>
        /// Returns the type of the operation.
        /// </summary>
        public OperationType Type { get; protected set; }

        public ExpressionOperator(OperationType type): base(string.Empty)
        {
            Type = type;

            switch(type)
            {
                case OperationType.Add:
                    Value = "+"; break;
                case OperationType.Subtract:
                    Value = "-"; break;
                case OperationType.Divide:
                    Value = "÷"; break;
                case OperationType.Multiply:
                    Value = "*"; break;
            }
        }
    }
}
