using System;

namespace JeuxDePoints {
    public class Cannon {
        // A cannon is a special action that players can use to shoot a point on the grid.
        // cannon is fixed on the x axis and can only shoot points in the same row, the power of the cannon determine how far it can shoot, 1 target the first intersection and 9 target the last intersection in the same row
        // x position of the cannon is determined by the player id, player 1 have a cannon on the left side and player 2 have a cannon on the right side

        private int playerId;
        private int maxAmmo;
        private int currentAmmo;
        private int yPosition;  

        public Cannon(int playerId, int yPosition) {
            this.playerId = playerId;
            this.maxAmmo = GameRule.MAX_AMMO;
            this.currentAmmo = maxAmmo;
            this.yPosition = yPosition;
        }

        public bool Shoot(int power) {
            if (currentAmmo <= 0) {
                return false; // No ammo left
            }
            if (power < GameRule.MIN_CANNON_POWER || power > GameRule.MAX_CANNON_POWER) {
                return false; // Invalid power
            }

            if(GameRule.INFINITE_AMMO) {
                currentAmmo--;
            }

            return true; // Shot fired successfully
        }

        public bool Reload() {
            if (!GameRule.CAN_RELOAD_AMMO) {
                return false; // Reloading not allowed
            }

            if(currentAmmo == maxAmmo) {
                return false; // Already fully loaded
            }
            currentAmmo = Math.Min(maxAmmo, currentAmmo + GameRule.AMMO_RELOAD_AMOUNT);
            return true; // Reloaded successfully
        }

        public bool MoveVertically(int deltaY, int gridHeight) {
            int newYPosition = yPosition + deltaY;
            if (newYPosition < 0 || newYPosition >= gridHeight) {
                return false; // Out of bounds
            }
            yPosition = newYPosition;
            return true; // Moved successfully
        }

        public int GetYPosition() => yPosition;

        public int GetCurrentAmmo() => currentAmmo;

        public int GetMaxAmmo() => maxAmmo;

        public int GetPlayerId() => playerId;
    }
}
