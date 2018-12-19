﻿using System;

namespace Rhisis.Core.Data
{
    [Flags]
    public enum StateFlags
    {
        OBJSTAF_COMBAT = 0x00000001,
        OBJSTAF_WALK = 0x00000002,
        OBJSTAF_SIT = 0x00000004,
        OBJSTAF_FLY = 0x00000008,
        OBJSTAF_ACC = 0x00000010,
        OBJSTAF_ETC = 0x00000020,
        OBJSTAF_ACCTURN = 0x00000040,
        OBJSTAF_TURBO = 0x00000080,
    }
}
