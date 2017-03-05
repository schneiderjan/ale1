using System.Collections.ObjectModel;
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
    /// This class contains properties that the main View can data expressionModel.Binaryd to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IFixConversionService _fixConversionService;
        private readonly ITruthTableService _truthTableService;
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

        public string Hash
        {
            get { return _hash; }
            set { _hash = value; RaisePropertyChanged();}
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

        public ObservableCollection<string> TruthTable
        {
            get { return _truthTable; }
            set { _truthTable = value; RaisePropertyChanged(); }
        }
        private ObservableCollection<string> _truthTable = new ObservableCollection<string>();
        private string _hash;


        public MainViewModel(IFixConversionService fixConversionService, ITruthTableService truthTableService)
        {
            ParsePrefixCommand = new RelayCommand(ParsePrefix, ParseCanExecute);

            //FOR DEBUGGING
            _expressionModel = new ExpressionModel();
            Prefix = "&((|(A,~(B)),C)";
            ExpressionModel.Prefix = "&((|(A,~(B)),C)";

            _fixConversionService = fixConversionService;
            _truthTableService = truthTableService;
        }

        private bool ParseCanExecute()
        {
            return !string.IsNullOrEmpty(_prefix);
        }

        private void ParsePrefix()
        {
            Infix = _fixConversionService.ParsePrefix(ExpressionModel);
            _fixConversionService.GetDistinctVariables(ExpressionModel);

            //create string for expressionModel.Binaryding with distinct values
            string distinctVariables = null;
            foreach (var c in _expressionModel.DistinctVariables)
            {
                distinctVariables += c + " ";
            }
            DistinctVariables = distinctVariables;

            TruthTable = new ObservableCollection<string>(_truthTableService.GetTruthTable(_expressionModel));
            Hash = _truthTableService.CalculateHash(_expressionModel);
        }

    }
}