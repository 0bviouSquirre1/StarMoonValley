namespace StarMoonValley
{
    class ModData
    {
        /* ********** *
         * Properties *
         * ********** */

        public int Cycle { get; set; }
        public int Phase { get; set; }
        public string PhaseName { get; set; }
        public int FirstCycle { get; internal set; }

        /* ************** *
         * Public Methods *
         * ************** */

        public ModData()
        {
            Cycle = 0;
            Phase = 0;
            PhaseName = "new";
            FirstCycle = 0;
        }
    }
}