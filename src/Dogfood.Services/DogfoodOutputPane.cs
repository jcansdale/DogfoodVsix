using System;
using System.ComponentModel.Composition;
using Dogfood.Exports;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Dogfood.Services
{
    [Export(typeof(IDogfoodOutputPane))]
    [Export(typeof(IMainThreadInitializable))]
    public class DogfoodOutputPane : IMainThreadInitializable, IDogfoodOutputPane
    {
        public static readonly Guid OutputPaneGuid = new Guid("ba0a91d0-ff3c-41a7-84a9-fd675ceb2e70");

        IProgress<string> progress;
        Window window;
        IVsOutputWindowPane pane;

        public void InitializeOnMainThread(IServiceProvider serviceProvider)
        {
            var dte = (DTE)serviceProvider.GetService(typeof(DTE));
            window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);

            var outputWindow = (IVsOutputWindow)serviceProvider.GetService(typeof(SVsOutputWindow));
            pane = CreatePane(outputWindow, OutputPaneGuid, "Dogfood", true, false);

            progress = new Progress<string>(line =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                pane.OutputString(line + Environment.NewLine);
            });
        }

        public void Activate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            pane.Activate();
            window.Activate();
        }

        static IVsOutputWindowPane CreatePane(IVsOutputWindow outputWindow, Guid paneGuid, string title,
            bool visible, bool clearWithSolution)
        {
            outputWindow.CreatePane(
                ref paneGuid,
                title,
                Convert.ToInt32(visible),
                Convert.ToInt32(clearWithSolution));

            outputWindow.GetPane(ref paneGuid, out IVsOutputWindowPane pane);
            return pane;
        }

        public void Report(string line)
        {
            progress.Report(line);
        }
    }
}
