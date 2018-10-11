using System;
using System.IO;
using System.ComponentModel.Design;
using System.ComponentModel.Composition;
using Dogfood.Exports;
using EnvDTE;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;

namespace Dogfood.Services
{
    [Export(typeof(IAsyncInitializable))]
    public class DogfoodCommand : IAsyncInitializable
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("998fbf43-97c5-4598-b758-29c5db102cda");

        readonly IDogfoodService dogfoodService;
        readonly IProjectUtilities projectUtilities;
        readonly IDogfoodOutputPane dogfoodOutputPane;
        readonly IServiceProvider serviceProvider;

        IAsyncServiceProvider asyncServiceProvider;

        [ImportingConstructor]
        public DogfoodCommand(
            IDogfoodService dogfoodService,
            IProjectUtilities projectUtilities,
            IDogfoodOutputPane dogfoodOutputPane,
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.dogfoodService = dogfoodService;
            this.projectUtilities = projectUtilities;
            this.dogfoodOutputPane = dogfoodOutputPane;
            this.serviceProvider = serviceProvider;
        }

        public async Task InitializeAsync(IAsyncServiceProvider asp)
        {
            asyncServiceProvider = asp;

            var commandService = (OleMenuCommandService)await asp.GetServiceAsync(typeof(IMenuCommandService));
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        async void MenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                await Execute();
            }
            catch (Exception ex)
            {
                dogfoodOutputPane.Report(ex.ToString());
            }
        }

        async Task Execute()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Visual Studio Extension (*.vsix)|*.vsix"
            };

            var dte = (DTE)await asyncServiceProvider.GetServiceAsync(typeof(DTE));
            var vsixFile = projectUtilities.FindVsixFile(dte.Solution);
            if (vsixFile != null)
            {
                openFileDialog.InitialDirectory = Path.GetDirectoryName(vsixFile);
                openFileDialog.FileName = Path.GetFileName(vsixFile);
            }

            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }

            dogfoodOutputPane.Activate();
            var success = await dogfoodService.Reinstall(openFileDialog.FileName, dogfoodOutputPane);
            if (!success)
            {
                return;
            }

            if (ShouldRestart(serviceProvider))
            {
                var shell = (IVsShell4)await asyncServiceProvider.GetServiceAsync(typeof(SVsShell));
                shell.Restart((uint)__VSRESTARTTYPE.RESTART_Normal);
            }
        }

        static bool ShouldRestart(IServiceProvider serviceProvider)
        {
            // OK = 1, Cancel = 2, Abort = 3, Retry = 4, Ignore = 5, Yes = 6, No = 7
            var result = VsShellUtilities.ShowMessageBox(serviceProvider, "Would you like to restart Visual Studio now?",
                "Restart Visual Studio", OLEMSGICON.OLEMSGICON_WARNING,
                OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            return result == 1;
        }
    }
}
