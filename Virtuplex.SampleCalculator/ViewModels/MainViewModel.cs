using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;

using Virtuplex.SampleCalculator.Base;
using Virtuplex.SampleCalculator.Calculations;
using Virtuplex.SampleCalculator.Services;
using Virtuplex.SampleCalculator.Common;

namespace Virtuplex.SampleCalculator.ViewModels
{
    /// <summary>
    /// View model for <see cref="MainView"/>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private string _currentValue = "0";
        private string _status;
        private bool _newExpression = false;
        private bool _isBusy = false;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private ExpressionCalculatorService _calculator;
        private ExpressionParserService _parser;

        private ICommand _inputNumberCommand;
        private ICommand _expressionCommand;
        private ICommand _delCommand;
        private ICommand _clearCommand;
        private ICommand _clearEntryCommand;
        private ICommand _calculateExpressionCommand;
        private ICommand _loadFileCommand;
        private ICommand _generateFileCommand;
        private ICommand _cancelCommand;

        /// <summary>
        /// Value currently selected on calculator.
        /// </summary>
        public string CurrentValue
        {
            get { return _currentValue; }
            set
            {
                Set(ref _currentValue, value);
            }
        }

        /// <summary>
        /// Contains members of current expression.
        /// </summary>
        public ObservableCollection<ExpressionMember> Expression { get; private set; } = new ObservableCollection<ExpressionMember>();

