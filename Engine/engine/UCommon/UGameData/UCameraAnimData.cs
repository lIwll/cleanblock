using System;
using System.Collections.Generic;

namespace UEngine.Data
{
    public class UCameraAnimData : UGameData< UCameraAnimData >
    {
        public float SwingX { get; set; }
        public int RateX { get; set; }

        public float SwingY { get; set; }
        public int RateY { get; set; }

        public float SwingZ { get; set; }
        public int RateZ { get; set; }

        public int Priority { get; set; }

        static public readonly string kFileName = "xml/CameraAnim";
    }
}
