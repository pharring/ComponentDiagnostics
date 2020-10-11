using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.VisualStudio.ComponentDiagnostics
{
    /// <summary>
    /// Interaction logic for OleComponentStatusIndicator.xaml
    /// </summary>
    public partial class OleComponentStatusIndicator : UserControl
    {
        public OleComponentStatusIndicator()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == TagProperty)
            {
                OnIndicatorValueChanged((bool)e.NewValue);
            }

            base.OnPropertyChanged(e);
        }

        private const double MinimumScale = 0.5;
        static readonly DoubleAnimation growAnimation = new DoubleAnimation(1.0, new Duration(TimeSpan.FromSeconds(2.0)), FillBehavior.HoldEnd);
        static readonly DoubleAnimation shrinkAnimation = new DoubleAnimation(0.0, new Duration(TimeSpan.FromSeconds(0.33333)), FillBehavior.HoldEnd);

        void OnIndicatorValueChanged(bool enabled)
        {
            // Remove existing animation
            this.clipScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);

            DoubleAnimation anim = enabled ? growAnimation : shrinkAnimation;

            // For very short durations, the growth animation hasn't even begun yet, so make
            // sure something is visible.
            this.clipScale.ScaleX = Math.Max(this.clipScale.ScaleX, MinimumScale);

            this.clipScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim, HandoffBehavior.SnapshotAndReplace);
        }
    }
}
