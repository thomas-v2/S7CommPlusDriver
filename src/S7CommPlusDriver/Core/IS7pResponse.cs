#region License
/******************************************************************************
 * S7CommPlusDriver
 * 
 * Copyright (C) 2023 Thomas Wiens, th.wiens@gmx.de
 *
 * This file is part of S7CommPlusDriver.
 *
 * S7CommPlusDriver is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 /****************************************************************************/
#endregion

namespace S7CommPlusDriver
{
    public interface IS7pResponse
    {
        byte ProtocolVersion
        {
            get;
            set;
        }

        ushort FunctionCode
        {
            get;
        }

        ushort SequenceNumber
        {
            get;
            set;
        }

        uint IntegrityId
        {
            get;
            set;
        }

        bool WithIntegrityId
        {
            get;
            set;
        }
    }
}
