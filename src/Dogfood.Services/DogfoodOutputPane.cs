using System;
using System.ComponentModel.Composition;
using Dogfood.Exports;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace Dogfood.Services
{
    [Export(typeof(IDogfoodOutputPane))]
    [Export(typeof(IAsyncInitializable))]
    public class DogfoodOutputPane : IAsyncInitializable, IDogfoodOutputPane
    {
        public static readonly Guid OutputPaneGuid = new Guid("ba0a91d0-ff3c-41a7-84a9-fd675ceb2e70");

        IProgress<string> progress;

        public async Task InitializeAsync(AsyncPackage package)
        {
            var pane = package.GetOutputPane(OutputPaneGuid, "Dogfood");

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            pane.Activate();

            progress = new Progress<string>(line =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                pane.OutputString(line + Environment.NewLine);
            });
        }

        public void Report(string line)
        {
            progress.Report(line);
        }
    }
}
