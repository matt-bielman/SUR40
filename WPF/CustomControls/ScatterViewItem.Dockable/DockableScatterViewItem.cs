using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Input;

namespace Bielman.SUR40.Controls
{
    public partial class DockableScatterViewItem : ScatterViewItem
    {
        private delegate void UpdateItemPosition();

        public delegate void OnDockChangedEventHandler(object sender, EventArgs args);
        public event OnDockChangedEventHandler OnDockChanged;
        
        public enum DockLocationEnum { None, Left, Right, Top, Bottom };

        private bool _isDockable = true;
        public bool IsDockable
        {
            get { return _isDockable; }
            set { _isDockable = value; }
        }

        private Thickness _dockThickness = new Thickness(64);
        public Thickness DockThickness
        {
            get { return _dockThickness; }
            set { _dockThickness = value; }
        }

        private Thickness _dockPadding = new Thickness(64);
        public Thickness DockPadding
        {
            get { return _dockPadding; }
            set { _dockPadding = value; }
        }

        private DockLocationEnum _prevDockedLocation = DockLocationEnum.None;
        private DockLocationEnum _dockedLocation = DockLocationEnum.None;
        public DockLocationEnum DockedLocation
        {
            get { return _dockedLocation; }
            private set { _dockedLocation = value; }
        }

        private BackgroundWorker _worker;
        
        private DockAnimation _dockAnimation;

        private float _dockAnimationDuration = 500;
        public float DockAnimationDuration
        {
            get { return _dockAnimationDuration; }
            set { _dockAnimationDuration = value; }
        }

        public DockableScatterViewItem()
        {
            InitializeComponents();
        }

        /// <summary>
        /// Check ScatterViewItem position against dock thickness and dock it if it's close to the edge of the screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _prevDockedLocation = DockedLocation;

            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (UpdateItemPosition)delegate ()
            {
                this.UpdateLayout();

                // Docked right
                if (this.Center.X >= (this.Parent as ScatterView).ActualWidth - this.DockThickness.Right)
                {
                    this.DockedLocation = DockLocationEnum.Right;
                    _dockAnimation.Play(270, new Point((this.Parent as ScatterView).ActualWidth + (this.ActualHeight / 2) - (this.DockPadding.Right), this.Center.Y));
                }
                // Docked left
                else if (this.Center.X <= this.DockThickness.Left)
                {
                    this.DockedLocation = DockLocationEnum.Left;
                    _dockAnimation.Play(90, new Point(((this.ActualHeight / 2) - this.DockPadding.Left) * -1, this.Center.Y));
                }
                // Docked top
                else if (this.Center.Y <= this.DockThickness.Top)
                {
                    this.DockedLocation = DockLocationEnum.Top;
                    _dockAnimation.Play(180, new Point(this.Center.X, this.DockPadding.Top - (this.ActualHeight / 2)));
                }
                // Docked bottom
                else if (this.Center.Y >= (this.Parent as ScatterView).ActualHeight - this.DockThickness.Bottom)
                {
                    this.DockedLocation = DockLocationEnum.Bottom;
                    _dockAnimation.Play(0, new Point(this.Center.X, (this.Parent as ScatterView).ActualHeight + (this.ActualHeight / 2) - this.DockPadding.Bottom));
                }
                // Not docked
                else
                {
                    this.DockedLocation = DockLocationEnum.None;
                }
            });
        }

        /// <summary>
        /// Check and apply docking after user releases ScatterViewItem and it comes to a stop.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            if ((_worker.IsBusy == false) && (this.IsDockable == true))
            {
                _worker.RunWorkerAsync();
            }
            base.OnManipulationCompleted(e);
        }

        private void InitializeComponents()
        {
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.WorkerReportsProgress = false;
            _worker.WorkerSupportsCancellation = true;
            _worker.RunWorkerCompleted += _worker_RunWorkerCompleted;

            _dockAnimation = new DockAnimation(this, _dockAnimationDuration);
        }

        private void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.DockedLocation != _prevDockedLocation)
            {
                if (OnDockChanged != null)
                {
                    OnDockChanged(this, EventArgs.Empty);
                }
            }
        }
    }
}
