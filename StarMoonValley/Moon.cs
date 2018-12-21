using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;

namespace StarMoonValley
{
    public class Moon
    {
        /* ********** *
         * Properties *
         * ********** */

        private int cycle;
        private int phase;
        private string phaseName;
        private int firstCycle;

        internal IMonitor Monitor;
        internal IDictionary<int, int> LunarCalendar = new Dictionary<int, int>();
        internal int Cycle
        {
            get { return cycle; }
            set
            {
                // Monitor.Log($"Cycle.set called, before calculations.", LogLevel.Trace);
                if (value < 1 || value >= 25) // possibly update this later to better account for negative inputs
                    cycle = 1;
                cycle = value;
                // Monitor.Log($"Cycle.set called. Received {value} and returned {cycle}.", LogLevel.Trace);
            }
        }
        internal int Phase
        {
            get { return phase; }
            set
            {
                phase = value; // value should be a Cycle int
                // Monitor.Log($"Phase.set called. Received {value} and returned {phase}.", LogLevel.Trace);
            }
        } // needs validation
        internal string PhaseName
        {
            get { return phaseName; }
            set
            {
                string name = value;

                if (name != phaseName)
                {
                    phaseName = name;

                    // print alert box when phase changes
                    Game1.addHUDMessage(new HUDMessage($"Tonight, the moon is {phaseName}."));
                    Monitor.Log($"PhaseName if conditions met. Phase changed to {phaseName}");
                }

                //PhaseName = CalculatePhaseName(value); // value should be a Phase int

                phaseName = value; // add validation?
                // Monitor.Log($"PhaseName.set called. Received {value} and returned {phaseName}.", LogLevel.Trace);

                // get string
            }
        } // needs some work
        internal bool HasChanged { get; set; }
        internal int FirstCycle
        {
            get { return firstCycle; }
            set
            {
                firstCycle = value;
                // Monitor.Log($"FirstCycle.set called. Received {value} and returned {FirstCycle}.", LogLevel.Trace);
            }
        }

        /* ************** *
         * Public Methods *
         * ************** */

        public Moon()
        {
            // in case we need to initialize stuff here
        }

        /* *************** *
         * internal Methods *
         * *************** */

        internal int CalculatePhase(int days)
        {
            int tPhase = -1;

            // Monitor.Log($"CalculatePhase() called. Received {days}.", LogLevel.Trace);
            if (days > 24)
                days -= 24;

            if (days == 1 || days == 7 || days == 13 || days == 19)
            {
                switch (days)
                {
                    case 1: // new moon
                        tPhase = 0;
                        // Monitor.Log($"New Moon", LogLevel.Trace);
                        break;
                    case 7: // half-full
                        tPhase = 1;
                        // Monitor.Log($"Half-Full Moon", LogLevel.Trace);
                        break;
                    case 13: // full
                        tPhase = 2;
                        // Monitor.Log($"Full Moon", LogLevel.Trace);
                        break;
                    case 19: // half-new
                        tPhase = 3;
                        // Monitor.Log($"Half-New Moon", LogLevel.Trace);
                        break;
                    default:
                        tPhase = 6;
                        // Monitor.Log("Error in Phase Case Switch", LogLevel.Trace);
                        break;
                }
            }
            else
            {
                if (days >= 2 && days <= 12)
                    tPhase = 4; // waxing
                if (days >= 14 && days <= 24)
                    tPhase = 5; // waning
                // Monitor.Log($"CalculatePhase() -> elseif waxing/waning accessed.", LogLevel.Trace);
            }
            // Monitor.Log($"CalculatePhase() completed. Returned {tPhase}", LogLevel.Trace);
            return tPhase;
        }

        internal string CalculatePhaseName(int tPhase)
        {
            string name = "";

            // Monitor.Log($"CalculatePhaseName() called. Received {tPhase}.", LogLevel.Trace);
            // receives the phase number and returns phase string
            if (tPhase >= 0 && tPhase <= 6)
            {
                switch (tPhase)
                {
                    case 0:
                        name = "new";
                        break;
                    case 1:
                        name = "half-full";
                        break;
                    case 2:
                        name = "full";
                        break;
                    case 3:
                        name = "half-new";
                        break;
                    case 4:
                        name = "waxing";
                        break;
                    case 5:
                        name = "waning";
                        break;
                    default:
                        name = "eldritch";
                        break;
                }
            }
            // Monitor.Log($"CalculatePhaseName() completed. Returned {name}", LogLevel.Trace);
            return name;
        }

        internal int CalculateFirstCycle(int days)
        {
            int tFirst = 0;
            tFirst = days - (Game1.dayOfMonth - 1);
            // Monitor.Log($"CalculateFirstCycle() called. Received {days} and returned {tFirst}", LogLevel.Trace);
            return tFirst;
        }

        internal void CalculateCalendar(int tFirst)
        {
            var date = SDate.Now();

            for (int day = 1; day <= 28; day++)
            {
                int currentCycle = tFirst + (day - 1) % 24;
                int tPhase = CalculatePhase(currentCycle);
                // Monitor.Log($"{day} - Cycle: {currentCycle}, Phase: {tPhase}");
                LunarCalendar.Add(day, tPhase);
            }
            Monitor.Log($"CalculateCalendar() called. Received {tFirst}, returned the Lunar Calendar.", LogLevel.Trace);
        }
        
        internal void InitializeCycle()
        {
            Monitor.Log($"InitializeCycle() Begun.", LogLevel.Trace);
            // choose a random day for our position in the cycle
            Random rnd = new Random();
            int days = rnd.Next(1, 25);
            Cycle = days; // days is the go-to local int for this work
            Phase = CalculatePhase(Cycle);
            PhaseName = CalculatePhaseName(Phase);
            FirstCycle = CalculateFirstCycle(Cycle);
            HasChanged = false; // update check to show that Cycle hasn't incremented today
            Monitor.Log($"InitializeCycle() called. Starting variables: {Cycle}, {Phase}, {PhaseName}, {FirstCycle}, {HasChanged}", LogLevel.Trace); 
        }

        internal void IncrementCycle()
        {
            Monitor.Log($"IncrementCycle() called. Starting variables: {Cycle}, {Phase}, {PhaseName}, {FirstCycle}, {HasChanged}", LogLevel.Trace);
            // increment cycle w/ each new day (soon to be @ 6pm)
            Cycle++;

            // return to beginning of 24-day cycle
            if (Cycle >= 25) // probably could use better validation eventually
            {
                Cycle = 1;
            }

            Phase = CalculatePhase(Cycle);
            PhaseName = CalculatePhaseName(Phase);
            HasChanged = true;

            Monitor.Log($"IncrementCycle() called. Starting variables: {Cycle}, {Phase}, {PhaseName}, {FirstCycle}, {HasChanged}", LogLevel.Trace);
        }
    }
}
