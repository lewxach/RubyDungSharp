using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubyDungSharp
{
    public class Timer
    {
        private const long NS_PER_SECOND = 1000000000L;
        private const long MAX_NS_PER_UPDATE = 1000000000L;
        private const int MAX_TICKS_PER_UPDATE = 100;
        private float ticksPerSecond;
        private long lastTime;
        public int ticks;
        public float a;
        public float timeScale = 1.0f;
        public float fps = 0.0f;
        public float passedTime = 0.0f;

        public Timer(float ticksPerSecond)
        {
            this.ticksPerSecond = ticksPerSecond;
            this.lastTime = GetCurrentTimeInNanoseconds();
        }

        static long GetCurrentTimeInNanoseconds()
        {
            DateTime now = DateTime.UtcNow;

            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long ticksSinceEpoch = (now - epoch).Ticks;

            long nanoseconds = ticksSinceEpoch * 100;

            return nanoseconds;
        }

        public void advanceTime()
        {
            long now = GetCurrentTimeInNanoseconds();
            long passedNs = now - this.lastTime;
            this.lastTime = now;
            if (passedNs < 0L)
            {
                passedNs = 0L;
            }
            if (passedNs > 1000000000L)
            {
                passedNs = 1000000000L;
            }
            this.fps = 1000000000L / passedNs;
            this.passedTime += (float)passedNs * this.timeScale * this.ticksPerSecond / 1.0E9f;
            this.ticks = (int)this.passedTime;
            if (this.ticks > 100)
            {
                this.ticks = 100;
            }
            this.passedTime -= (float)this.ticks;
            this.a = this.passedTime;
        }
    }
}