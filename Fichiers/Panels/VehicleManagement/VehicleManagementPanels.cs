using System;
using System.Collections.Generic;
using Administrator.Entities;
using Life;
using Life.Network;
using Life.VehicleSystem;
using ModKit.Helper;
using ModKit.Helper.DiscordHelper;
using SQLite;
using c = Administrator.Helpers.Colors;
using t = Administrator.Helpers.TextHelper;

namespace Administrator.Panels.VehicleManagement
{
    public class VehicleManagementPanels
    {
        #region Context
        /// <summary>
        /// Contexte global du plugin.
        /// </summary>
        [Ignore] public static ModKit.ModKit Context { get; set; }

        /// <summary>
        /// Initialise les panels de gestion des véhicules avec le contexte du plugin.
        /// </summary>
        /// <param name="context">Instance du plugin permettant d'accéder à ModKit.</param>
        public VehicleManagementPanels(ModKit.ModKit context)
        {
            Context = context;
        }

        #endregion

        #region Vehicle Management

        /// <summary>
        /// Ouvre un panel permettant la gestion des véhicules du serveur.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        public void VehicleManagementPanel(Player player)
        {
            Administrator_Configuration.Reload();
            var configuration = Administrator_Configuration.Data;

            LifeVehicle vehicle = Nova.v.GetVehicle((int)player.GetVehicleId());

            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des véhicules", "Faites un choix", c.GreenColor), Life.UI.UIPanel.PanelType.Tab, player, () => VehicleManagementPanel(player));

            panel.AddTabLine("Déplacer vers l'avant", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionVehicules_Déplacement)
                {
                    MoveVehicleForward(player, vehicle);
                }
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Déplacer vers l'arrière", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionVehicules_Déplacement)
                {
                    MoveVehicleBackward(player, vehicle);
                }
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Déplacer vers la gauche", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionVehicules_Déplacement)
                {
                    MoveVehicleLeft(player, vehicle);
                }
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Déplacer vers la droite", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionVehicules_Déplacement)
                {
                    MoveVehicleRight(player, vehicle);
                }
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Déplacer vers le haut", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionVehicules_Déplacement)
                {
                    MoveVehicleUp(player, vehicle);
                }
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Déplacer vers le bas", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionVehicules_Déplacement)
                {
                    MoveVehicleDown(player, vehicle);
                }
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Retourner le véhicule", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionVehicules_Déplacement)
                {
                    FlipVehicle(player, vehicle);
                }
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Réparer le véhicule", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionVehicules_Réparation)
                {
                    RepairVehicle(player, vehicle);
                }
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Remplir le véhicule", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionVehicules_Remplissage)
                {
                    RefuelVehicle(player, vehicle);
                }
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Ranger le véhicule", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionVehicules_Rangement)
                {
                    StowVehicle(player, vehicle);
                }
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.AddButton(c.Format(c.GreenColor, "Sélectionner"), ui => panel.SelectTab());

            panel.Display();
        }

        #endregion

        #region Vehicle Movement Functions

        /// <summary>
        /// Déplace le véhicule vers l'avant.
        /// </summary>
        /// <param name="player">Le joueur ayant déclanché l'action.</param>
        /// <param name="vehicle">Le véhicule cible.</param>
        public async void MoveVehicleForward(Player player, LifeVehicle vehicle)
        {
            vehicle.instance.RpcAddPosition(vehicle.instance.transform.forward);
            vehicle.instance.transform.position += vehicle.instance.transform.forward;
            player.Notify(c.Format(c.GreenColor, "Succès"), "Le véhicule a été déplacé vers l'avant.", Life.NotificationManager.Type.Success);
            await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**🚗 Véhicule déplacé vers l'avant**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🚘 Plaque Véhicule", "🔧 ID Véhicule" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, vehicle.plate, vehicle.vehicleId.ToString() }, true, true, "Administrator - By noah_fournier");
        }

        /// <summary>
        /// Déplace le véhicule vers l'arrière.
        /// </summary>
        /// <param name="player">Le joueur ayant déclanché l'action.</param>
        /// <param name="vehicle">Le véhicule cible.</param>
        public async void MoveVehicleBackward(Player player, LifeVehicle vehicle)
        {
            vehicle.instance.RpcAddPosition(-vehicle.instance.transform.forward);
            vehicle.instance.transform.position -= vehicle.instance.transform.forward;
            player.Notify(c.Format(c.GreenColor, "Succès"), "Le véhicule a été déplacé vers l'arrière.", Life.NotificationManager.Type.Success);
            await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**🚗 Véhicule déplacé vers l'arrière**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🚘 Plaque Véhicule", "🔧 ID Véhicule" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, vehicle.plate, vehicle.vehicleId.ToString() }, true, true, "Administrator - By noah_fournier");
        }

        /// <summary>
        /// Déplace le véhicule vers la gauche.
        /// </summary>
        /// <param name="player">Le joueur ayant déclanché l'action.</param>
        /// <param name="vehicle">Le véhicule cible.</param>
        public async void MoveVehicleLeft(Player player, LifeVehicle vehicle)
        {
            vehicle.instance.RpcAddPosition(-vehicle.instance.transform.right);
            vehicle.instance.transform.position -= vehicle.instance.transform.right;
            player.Notify(c.Format(c.GreenColor, "Succès"), "Le véhicule a été déplacé vers la gauche.", Life.NotificationManager.Type.Success);
            await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**🚗 Véhicule déplacé vers la gauche**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🚘 Plaque Véhicule", "🔧 ID Véhicule" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, vehicle.plate, vehicle.vehicleId.ToString() }, true, true, "Administrator - By noah_fournier");
        }

