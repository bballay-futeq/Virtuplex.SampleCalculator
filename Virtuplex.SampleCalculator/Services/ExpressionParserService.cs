using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Virtuplex.SampleCalculator.Calculations;

namespace Virtuplex.SampleCalculator.Services
{
    /// <summary>
    /// Parses the input file and calculats expressions into output.
    /// </summary>
    public class ExpressionParserService
    {
        ExpressionCalculatorService _calculator;

        public ExpressionParserService(ExpressionCalculatorService calculator)
        {
            _calculator = calculator;
        }

        /// <summary>
        /// Reads the input file, calculates expression and writes it into the output file. 
        /// </summary>
        /// <param name="inputFile">File to read from.</param>
        /// <param name="outputFile">File to write the reults to.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public async Task ParseExpressionFile(string inputFile, string outputFile, CancellationToken token)
        {
            try
            {
                // clear existing file
                if (File.Exists(outputFile))
                {
                    using (var fs = File.Open(outputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        lock (fs)
                        {
                            fs.SetLength(0);
                        }
                    }
                }
                                 
                // Loads streams and writers.
                using (var inputStream = File.OpenRead(inputFile))
                using (var outputStream = File.OpenWrite(outputFile))
                using (var input = new StreamReader(inputStream))
                using (var output = new StreamWriter(outputStream))
                { 
                    string line; 

                    var opsMap = new Dictionary<char, OperationType>()
                    {
                        {'+', OperationType.Add },
                        {'-', OperationType.Subtract },
                        {'*', OperationType.Multiply },
                        {'/', OperationType.Divide }
                    };

                    // To prevent loading the file in the memory, read it one line after another.
                    while ((line = await input.ReadLineAsync()) != null)
                    {
                        token.ThrowIfCancellationRequested();

                        // If the line is empty then continue.
                        if (string.IsNullOrWhiteSpace(line))
                        { 
                            continue;
                        }

                        var expression = new List<ExpressionMember>();
                        var currentnumber = new StringBuilder();
                        var pendingOperator = false;
                        var hasError = false;

                        line = line.Replace(" ", "");

                        // Parses the string expression and checks for invalid characters or invalid operations (e.g. 2*/3, etc..)
                        for (var i = 0; i < line.Length; i++)
                        {
                            // Check if is number
                            if (char.IsNumber(line[i]))
                            {
                                currentnumber.Append(line[i]);

                                pendingOperator = false;

                                if(i == line.Length - 1)
                                {
                                    expression.Add(new ExpressionMember(currentnumber.ToString()));
                                }

                                continue;
                            }

                            //Check if operator 
                            if (opsMap.ContainsKey(line[i]))
                            {
                                var op = opsMap[line[i]];

                                if(op == OperationType.Subtract && i == 0)
                                {
                                    currentnumber.Append("-");
                                    
                                    continue;
                                }

                                // check for duplicit operators. only possible valid scenario is something like 2*-3.
                                // 2**3 would be invalid. 
                                if(pendingOperator)
                                {
                                    if(op == OperationType.Subtract)
                                    {
                                        currentnumber.Append("-");
                                    }
                                    else
                                    {
                                        await output.WriteLineAsync($"Error - Invalid character: '{line[i]}'");
                                        hasError = true;

                                        break;
                                    }
                                }
                                else
                                {
                                    expression.Add(new ExpressionMember(currentnumber.ToString()));

                                    currentnumber = new StringBuilder(); 

                                    expression.Add(new ExpressionOperator(op));

                                    pendingOperator = true;
                                }

                                continue;
                            }
                             
                            // No allowed charfound, write an error and break loop.
                            await output.WriteLineAsync($"Error - Invalid character: '{line[i]}'");
                            hasError = true;

                            break;
                        }                        

                        // If there's no error, calculate the expression.
                        if (!hasError)
                        { 
                            await output.WriteLineAsync(_calculator.Calculate(expression).ToString());
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Operation cancelled by user.");
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unable to create file: {e.Message}");
            }
        }
    }
}
