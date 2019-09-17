﻿using Microsoft.Xna.Framework;
using Pulsarc.Skinning;
using Wobble.Logging;

namespace Pulsarc.UI.Screens.MainMenu.UI
{
    class GameIcon : Drawable
    {
        public GameIcon(Vector2 position, Anchor anchor = Anchor.CenterTop) : base(Skin.assets["menu_game_icon"], position, anchor: anchor) {}
    }
}
