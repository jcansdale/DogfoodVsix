using System;
using System.ComponentModel.Composition;
using Dogfood.Exports;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

namespace Dogfood.Services
{
    [Export(typeof(IDogfoodOutputPane))]
    [Export(typeof(IAsyncInitializable))]
    public class DogfoodOutputPane : IAsyncInitializable, IDogfoodOutputPane
    {
        public static readonly Guid OutputPaneGuid = new Guid("ba0a91d0-ff3c-41a7-84a9-fd675ceb2e70");

        IProgress<string> progress;

        public async Task InitializeAsync(IAsyncServiceProvider asyncServiceProvider)
        {
            var outputWindow = (IVsOutputWindow)await asyncServiceProvider.GetServiceAsync(typeof(SVsOutputWindow));
            var pane = CreatePane(outputWindow, OutputPaneGuid, "Dogfood", true, false);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            pane.Activate();

            progress = new Progress<string>(line =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                pane.OutputString(line + Environment.NewLine);
            });
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
