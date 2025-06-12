using System.Collections.Generic;
using System;
using System.Linq;
using Administrator.Entities;
using Life;
using Life.Network;
using ModKit.Helper;
using ModKit.Helper.DiscordHelper;
using SQLite;
using c = Administrator.Helpers.Colors;
using t = Administrator.Helpers.TextHelper;

namespace Administrator.Panels.EventManagement
{
    public class EventManagementPanels
    {
        #region Context

        /// <summary>
        /// Contexte global du plugin.
        /// </summary>
        [Ignore] public static ModKit.ModKit Context { get; set; }

        /// <summary>
        /// Initialise les panels de gestion des événements avec le contexte du plugin.
        /// </summary>
        /// <param name="context">Instance du plugin permettant d'accéder à ModKit.</param>
        public EventManagementPanels(ModKit.ModKit context)
        {
            Context = context;
        }

        #endregion

        #region Event Management

        /// <summary>
        /// Booléen indiquant si tous les joueurs du serveur sont mute.
        /// </summary>
        bool MuteAll = false;

        /// <summary>
        /// Ouvre un panel permettant la gestion des événements du serveur.
        /// </summary>
        /// <param name="player">Le joueur ayant ouvert le panel.</param>
        public async void EventManagementPanel(Player player)
        {
            await Administrator_Configuration.Reload();
            var configuration = Administrator_Configuration.Data;

            Panel panel = Context.PanelHelper.Create(t.Title("Gestion des événements", "Faites un choix", c.YellowColor), Life.UI.UIPanel.PanelType.Tab, player, () => EventManagementPanel(player));

            panel.AddTabLine(player.setup.voice.Networkmegaphone ? "Désactiver le mégaphone" : "Activer le mégaphone", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionEvenements_Megaphone) MegaphoneManagement(player);
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine(MuteAll ? "Démute tous les joueurs" : "Mute tous les joueurs", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionEvenements_MuteAll) MuteAllManagement(player);
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });
            panel.AddTabLine("Réanimer tous les joueurs", ui =>
            {
                if (player.account.adminLevel >= configuration.Permissions_GestionEvenements_Reanimation) ReviveAllPlayers(player);
                else
                {
                    player.Notify(c.Format(c.RedColor, "Permissions insuffisantes"), "Vous ne possédez pas les permissions nécessaires !", NotificationManager.Type.Error);
                    panel.Refresh();
                }
            });

            panel.PreviousButton(c.Format(c.BlueColor, "Retour"));
            panel.NextButton(c.Format(c.GreenColor, "Sélectionner"), () =>
            {
                panel.SelectTab();
                panel.Refresh();
            });

            panel.Display();
        }

        #endregion

        #region Event Management Functions

        /// <summary>
        /// Active ou désactive le mégaphone.
        /// </summary>
        /// <param name="player">Le joueur ayant déclenché l'action.</param>
        public async void MegaphoneManagement(Player player)
        {
            bool state = player.setup.voice.Networkmegaphone;

            if (state)
            {
                player.setup.voice.Networkmegaphone = false;
                player.Notify(c.Format(c.RedColor, "Mégaphone désactivé"), "Le mégaphone a été désactivé !", Life.NotificationManager.Type.Error);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#ff0000", "**📢 Mégapgone désactivé**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId }, true, true, "Administrator - By noah_fournier");
            }
            else
            {
                player.setup.voice.Networkmegaphone = true;
                player.Notify(c.Format(c.GreenColor, "Mégaphone activé"), "Le mégaphone a été activé !", Life.NotificationManager.Type.Success);
                await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**📢 Mégapgone activé**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId }, true, true, "Administrator - By noah_fournier");
            }
        }

        public async void MuteAllManagement(Player player)
        {
            var players = Nova.server.GetAllInGamePlayers().Where(p => p.account.adminLevel == 0);
            foreach (var p in players)
            {
                if (MuteAll)
                {
                    p.setup.voice.NetworkisMuted = false;
                    p.setup.voice.isMuted = false;
                }
                else
                {
                    p.setup.voice.NetworkisMuted = true;
                    p.setup.voice.isMuted = true;
                }
            }
            MuteAll = !MuteAll;
            player.Notify(c.Format(c.GreenColor, "MuteAll"), $"Tous les joueurs ont été {(MuteAll ? "Mute" : "Unmute")} !");
            await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", $"**🎙️ Joueurs {(MuteAll ? "Mute" : "Unmute")}**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId }, true, true, "Administrator - By noah_fournier");
        }

        /// <summary>
        /// Réanime tous les joueurs du serveur.
        /// </summary>
        /// <param name="player">Le joueur ayant déclenché l'action.</param>
        public async void ReviveAllPlayers(Player player)
        {
            foreach (var p in Nova.server.GetAllInGamePlayers())
            {
                p.Health = 100;
            }
            player.Notify(c.Format(c.GreenColor, "Joueurs réanimés"), "Vous avez réanimé tous les joueurs du serveur !", Life.NotificationManager.Type.Success);
            await DiscordHelper.SendEmbed(Administrator.WebhookClient, "#00ff00", "**❤️ Joueurs réanimés**", null, new List<string> { "🕒 Heure", "👤 Administrateur", "🆔 ID Joueur", "🎮 Steam ID" }, new List<string> { DateTime.Now.ToString("HH:mm:ss"), player.FullName, player.character.Id.ToString(), player.account.steamId }, true, true, "Administrator - By noah_fournier");
        }

        #endregion
    }
}
