using Administrator.Entities;
using Administrator.Panels.EventManagement;
using Administrator.Panels.PlayerManagement;
using Administrator.Panels.VehicleManagement;
using Life.Network;
using ModKit.Helper;
using ModKit.Utils;
using SQLite;
using c = Administrator.Helpers.Colors;
using t = Administrator.Helpers.TextHelper;

namespace Administrator.Panels
{
    internal class PanelsManager
    {
        #region Context

        /// <summary>
        /// Contexte global du plugin.
        /// </summary>
        [Ignore] public ModKit.ModKit Context { get; set; }

        /// <summary>
        /// Instance des panels de gestion des joueurs.
        /// </summary>
        public PlayerManagementPanels PlayerManagementPanels;

        /// <summary>
        /// Instance des panels de gestion des véhicules.
        /// </summary>
        public VehicleManagementPanels VehicleManagementPanels;

        /// <summary>
        /// Instance des panels de gestion des événements.
        /// </summary>
        public EventManagementPanels EventManagementPanels;

        /// <summary>
        /// Initialise le gestionnaire de panels avec le contexte du plugin.
        /// </summary>
        /// <param name="context">Instance du plugin permettant d'accéder à ModKit.</param>
        public PanelsManager(ModKit.ModKit context)
        {
            Context = context;

            PlayerManagementPanels = new PlayerManagementPanels(context);
            VehicleManagementPanels = new VehicleManagementPanels(context);
            EventManagementPanels = new EventManagementPanels(context);
        }

        #endregion

        #region Panels

        /// <summary>
        /// Ouvre le panel principal du plugin.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le plugin.</param>
        public async void MainPanel(Player player)
        {
            await Administrator_Configuration.Reload();
            var configuration = Administrator_Configuration.Data;

            Panel panel = Context.PanelHelper.Create(t.Title(configuration.Nom, configuration.Description, c.WhiteColor), Life.UI.UIPanel.PanelType.TabPrice, player, () => MainPanel(player));

            panel.AddTabLine(t.TabInfo("Gestion des joueurs", "Gestion complète des joueurs.", c.BlueColor), "", ItemUtils.GetIconIdByItemId(1349), ui => PlayerManagementPanels.PlayerManagementPanel(player));
            panel.AddTabLine(t.TabInfo("Gestion des véhicules", "Gestion complète des véhicules.", c.GreenColor), "", VehicleUtils.GetIconId(55), ui =>
            {
                var inVehicle = player.GetVehicle() ? true : false;
                if (inVehicle) VehicleManagementPanels.VehicleManagementPanel(player);
                else player.Notify(c.Format(c.RedColor, "Aucun véhicule"), "Vous n'êtes pas dans un véhicule !", Life.NotificationManager.Type.Error);
            });
            panel.AddTabLine(t.TabInfo("Gestion des événements", "Gestion complète des événements.", c.YellowColor), "", ItemUtils.GetIconIdByItemId(1311), ui => EventManagementPanels.EventManagementPanel(player));

            panel.CloseButton(c.Format(c.RedColor, "Fermer"));
            panel.NextButton(c.Format(c.GreenColor, "Sélectionner"), () => panel.SelectTab());

            panel.Display();
        }

        #endregion
    }
}
