using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Virtuplex.SampleCalculator.Calculations;
using Virtuplex.SampleCalculator.Services;

namespace Virtuplex.SampleCalculator.Tests
{
    public class Tests
    {
        ExpressionCalculatorService calculator = new ExpressionCalculatorService();
        ExpressionParserService eps;
        Dictionary<ExpressionGroup, BigInteger> values = new Dictionary<ExpressionGroup, BigInteger>();

        [SetUp]
        public void Setup()
        {
            values = new Dictionary<ExpressionGroup, BigInteger>()
            {
                { ExpressionGroup.FromArray("2", "*", "4", "-","3"), new BigInteger(5) },
                { ExpressionGroup.FromArray("6", "+", "3", "*","2"), new BigInteger(12) },
                { ExpressionGroup.FromArray("7", "-", "8", "/","4"), new BigInteger(5) }
            };

            eps = new ExpressionParserService(calculator);
        }

        [Test]
        public void CalculateExpression()
        {
            foreach (var group in values.Keys)
            {
                var value = calculator.Calculate(group.Members.Cast<ExpressionMember>().ToList());

                Assert.AreEqual(values[group], value);
            }

            Assert.Pass();
        }

        [Test]
        public async Task CalculateExpressionFromFile()
        {
            var inputFileName = "test-input.txt";
            var outputFileName = "test-output.txt";
            var sampleInput = new string[]
            {
                "2 + -3 * 2",
                "a + 1",
                "2 / 3",
                "",
                "2.1*2"
            };
            var expectedOutput = new string[]
            {
                "-4",
                "Error - Invalid character: 'a'",
                "0",
                "Error - Invalid character: '.'"
            };

            await File.WriteAllLinesAsync(inputFileName, sampleInput);

            await eps.ParseExpressionFile(inputFileName, outputFileName, CancellationToken.None);

            // check output 
            var output = await File.ReadAllLinesAsync(outputFileName);

            // check if it contains same amount of lines
            if (output.Length != expectedOutput.Length)
            {
                Assert.Fail("Output file has different size than expected.");
            }

            for (var i = 0; i < output.Length; i++)
            {
                if (output[i] != expectedOutput[i])
                {
                    Assert.AreEqual(expectedOutput[i], output[i]);
                }
            }

            Assert.Pass();
        }
    }
}