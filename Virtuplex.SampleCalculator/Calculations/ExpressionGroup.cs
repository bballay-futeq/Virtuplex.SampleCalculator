using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Virtuplex.SampleCalculator.Calculations
{
    /// <summary>
    /// Contains a grop of numbers that have arithmetical precednce.
    /// </summary>
    public class ExpressionGroup : IExpressionMember
    {
        /// <summary>
        /// Contains group members. Im using list as it has better performance for smaller ammount of items than hashset. 
        /// </summary>
        public List<IExpressionMember> Members { get; set; } = new List<IExpressionMember>();

        public ExpressionGroup() { }

        public ExpressionGroup(IEnumerable<IExpressionMember> members)
        {
            Members = members.ToList();
        }

        /// <summary>
        /// Creates a group from string params.
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public static ExpressionGroup FromArray(params string[] members)
        {
            var group = new ExpressionGroup();

            foreach(var member in members)
            {
                switch (member)
                {
                    case "+": group.Members.Add(new ExpressionOperator(OperationType.Add)); break;
                    case "-": group.Members.Add(new ExpressionOperator(OperationType.Subtract)); break;
                    case "*": group.Members.Add(new ExpressionOperator(OperationType.Multiply)); break;
                    case "/": group.Members.Add(new ExpressionOperator(OperationType.Divide)); break;
                    default: group.Members.Add(new ExpressionMember(member)); break;
                }
            }

            return group;
        }

        /// <summary>
        /// Calulates the value of group members.
        /// </summary>
        public BigInteger GetValue()
        {
            if (Members.Count == 0)
            {
                return 0;
            }

            if (Members.Count <= 1)
            {
                return Members[0].GetValue();
            }

            var returnValue = Members[0].GetValue();
            var currentOpertion = default(OperationType);

            for (var i = 1; i < Members.Count; i++)
            {
                var current = Members[i];
                
                if (current is ExpressionOperator)
                {
                    currentOpertion = ((ExpressionOperator)current).Type;
                }
                else
                {
                    var currentBigint = current.GetValue();

                    switch (currentOpertion)
                    {
                        case OperationType.Multiply: returnValue = returnValue * currentBigint;
                            break;
                        case OperationType.Divide:
                            {
                                if(currentBigint == 0)
                                {
                                    throw new ArgumentException("Cannot divide by zero!");
                                }

                                returnValue = returnValue / currentBigint;
                            }
                            break;
                        case OperationType.Add: returnValue = returnValue + currentBigint;
                            break;
                        case OperationType.Subtract: returnValue = returnValue - currentBigint;
                            break;
                    }
                }
            }

            return returnValue;
        }
    } 
}
