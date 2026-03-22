using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace JeuxDePoints {
    internal sealed class BulletAnimator : IDisposable {
        private readonly Timer timer;
        private readonly Control invalidateTarget;

        private List<(int x, int y)> currentPath;
        private int currentStep;
        private int animatingPointIndex = -1;
        private Brush currentBrush;

        public event Action<int> AnimationCompleted;

        public bool IsAnimating => currentPath != null;
        public int AnimatingPointIndex => animatingPointIndex;
        public Brush CurrentBrush => currentBrush;

        public bool HasCurrentBullet =>
            currentPath != null &&
            currentStep >= 0 &&
            currentStep < currentPath.Count;

        public (int x, int y) CurrentPosition => currentPath[currentStep];

        public BulletAnimator(Control invalidateTarget, int frameIntervalMs = 20) {
            this.invalidateTarget = invalidateTarget;
            timer = new Timer { Interval = frameIntervalMs };
            timer.Tick += OnTick;
        }

        public void Start(List<(int x, int y)> path, int pointIndex, Brush brush) {
            if (path == null || path.Count == 0) {
                return;
            }

            currentPath = path;
            currentStep = 0;
            animatingPointIndex = pointIndex;
            currentBrush = brush;

            if (!timer.Enabled) {
                timer.Start();
            }

            invalidateTarget.Invalidate();
        }

        public void Stop() {
            if (timer.Enabled) {
                timer.Stop();
            }

            currentPath = null;
            currentStep = 0;
            animatingPointIndex = -1;
            currentBrush = null;
        }

        private void OnTick(object sender, EventArgs e) {
            if (currentPath == null) {
                return;
            }

            currentStep += 1;
            if (currentStep >= currentPath.Count) {
                int completedIndex = animatingPointIndex;
                Stop();
                AnimationCompleted?.Invoke(completedIndex);
                return;
            }

            invalidateTarget.Invalidate();
        }

        public void Dispose() {
            timer.Tick -= OnTick;
            timer.Stop();
            timer.Dispose();
        }
    }
}
