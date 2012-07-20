using System;

namespace NotifyMessageDemo
{
    public class AnimatedLocation
    {
        private readonly double _fromLeft;
        public double FromLeft
        {
            get { return _fromLeft; }
        }

        private readonly double _toLeft;
        public double ToLeft
        {
            get { return _toLeft; }
        }

        private readonly double _fromTop;
        public double FromTop
        {
            get { return _fromTop; }
        }

        private readonly double _toTop;
        public double ToTop
        {
            get { return _toTop; }
        }
        
        public AnimatedLocation(double fromLeft, double toLeft, double fromTop, double toTop)
        {
            this._fromLeft  = fromLeft;
            this._toLeft    = toLeft;
            this._fromTop   = fromTop;
            this._toTop     = toTop;
        }
    }
}
