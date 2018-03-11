using System;
using System.ComponentModel.Design;
using Dogfood.Exports;
using Dogfood.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.ComponentModelHost;
using Task = System.Threading.Tasks.Task;

namespace InstallExperiment
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class DogfoodCommand
    {
        public static readonly Guid OutputPaneGuid = new Guid("ba0a91d0-ff3c-41a7-84a9-fd675ceb2e70");

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("998fbf43-97c5-4598-b758-29c5db102cda");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="DogfoodCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private DogfoodCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static DogfoodCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            var commandService = (OleMenuCommandService)await package.GetServiceAsync(typeof(IMenuCommandService));

            Instance = new DogfoodCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        async void MenuItemCallback(object sender, EventArgs e)
        {
            var pane = package.GetOutputPane(OutputPaneGuid, "Dogfood");

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            pane.Activate();
            var progress = new Progress<string>(line =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                pane.OutputString(line + Environment.NewLine);
            });

            var componentModel = (IComponentModel)await package.GetServiceAsync(typeof(SComponentModel));
            var service = componentModel.GetService<IDogfoodService>();
            await service.Execute(progress);
        }
    }
}