        /// <summary>
        /// Determines whether there's a background operation running.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                Set(ref _isBusy, value);
            }
        }

        /// <summary>
        /// Status of the current operation.
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                Set(ref _status, value);
            }
        }

        /// <summary>
        /// Output from the load & generate operations.
        /// </summary>
        public ObservableCollection<string> Output { get; private set; } = new ObservableCollection<string>();

        /// <summary>
        /// Triggered when user clicks the number button.
        /// </summary>
        public ICommand InputNumberCommand => _inputNumberCommand ?? (_inputNumberCommand = new Command<char>(OnNumberCommand));

        /// <summary>
        /// Trigged when user clicks the expression button.
        /// </summary>
        public ICommand OperationCommand => _expressionCommand ?? (_expressionCommand = new Command<OperationType>(OnOperationCommand));

        /// <summary>
        /// Serves as a backspace.
        /// </summary>
        public ICommand DelCommand => _delCommand ?? (_delCommand = new Command(OnDeleteCommand));

        /// <summary>
        /// Clears the entire expression.
        /// </summary>
        public ICommand ClearCommand => _clearCommand ?? (_clearCommand = new Command(OnClearCommand));

        /// <summary>
        /// Clears the entry.
        /// </summary>
        public ICommand ClearEntryCommand => _clearEntryCommand ?? (_clearEntryCommand = new Command(() => CurrentValue = "0"));

        /// <summary>
        /// Calculates the current expression.
        /// </summary>
        public ICommand CalculateExpressionCommand => _calculateExpressionCommand ?? (_calculateExpressionCommand = new Command(OnCalculateExpressionCommand));

        /// <summary>
        /// Genertes really large file with random operations. 
        /// </summary>
        public ICommand GenerateFileCommand => _generateFileCommand ?? (_generateFileCommand = new Command(OnGenerateFileCommand));

        /// <summary>
        /// Cancels currntly run background operation. 
        /// </summary>
        public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new Command(() => _cts.Cancel()));

        /// <summary>
        /// Loads file from disk, parses it and writes output file.
        /// </summary>
        public ICommand LoadFileCommand => _loadFileCommand ?? (_loadFileCommand = new Command(OnLoadFileCommand));


        public MainViewModel()
        {
            // RLoad services, not using standard DI to avoid having to create a VM locator.
            var keyHandlerService = App.ServiceProvider.GetRequiredService<KeyHandlerService>();
            _calculator = App.ServiceProvider.GetRequiredService<ExpressionCalculatorService>();
            _parser = App.ServiceProvider.GetRequiredService<ExpressionParserService>();

            // Register events 
            keyHandlerService.NumberKeyPressed += (s, a) => OnNumberCommand(a);
            keyHandlerService.ExpressionKeyPressed += (s, a) => OnOperationCommand(a);
            keyHandlerService.DelPressed += (s, a) => OnDeleteCommand();
            keyHandlerService.ClearPressed += (s, a) => OnClearCommand();
            keyHandlerService.EqualsPressed += (s, a) => OnCalculateExpressionCommand();
            keyHandlerService.ClearEntryPressed += (s, a) => CurrentValue = "0";
            
        }

        /// <summary>
        /// Calculates the current expression.
        /// </summary>
        private async void OnCalculateExpressionCommand()
        {
            if (CurrentValue != "0")
            {
                Expression.Add(new ExpressionMember(CurrentValue));
            }

            var result = await _calculator.Calculate(Expression.ToList());

            CurrentValue = result.ToString();

            _newExpression = true;
        }

        /// <summary>
        /// Executed when number command is triggered.
        /// </summary>
        /// <param name="number">Number passed from CommandParameter</param>
        private void OnNumberCommand(char number)
        {
            if (CurrentValue == "0")
            {
                CurrentValue = string.Empty;
            }

            if (_newExpression)
            {
                Expression.Clear();
                _newExpression = false;
            }

            CurrentValue = CurrentValue + number;
        }

        /// <summary>
        /// Handles the backspace key or Del button.
        /// </summary>
        private void OnDeleteCommand()
        {
            if (CurrentValue == "0")
            {
                return;
            }

            if (CurrentValue.Length == 1)
            {
                CurrentValue = "0";
                return;
            }

            CurrentValue = CurrentValue.Substring(0, CurrentValue.Length - 1);
        }

        /// <summary>
        /// Handles the clear command.
        /// </summary>
        private void OnClearCommand()
        {
            this.Expression.Clear();
            this.CurrentValue = "0";
        }

        /// <summary>
        /// Executed when operation is triggered. 
        /// </summary>
        /// <param name="type"></param>
        private void OnOperationCommand(OperationType type)
        {
            if (_newExpression)
            {
                Expression.Clear();
                _newExpression = false;
            }

            if (CurrentValue != "0")
            {
                Expression.Add(new ExpressionMember(this.CurrentValue));
                Expression.Add(new ExpressionOperator(type));

                CurrentValue = "0";
            }
        }

        /// <summary>
        /// Execute when generate file is triggered.
        /// </summary>
        private async void OnGenerateFileCommand()
        {
            var sfd = new SaveFileDialog()
            {
                Filter = "Text files (*.txt)|*.txt"
            };

            if (sfd.ShowDialog() == true)
            {
                IsBusy = true;
                Status = string.Empty;

                var (observable, progress) = ObservableProgress.CreateForUi<string>(TimeSpan.FromSeconds(1));

                using (observable.Subscribe((val) => Status = val))
                {
                    if (await App.ServiceProvider.GetRequiredService<FileGeneratorService>().GenerateFile(sfd.FileName, progress, _cts.Token) == true)
                    {
                        MessageBox.Show("File created!");
                    }
                }

                Status = string.Empty;
                IsBusy = false;

                _cts = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// Execute when LoadFileCommand is triggered.
        /// </summary>
        private async void OnLoadFileCommand()
        {
            var filter = "Text files (*.txt)|*.txt";
            var sfd = new SaveFileDialog()
            {
                Title = "Select input file",
                Filter = filter
            };
            var ofd = new OpenFileDialog()
            {
                Title = "Select output file",
                Filter = filter
            };

            if (ofd.ShowDialog() == true)
            {
                if (sfd.ShowDialog() == true)
                {
                    IsBusy = true;
                    Status = string.Empty;

                    await App.ServiceProvider.GetRequiredService<ExpressionParserService>().ParseExpressionFile(ofd.FileName, sfd.FileName, _cts.Token);

                    Status = string.Empty;
                    IsBusy = false;

                    MessageBox.Show("Operation complete.");

                    _cts = new CancellationTokenSource();
                }
            }
        }
    }
}
