using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Bielman.SUR40.Controls
{
    public class DockAnimation
    {
        private DockableScatterViewItem _dockableScatterViewItem;

        private Storyboard _dockStoryboard;
        public Storyboard DockStoryboard
        {
            get { return _dockStoryboard; }
            private set { _dockStoryboard = value; }
        }

        private DoubleAnimation _orientationAnimation;
        public DoubleAnimation OrientationAnimation
        {
            get { return _orientationAnimation; }
            private set { _orientationAnimation = value; }
        }

        private PointAnimation _positionAnimation;
        public PointAnimation PositionAnimation
        {
            get { return _positionAnimation; }
            private set { _positionAnimation = value; }
        }

        private float _storyboardDuration;
        public float StoryboardDuration
        {
            get { return _storyboardDuration; }
            set { _storyboardDuration = value; }
        }

        private float _targetOrientation;
        private Point _targetPosition;

        /// <summary>
        /// We construct the docking animation in code since dynamic 
        /// property binding support is limited in user controls
        /// </summary>
        /// <param name="target"></param>
        /// <param name="durationInMilliseconds"></param>
        public DockAnimation(DockableScatterViewItem target, float durationInMilliseconds)
        {
            _dockableScatterViewItem = target;

            StoryboardDuration = durationInMilliseconds;

            DockStoryboard = new Storyboard();
            DockStoryboard.FillBehavior = FillBehavior.Stop;
            DockStoryboard.Completed += DockStoryboard_Completed;

            OrientationAnimation = new DoubleAnimation();
            DockStoryboard.Children.Add(OrientationAnimation);

            PositionAnimation = new PointAnimation();
            DockStoryboard.Children.Add(PositionAnimation);

            Storyboard.SetTarget(OrientationAnimation, target);
            Storyboard.SetTargetProperty(OrientationAnimation, new PropertyPath(DockableScatterViewItem.OrientationProperty));

            Storyboard.SetTarget(PositionAnimation, target);
            Storyboard.SetTargetProperty(PositionAnimation, new PropertyPath(DockableScatterViewItem.CenterProperty));
        }

        public virtual void Play(float _newOrientation, Point _newPosition)
        {
            // Store final orientation and position so we can apply it after animating
            _targetOrientation = _newOrientation;

            // Adjust target to "nearest" angle to prevent excessive rotation
            if (_dockableScatterViewItem.Orientation - _newOrientation > 180) { _targetOrientation += 360; }
            if (_dockableScatterViewItem.Orientation - _newOrientation < -180) { _targetOrientation -= 360; }

            _targetPosition = _newPosition;

            // Align ScatterViewItem to the docked edge
            OrientationAnimation.From = _dockableScatterViewItem.Orientation;
            OrientationAnimation.To = _targetOrientation;
            OrientationAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(StoryboardDuration));

            // Partially tuck the ScatterViewItem under the docked edge
            PositionAnimation.From = _dockableScatterViewItem.Center;
            PositionAnimation.To = _targetPosition;
            PositionAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(_storyboardDuration));

            _dockStoryboard.Begin();
        }

        /// <summary>
        /// Apply final resting orientation and position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DockStoryboard_Completed(object sender, EventArgs e)
        {
            _dockableScatterViewItem.Orientation = _targetOrientation;
            _dockableScatterViewItem.Center = _targetPosition;
        }
    }
}