        /// <summary>
        /// Déplace le véhicule vers la droite.
        /// </summary>
        /// <param name="player">Le joueur ayant déclanché l'action.</param>
        /// <param name="vehicle">Le véhicule cible.</param>
        public async void MoveVehicleRight(Player player, LifeVehicle vehicle)
        {
            vehicle.instance.RpcAddPosition(vehicle.instance.transform.right);
            vehicle.instance.transform.position += vehicle.instance.transform.right;
            player.Notify(c.Format(c.GreenColor, "Succès"), "Le véhicule a été déplacé vers la droite.", Life.NotificationManager.Type.Success);
            await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**🚗 Véhicule déplacé vers la droite**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🚘 Plaque Véhicule", "🔧 ID Véhicule" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, vehicle.plate, vehicle.vehicleId.ToString() }, true, true, "Administrator - By noah_fournier");
        }

        /// <summary>
        /// Déplace le véhicule vers le haut.
        /// </summary>
        /// <param name="player">Le joueur ayant déclanché l'action.</param>
        /// <param name="vehicle">Le véhicule cible.</param>
        public async void MoveVehicleUp(Player player, LifeVehicle vehicle)
        {
            vehicle.instance.RpcAddPosition(vehicle.instance.transform.up);
            vehicle.instance.transform.position += vehicle.instance.transform.up;
            player.Notify(c.Format(c.GreenColor, "Succès"), "Le véhicule a été déplacé vers le haut.", Life.NotificationManager.Type.Success);
            await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**🚗 Véhicule déplacé vers le haut**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🚘 Plaque Véhicule", "🔧 ID Véhicule" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, vehicle.plate, vehicle.vehicleId.ToString() }, true, true, "Administrator - By noah_fournier");
        }

        /// <summary>
        /// Déplace le véhicule vers le bas.
        /// </summary>
        /// <param name="player">Le joueur ayant déclanché l'action.</param>
        /// <param name="vehicle">Le véhicule cible.</param>
        public async void MoveVehicleDown(Player player, LifeVehicle vehicle)
        {
            vehicle.instance.RpcAddPosition(-vehicle.instance.transform.up);
            vehicle.instance.transform.position -= vehicle.instance.transform.up;
            player.Notify(c.Format(c.GreenColor, "Succès"), "Le véhicule a été déplacé vers le bas.", Life.NotificationManager.Type.Success);
            await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**🚗 Véhicule déplacé vers le bas**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🚘 Plaque Véhicule", "🔧 ID Véhicule" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, vehicle.plate, vehicle.vehicleId.ToString() }, true, true, "Administrator - By noah_fournier");
        }

        /// <summary>
        /// Retourne le véhicule.
        /// </summary>
        /// <param name="player">Le joueur ayant déclanché l'action.</param>
        /// <param name="vehicle">Le véhicule cible.</param>
        public async void FlipVehicle(Player player, LifeVehicle vehicle)
        {
            vehicle.instance.RpcFlip();
            player.Notify(c.Format(c.GreenColor, "Succès"), "Le véhicule a été retourné.", Life.NotificationManager.Type.Success);
            await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**🚗 Véhicule retourné**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🚘 Plaque Véhicule", "🔧 ID Véhicule" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, vehicle.plate, vehicle.vehicleId.ToString() }, true, true, "Administrator - By noah_fournier");
        }

        /// <summary>
        /// Répare le véhicule.
        /// </summary>
        /// <param name="player">Le joueur ayant déclanché l'action.</param>
        /// <param name="vehicle">Le véhicule cible.</param>
        public async void RepairVehicle(Player player, LifeVehicle vehicle)
        {
            vehicle.instance.Repair();
            player.Notify(c.Format(c.GreenColor, "Succès"), "Le véhicule a été réparé.", Life.NotificationManager.Type.Success);
            await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**🚗 Véhicule réparé**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🚘 Plaque Véhicule", "🔧 ID Véhicule" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, vehicle.plate, vehicle.vehicleId.ToString() }, true, true, "Administrator - By noah_fournier");
        }

        /// <summary>
        /// Remplit le véhicule.
        /// </summary>
        /// <param name="player">Le joueur ayant déclanché l'action.</param>
        /// <param name="vehicle">Le véhicule cible.</param>
        public async void RefuelVehicle(Player player, LifeVehicle vehicle)
        {
            vehicle.fuel = 100f;
            player.Notify(c.Format(c.GreenColor, "Succès"), "Le véhicule a été rempli.", Life.NotificationManager.Type.Success);
            await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**🚗 Véhicule rempli**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🚘 Plaque Véhicule", "🔧 ID Véhicule" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, vehicle.plate, vehicle.vehicleId.ToString() }, true, true, "Administrator - By noah_fournier");
        }

        /// <summary>
        /// Range le véhicule.
        /// </summary>
        /// <param name="player">Le joueur ayant déclanché l'action.</param>
        /// <param name="vehicle">Le véhicule cible.</param>
        public async void StowVehicle(Player player, LifeVehicle vehicle)
        {
            Nova.v.StowVehicle(vehicle.vehicleId);
            vehicle.isStowed = true;
            player.Notify(c.Format(c.GreenColor, "Succès"), "Le véhicule a été rangé.", Life.NotificationManager.Type.Success);
            await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**🚗 Véhicule rangé**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID", "🚘 Plaque Véhicule", "🔧 ID Véhicule" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId, vehicle.plate, vehicle.vehicleId.ToString() }, true, true, "Administrator - By noah_fournier");
        }

        #endregion
    }
}
