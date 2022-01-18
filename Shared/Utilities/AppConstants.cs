using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nexRemoteFree.Shared.Utilities
{
    public class AppConstants
    {
        public const string DefaultProductName = "nex-RemoteFree";
        public const string DefaultPublisherName = "nex-IT Jakub Potoczny";
        public const long MaxUploadFileSize = 100_000_000;
        public const int RelayCodeLength = 4;
        public const double ScriptRunExpirationMinutes = 30;

        public const string nexRemoteFreeAscii = @"
                         _______  
                      |     |
  ____   ___  \  /    |     |
 / __ \// _ \  \/ ___ |     |
 | || |\  __/  /\     |     |
 |_||_| \___  /  \    |     |
                                            ";
    }
}
