using Administrator.Entities;
using Administrator.Panels;
using Life;
using Life.Network;
using ModKit.Helper;
using ModKit.Helper.DiscordHelper;
using ModKit.Interfaces;
using ModKit.Internal;
using _menu = AAMenu.Menu;
using c = Administrator.Helpers.Colors;

namespace Administrator
{
    public class Administrator : ModKit.ModKit
    {
        #region Fields

        /// <summary>
        /// Gestionnaire central des panels d'administration.
        /// </summary>
        private PanelsManager PanelsManager;

        /// <summary>
        /// Webhook permettant d'envoyer les logs des actions des panels.
        /// </summary>
        public static DiscordWebhookClient WebhookClient;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructeur principal du plugin.
        /// Initialise les informations du plugin et le gestionnaire de panels.
        /// </summary>
        /// <param name="api">Interface du jeu fournie par ModKit.</param>
        public Administrator(IGameAPI api) : base(api)
        {
            PluginInformations = new PluginInformations(AssemblyHelper.GetName(), "1.0.1", "Noah FOURNIER");
            PanelsManager = new PanelsManager(this);
        }

        #endregion

        #region Initialisation

        /// <summary>
        /// Méthode appelée lors de l'initialisation du plugin.
        /// </summary>
        public override async void OnPluginInit()
        {
            base.OnPluginInit();
            ModKit.Helper.JsonHelper.JsonHelper.RegisterJson<Administrator_Configuration>();
            await Administrator_Configuration.Reload(); ;
            WebhookClient = new DiscordWebhookClient(Administrator_Configuration.Data.Webhook);
            InitCommands();
            InsertMenu();
            Logger.LogSuccess($"{PluginInformations.SourceName} v{PluginInformations.Version}", "initialisé");
        }

        #endregion

        #region Commands

        /// <summary>
        /// Initialise les commandes.
        /// </summary>
        public async void InitCommands()
        {
            await Administrator_Configuration.Reload();
            var configuration = Administrator_Configuration.Data;

            SChatCommand adminCommand = new SChatCommand("/admin", new string[] { "/a", "/administrateur", "/administrator" }, "Administration du serveur", "/admin", (player, args) =>
            {
                if (player.account.adminLevel >= configuration.Permissions_Commande) PanelsManager.MainPanel(player);
                else
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires pour cela !", NotificationManager.Type.Error);
            });
            adminCommand.Register();
        }

        #endregion

        #region Menu Integration

        /// <summary>
        /// Insère les lignes du AAMenu.
        /// </summary>
        public void InsertMenu()
        {
            Administrator_Configuration.Reload();
            var configuration = Administrator_Configuration.Data;

            _menu.AddAdminTabLine(PluginInformations, configuration.Permissions_Commande, configuration.Nom, (ui) =>
            {
                Player player = PanelHelper.ReturnPlayerFromPanel(ui);
                PanelsManager.MainPanel(player);
            });
        }

        #endregion
    }
}
