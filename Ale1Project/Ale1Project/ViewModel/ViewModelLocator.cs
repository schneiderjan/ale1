/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:Ale1Project.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{expressionModel.Binaryding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using Ale1Project.Model;
using Ale1Project.Service;

namespace Ale1Project.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the expressionModel.Binarydings.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<IFixConversionService, FixConversionService>();
            SimpleIoc.Default.Register<ITruthTableService, TruthTableService>();
            SimpleIoc.Default.Register<IOperatorService, OperatorService>();
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data expressionModel.Binaryding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public IFixConversionService FixConversionService
        {
            get { return ServiceLocator.Current.GetInstance<FixConversionService>(); }
        }

        public ITruthTableService TruthTableService
        {
            get { return ServiceLocator.Current.GetInstance<TruthTableService>(); }
        }

        public IOperatorService OperatorService
        {
            get { return ServiceLocator.Current.GetInstance<OperatorService>(); }

        }

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
        }
    }
}