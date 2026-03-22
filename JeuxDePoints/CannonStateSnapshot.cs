namespace JeuxDePoints {
    public class CannonStateSnapshot {
        public int PlayerId { get; }
        public int YPosition { get; }
        public int CurrentAmmo { get; }
        public int MaxAmmo { get; }

        public CannonStateSnapshot(int playerId, int yPosition, int currentAmmo, int maxAmmo) {
            PlayerId = playerId;
            YPosition = yPosition;
            CurrentAmmo = currentAmmo;
            MaxAmmo = maxAmmo;
        }

        public static CannonStateSnapshot FromCannon(Cannon cannon) {
            return new CannonStateSnapshot(
                cannon.GetPlayerId(),
                cannon.GetYPosition(),
                cannon.GetCurrentAmmo(),
                cannon.GetMaxAmmo()
            );
        }
    }
}
