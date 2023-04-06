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
    public static class ElementID
    {
        public const byte StartOfObject = 0xa1;
        public const byte TerminatingObject = 0xa2;
        public const byte Attribute = 0xa3;
        public const byte Relation = 0xa4;
        public const byte StartOfTagDescription = 0xa7;
        public const byte TerminatingTagDescription = 0xa8;
        public const byte VartypeList = 0xab;
        public const byte VarnameList = 0xac;
    }
}
