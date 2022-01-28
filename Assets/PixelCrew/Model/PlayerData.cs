using System;

namespace PixelCrew.Model
{
    [Serializable]
    public class PlayerData
    {
        public int Coins;
        public int Hp;
        public int Swords;
        public bool IsArmed;
        public bool DoubleJumpIsActive;
        public bool DashIsActive;
        public bool WallHookIsActive;
        public bool ThrowingSwordIsActive;
    }
}
