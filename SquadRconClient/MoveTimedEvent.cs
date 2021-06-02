using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace SquadRconClient
{
    public class MoveTimedEvent
    {
        private Dictionary<string, object> _args;
        private readonly System.Timers.Timer _timer;

        public delegate void TimedEventFireDelegate(MoveTimedEvent evt);

        public event TimedEventFireDelegate OnFire;

        public MoveTimedEvent(double interval)
        {
            this._timer = new System.Timers.Timer();
            this._timer.Interval = interval;
            this._timer.Elapsed += new ElapsedEventHandler(this._timer_Elapsed);
        }

        public MoveTimedEvent(double interval, Dictionary<string, object> args)
            : this(interval)
        {
            this.Args = args;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.OnFire != null)
            {
                this.OnFire(this);
            }
        }

        public void Start()
        {
            this._timer.Start();
        }

        public void Stop()
        {
            this._timer.Stop();
        }

        public void Kill()
        {
            this._timer.Stop();
            this._timer.Dispose();
        }

        public Dictionary<string, object> Args
        {
            get { return this._args; }
            set { this._args = value; }
        }

        public double Interval
        {
            get { return this._timer.Interval; }
            set { this._timer.Interval = value; }
        }
    }
}
