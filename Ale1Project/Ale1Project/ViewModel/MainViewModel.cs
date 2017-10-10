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
    //~(&(~(&(A,C)),~(&(~(B),C))))
    //|((|(A,(B)),C)
    //&(&(p,q),~(p))

    //tautology
    //|(>(p,q),>(q,p)) 

    //use for simplification of truth table
    //&(=(A,B),|(C,D)) 

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
        private readonly ExpressionModel _expressionModel;
        private readonly ExpressionModel _expressionModelDisjunctiveNormalForm;
        private readonly ExpressionModel _expressionModelSimplifiedDisjunctiveNormalForm;
        private readonly ExpressionModel _expressionModelNand;


        private ObservableCollection<string> _truthTable = new ObservableCollection<string>();
        private ObservableCollection<string> _simplifiedTruthTable = new ObservableCollection<string>();

        private GraphVizFileModel _graphVizFileModel;

        private string _prefix;
        private string _infix;
        private string _distinctVariables;
        private string _hash;
        private string _disjunctiveNormalForm;
        private string _hashDisjunctiveNormalForm;
        private string _simplifiedDisjunctiveNormalForm;
        private string _hashDisjunctiveNormalFormSimplified;
        private string _nand;
        private string _hashNand;

        public string HashNand
        {
            get { return _hashNand; }
            set { _hashNand = value; RaisePropertyChanged(); }
        }

        public string Nand
        {
            get { return _nand; }
            set { _nand = value; RaisePropertyChanged(); }
        }

        public string HashDisjunctiveNormalFormSimplified
        {
            get { return _hashDisjunctiveNormalFormSimplified; }
            set { _hashDisjunctiveNormalFormSimplified = value; RaisePropertyChanged(); }
        }

        public string DisjunctiveNormalForm
        {
            get { return _disjunctiveNormalForm; }
            set { _disjunctiveNormalForm = value; RaisePropertyChanged(); }
        }

        public string SimplifiedDisjunctiveNormalForm
        {
            get { return _simplifiedDisjunctiveNormalForm; }
            set { _simplifiedDisjunctiveNormalForm = value; RaisePropertyChanged(); }
        }

        public string Prefix
        {
            get { return _prefix; }
            set
            {
                _prefix = value;
                _expressionModel.Prefix = value;
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
            get { return _expressionModel.Infix; }
            set { _infix = value; RaisePropertyChanged(); }
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

        public string HashDisjunctiveNormalForm
        {
            get { return _hashDisjunctiveNormalForm; }
            set { _hashDisjunctiveNormalForm = value; RaisePropertyChanged(); }
        }

        public MainViewModel(IFixConversionService fixConversionService, ITruthTableService truthTableService, IGraphVizService graphVizService, IFileService fileService)
        {
            ParsePrefixCommand = new RelayCommand(ParsePrefix, ParseCanExecute);

            _expressionModelDisjunctiveNormalForm = new ExpressionModel();
            _expressionModelSimplifiedDisjunctiveNormalForm = new ExpressionModel();
            _expressionModelNand = new ExpressionModel();

            //FOR DEBUGGING
            _expressionModel = new ExpressionModel();
            //Prefix = "|((|(A,(B)),C)";
            Prefix = "&((|(A,~(B)),C)";
            //ExpressionModel.Prefix = "|((|(A,(B)),C)";
            _expressionModel.Prefix = "&((|(A,~(B)),C)";

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
            Infix = _fixConversionService.ParsePrefix(_expressionModel);
            _fixConversionService.GetDistinctVariables(_expressionModel);

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

            //Disjuntive normal form of input
            DisjunctiveNormalForm = _truthTableService.GetDisjunctiveNormalForm(_expressionModel);
            _expressionModelDisjunctiveNormalForm.Prefix = DisjunctiveNormalForm;
            _expressionModelDisjunctiveNormalForm.DistinctVariables = _expressionModel.DistinctVariables;

            //Calculate truth table and parse infix of DNF
            _fixConversionService.ParsePrefix(_expressionModelDisjunctiveNormalForm);
            _truthTableService.GetTruthTable(_expressionModelDisjunctiveNormalForm);

            //Get hash of DNF
            HashDisjunctiveNormalForm = _truthTableService.CalculateHash(_expressionModelDisjunctiveNormalForm);
            _graphVizFileModel = _graphVizService.ConvertExpressionModelToGraphVizFile(_expressionModelDisjunctiveNormalForm);
            _fileService.WriteGraphVizFileToDotFile(_graphVizFileModel.Lines);
            _graphVizService.DisplayGraph();

            //Simplified Disjunctive normal form of input
            SimplifiedDisjunctiveNormalForm = _truthTableService.GetSimplifiedDisjunctiveNormalForm(_expressionModel);
            _expressionModelSimplifiedDisjunctiveNormalForm.Prefix = SimplifiedDisjunctiveNormalForm;
            _expressionModelSimplifiedDisjunctiveNormalForm.DistinctVariables = _expressionModel.DistinctVariables;

            //Calculate truth table and parse infix of Simpl. DNF
            _fixConversionService.ParsePrefix(_expressionModelSimplifiedDisjunctiveNormalForm);
            _truthTableService.GetTruthTable(_expressionModelSimplifiedDisjunctiveNormalForm);

            //Get hash of Simpl. DNF
            HashDisjunctiveNormalFormSimplified = _truthTableService.CalculateHash(_expressionModelSimplifiedDisjunctiveNormalForm);
            _graphVizFileModel = _graphVizService.ConvertExpressionModelToGraphVizFile(_expressionModelSimplifiedDisjunctiveNormalForm);
            _fileService.WriteGraphVizFileToDotFile(_graphVizFileModel.Lines);
            _graphVizService.DisplayGraph();

            //Nand
            Nand = _fixConversionService.GetNandForm(_expressionModel);
        }

    }
}