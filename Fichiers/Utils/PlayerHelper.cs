using Life;
using Life.Network;
using System.Linq;

namespace Administrator.Helpers
{
    public static class PlayerHelper
    {
        public static Player GetPlayer(object input)
        {
            if (input == null) return null;

            var players = Nova.server.GetAllInGamePlayers();

            if (input is int characterId)
            {
                return players.FirstOrDefault(p => p.character.Id == characterId);
            }

            if (input is string text)
            {
                string lowerText = text.ToLower();

                return players.FirstOrDefault(p =>
                    (!string.IsNullOrEmpty(p.character.Firstname) && p.character.Firstname.ToLower() == lowerText) ||
                    (!string.IsNullOrEmpty(p.character.Lastname) && p.character.Lastname.ToLower() == lowerText)
                );
            }

            return null;
        }
    }
}
