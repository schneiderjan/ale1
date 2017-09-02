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

    //tautology
    //|(>(p,q),>(q,p)) 

    //fix problems with hash
    //|((|(A,(B)),C)
    //&(&(p,q),~(p))

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
        private readonly IGraphVizService _graphVizService;
        private readonly IFileService _fileService;
        private ObservableCollection<string> _truthTable = new ObservableCollection<string>();
        private string _prefix;
        private string _infix;
        private string _distinctVariables;
        private ExpressionModel _expressionModel;
        private GraphVizFileModel _graphVizFileModel;
        private string _hash;
        private ObservableCollection<string> _simplifiedTruthTable = new ObservableCollection<string>();
        private string _disjunctiveNormalForm;

        public string DisjunctiveNormalForm
        {
            get { return _disjunctiveNormalForm; }
            set { _disjunctiveNormalForm = value; RaisePropertyChanged(); }
        }

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
            set { _hash = value; RaisePropertyChanged(); }
        }

        public string Infix
        {
            get { return ExpressionModel.Infix; }
            set { _infix = value; RaisePropertyChanged(); }
        }

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

        public ObservableCollection<string> SimplifiedTruthTable
        {
            get { return _simplifiedTruthTable; }
            set { _simplifiedTruthTable = value; RaisePropertyChanged(); }
        }

        public MainViewModel(IFixConversionService fixConversionService, ITruthTableService truthTableService, IGraphVizService graphVizService, IFileService fileService)
        {
            ParsePrefixCommand = new RelayCommand(ParsePrefix, ParseCanExecute);

            //FOR DEBUGGING
            _expressionModel = new ExpressionModel();
            Prefix = "|((|(A,(B)),C)";
            //Prefix = "&((|(A,~(B)),C)";
            ExpressionModel.Prefix = "|((|(A,(B)),C)";
            //ExpressionModel.Prefix = "&((|(A,~(B)),C)";

            _fixConversionService = fixConversionService;
            _truthTableService = truthTableService;
            _graphVizService = graphVizService;
            _fileService = fileService;
        }

        private bool ParseCanExecute()
        {
            return !string.IsNullOrEmpty(_prefix);
        }

        private void ParsePrefix()
        {
            //Conversion to Infix and Distinct variables of expression
            Infix = _fixConversionService.ParsePrefix(ExpressionModel);
            _fixConversionService.GetDistinctVariables(ExpressionModel);

            //Display Graph: create GraphVizFileModel, write to dot-file, create and open png-file of graph
            _graphVizFileModel = _graphVizService.ConvertExpressionModelToGraphVizFile(_expressionModel);
            _fileService.WriteGraphVizFileToDotFile(_graphVizFileModel.Lines);
            _graphVizService.DisplayGraph();

            //Create string for expressionModel.Binary with distinct values
            string distinctVariables = null;
            foreach (var c in _expressionModel.DistinctVariables)
            {
                distinctVariables += c + " ";
            }
            DistinctVariables = distinctVariables;

            //Build truth table
            TruthTable = new ObservableCollection<string>(_truthTableService.GetTruthTable(_expressionModel));
            Hash = _truthTableService.CalculateHash(_expressionModel);

            //Simplification of truth table
            SimplifiedTruthTable = new ObservableCollection<string>(_truthTableService.SimplifyTruthTable(_expressionModel));

            DisjunctiveNormalForm = _truthTableService.GetDisjunctiveNormalForm(_expressionModel);
        }

    }
}