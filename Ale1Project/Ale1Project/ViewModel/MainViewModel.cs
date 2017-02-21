using GalaSoft.MvvmLight;
using Ale1Project.Model;
using Ale1Project.Service;
using GalaSoft.MvvmLight.CommandWpf;

namespace Ale1Project.ViewModel
{
    //"&((|(A,~(B)),C)"
    //&(|(A,B),>(C,~(&(|(D,E),>(&(|(F,G),>(H,~(&(|(I,J),>(K,~(L)))))),~(M))))))
    //"&(=(A,B),|(C,D))"
    //"&((|(A,~(B)),C)"
    //"=( >(A,B), |( ~(A) ,B) ) 
    //&(A, ~(B))
    //&(=(A,B),|(C,D))
    //~(&(~(&(A,C)),~(&(~(B),C))))

    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IFixConversionService _fixConversionService;
        private string _prefix;
        private string _infix;
        private string _distinctVariables;
        private ExpressionModel _expressionModel;

        public string Prefix
        {
            get { return _prefix; }
            set
            {
                _prefix = value;
                ExpressionModel.Prefix = value;
                RaisePropertyChanged();
                ParsePrefixCommand.RaiseCanExecuteChanged();
            }
        }

        public string Infix { get { return ExpressionModel.Infix; } set { _infix = value; RaisePropertyChanged(); } }

        public ExpressionModel ExpressionModel
        {
            get { return _expressionModel; }
            set { _expressionModel = value; RaisePropertyChanged(); }
        }

        public RelayCommand ParsePrefixCommand { get; set; }

        public string DistinctVariables
        {
            get { return _distinctVariables; }
            set { _distinctVariables = value; RaisePropertyChanged(); }
        }

        public MainViewModel(IFixConversionService fixConversionService)
        {
            ParsePrefixCommand = new RelayCommand(ParsePrefix, ParseCanExecute);

            //FOR DEBUGGING
            _expressionModel = new ExpressionModel();
            Prefix = "&((|(A,~(B)),C)";
            ExpressionModel.Prefix = "&((|(A,~(B)),C)";

            _fixConversionService = fixConversionService;
        }

        private bool ParseCanExecute()
        {
            return !string.IsNullOrEmpty(_prefix);
        }

        private void ParsePrefix()
        {
            Infix = _fixConversionService.ParsePrefix(ExpressionModel);
            _fixConversionService.GetDistinctVariables(ExpressionModel);

            //create string for binding with distinct values
            string distinctVariables = null;
            foreach (var c in _expressionModel.DistinctVariables)
            {
                distinctVariables += c + " ";
            }
            DistinctVariables = distinctVariables;
        }
    }
}