﻿using System;

namespace CountDown.Applications.Domain
{
    public interface ICountDownItem
    {
        DateTime AlertTime { get; set; }

        DateTime Time { get; set; }

        string Notice { get; set; }

        bool HasAlert { get; set; }
    }
}