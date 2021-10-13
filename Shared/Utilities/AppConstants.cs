﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remotely.Shared.Utilities
{
    public class AppConstants
    {
        public const string DefaultProductName = "nex-Remote";
        public const string DefaultPublisherName = "nex-IT Jakub Potoczny";
        public const long MaxUploadFileSize = 100_000_000;
        public const int RelayCodeLength = 4;
        public const double ScriptRunExpirationMinutes = 30;

        public const string RemotelyAscii = @"
                         _______  
                      |     |
  ____   ___  \  /    |     |
 / __ \// _ \  \/ ___ |     |
 | || |\  __/  /\     |     |
 |_||_| \___  /  \    |     |
                                            ";
    }
}
