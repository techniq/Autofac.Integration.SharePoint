using System;
using System.ComponentModel;
using System.Threading;
using System.Web;
using Microsoft.SharePoint;

namespace Autofac.Integration.SharePoint.Demo.Playground
{
    public delegate PlaygroundPresenter PlaygroundPresenterFactory(IPlaygroundView view);

    public class PlaygroundPresenter
    {
        private readonly ILogger _logger;
        private readonly IPlaygroundView _view;

        public PlaygroundPresenter(IPlaygroundView view)
        {
            _view = view;

            try
            {
                //// try some manual detection
                // ...does not work in Sharepoint integrated mode!!!
                //var cpa = (IContainerProviderAccessor) HttpContext.Current.ApplicationInstance;
                //IContainerProvider cp = cpa.ContainerProvider;
                //_logger = cp.RequestLifetime.Resolve<ILogger>();

                // an easier way is:
                this._logger = SPServiceLocator.GetRequestLifetime().Resolve<ILogger>();

                if (!_view.IsPostback)
                {
                    _logger.Log(typeof (PlaygroundPresenter), "Let's play (now with autofac loaded singleton)");
                    _view.SuccessMessage = "Let's play!!";
                }
            }
            catch (Exception ex)
            {
                _view.ErrorMessage = "Playground is under construction: " + ex.Message;
            }
        }

        public ILogger AnotherLogger { get; set; }

        public void TestPostbackWithPropertyInjection()
        {
            // nothing to do here, property injection in Autofac (for ASP.NET) happens on the View (Page, UserControl)
            // other items need to use service locator pattern
            // BUT we can inject properties AFTER the fact
            SPServiceLocator.GetRequestLifetime().InjectProperties(this);
            // Furthermore, since another logger is a singleton, it should be in the same ILifetimeScope as the _logger
            if (AnotherLogger == null || _logger.Uid != AnotherLogger.Uid)
            {
                _view.ErrorMessage = "Property injection failed";
            }
            else
            {
                _view.SuccessMessage = "Property injection was successful";
                AnotherLogger.Log(typeof (PlaygroundPresenter), _view.SuccessMessage);
            }
        }

        public void TestPostbackWithServiceLocator()
        {
            // nothing to do here, property injection in Autofac (for ASP.NET) happens on the View (Page, UserControl)
            // other items need to use service locator pattern
            // BUT we can inject properties AFTER the fact
            AnotherLogger = SPServiceLocator.GetRequestLifetime().Resolve<ILogger>();
            // Furthermore, since another logger is a singleton, it should be in the same ILifetimeScope as the _logger
            if (AnotherLogger == null || _logger.Uid != AnotherLogger.Uid)
            {
                _view.ErrorMessage = "Service location failed or loggers are in different lifetime";
            }
            else
            {
                _view.SuccessMessage = "Service locator call was successful and loggers are the same instance";
                AnotherLogger.Log(typeof (PlaygroundPresenter), _view.SuccessMessage);
            }
        }

        public void TestWithElevatedPrivileges()
        {
            var site = SPContext.Current.Site;
            SPSecurity.RunWithElevatedPrivileges(delegate
                                                     {
                                                         AnotherLogger =
                                                             SPServiceLocator.GetRequestLifetime(site).Resolve<ILogger>();
                                                         if (AnotherLogger == null)
                                                         {
                                                             _view.ErrorMessage =
                                                                 "Elevated privileges was NOT successful";
                                                         }
                                                         else
                                                         {
                                                             _view.SuccessMessage = "Elevated privileges was successful";
                                                             AnotherLogger.Log(typeof (PlaygroundPresenter),
                                                                               _view.SuccessMessage);
                                                         }
                                                     });
        }

        public void TestWithHttpContextCurrentIsNull()
        {
            string uid = _logger.Uid;
            var site = SPContext.Current.Site;
            var requestObj = SPServiceLocator.GetRequestLifetime(site).Resolve<IPlayInterface>();
            HttpContext context = HttpContext.Current;
            HttpContext.Current = null;
            try
            {
                using (ILifetimeScope container = SPServiceLocator.NewDisposableLifetime(site))
                {
                    var logger = container.Resolve<ILogger>();
                    var requestObj2 = container.Resolve<IPlayInterface>();
                    if (logger.Uid != uid)
                        _view.ErrorMessage = "Logger singletons are not referencing the same instance, oh oh";
                    else if (requestObj.Uid == requestObj2.Uid)
                        _view.ErrorMessage = "InstancePerLifetimeScoped items should not 'cross' scopes, weird, oh oh";
                    else
                    {
                        _view.SuccessMessage =
                            "Awesome, singletons still reference the same instances, and per lifetime scoped items don't";
                        logger.Log(typeof (PlaygroundPresenter), _view.SuccessMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _view.ErrorMessage = ex.Message;
            }
            HttpContext.Current = context;
        }

        public void TestInBackgroundThread()
        {
            // page must be async for the following (which still hangs the page)
            var bw = new BackgroundWorker {WorkerSupportsCancellation = false, WorkerReportsProgress = true};
            bw.DoWork += BwDoWork;
            bw.ProgressChanged += BwProgressChanged;
            bw.RunWorkerCompleted += BwRunWorkerCompleted;
            bw.RunWorkerAsync();

            // another way
            //ThreadPool.QueueUserWorkItem(cb => RunABackgroundThread());
        }

        private void BwProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _logger.Log(typeof (PlaygroundPresenter), "Received worker progress: " + e.ProgressPercentage + "%");
        }

        private void BwRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                _view.ErrorMessage = "Oh no, background worker caused an error: " + e.Error.Message;
            else
                _view.SuccessMessage = e.Result.ToString();
        }

        private static void BwDoWork(object sender, DoWorkEventArgs e)
        {
            ILifetimeScope container = null;
            try
            {
                container = SPServiceLocator.NewDisposableLifetime("worker");
                var bwLogger = container.Resolve<ILogger>();

                var worker = sender as BackgroundWorker;

                for (int i = 1; (i <= 10); i++)
                {
                    // Perform a time consuming operation and report progress.
                    Thread.Sleep(500);
                    if (worker != null)
                        worker.ReportProgress((i*10));
                    bwLogger.Log(typeof (PlaygroundPresenter), string.Format("bw worker progress: {0}%", (i*10)));
                        // watch this in ULS
                }

                e.Result = "Super, background worker has completed";
            }
            finally
            {
                if (container != null)
                    container.Dispose();
            }
        }
    }
}