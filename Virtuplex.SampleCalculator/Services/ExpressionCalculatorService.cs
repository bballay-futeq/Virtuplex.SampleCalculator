using Microsoft.CodeAnalysis.CSharp.Scripting;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;
using Virtuplex.SampleCalculator.Calculations;
using Microsoft.CodeAnalysis.Scripting;
using System.Threading.Tasks;

namespace Virtuplex.SampleCalculator.Services
{
    public class ExpressionCalculatorService
    {
        /// <summary>
        /// Calculates the result of expression. 
        /// </summary>
        /// <param name="expressionMembers">All parts of expression.</param>
        /// <returns></returns>
        public async Task<BigInteger> Calculate(List<ExpressionMember> expressionMembers)
        {
            if (expressionMembers.Count == 0)
            {
                return BigInteger.Zero;
            }

            if (expressionMembers.Count == 1)
            {
                return BigInteger.Parse(expressionMembers[0].Value);
            }

            var groups = GroupMembers(expressionMembers);
            var resultGroup = new ExpressionGroup()
            {
                Members = groups
            };

            await Task.CompletedTask;

            var result = resultGroup.GetValue();

            return result;
        }

        /// <summary>
        /// Group operations with arithmetic precedence. 
        /// </summary>
        /// <param name="expressionMembers"></param>
        /// <returns></returns>
        private List<IExpressionMember> GroupMembers(List<ExpressionMember> expressionMembers)
        {
            var result = new List<IExpressionMember>();
            var currentGroup = new ExpressionGroup();

            foreach (var m in expressionMembers)
            {
                if (m is ExpressionOperator)
                {
                    var op = (ExpressionOperator)m;

                    if (op.Type == OperationType.Add || op.Type == OperationType.Subtract)
                    {
                        result.Add(currentGroup);
                        result.Add(op);

                        currentGroup = new ExpressionGroup();
                    }
                    else
                    {
                        currentGroup.Members.Add(op);
                    }
                }
                else
                {
                    currentGroup.Members.Add(m);
                }
            }

            if (currentGroup.Members.Count > 0)
            {
                result.Add(currentGroup);
            }

            return result;
        }
    }
}
