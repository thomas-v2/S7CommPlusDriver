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

using System;
using System.Collections.Generic;
using System.IO;

namespace S7CommPlusDriver
{
    public class PVarnameList
    {
        public List<string> Names;

        public int Deserialize(Stream buffer)
        {
            int ret = 0;
            int maxret;
            byte namelen = 0;
            UInt16 blocklen = 0;
            string name;
            byte unknown2;

            Names = new List<string>();

            ret += S7p.DecodeUInt16(buffer, out blocklen);
            maxret = ret + blocklen;
            while (blocklen > 0)
            {
                do
                {
                    name = String.Empty;
                    // Laenge eines Namens max. 128 Zeichen
                    ret += S7p.DecodeByte(buffer, out namelen);
                    ret += S7p.DecodeWString(buffer, namelen, out name);
                    Names.Add(name);
                    // Noch 1 Byte mit 0 am Ende. Zusätzliche Null-Terminierung des Strings, oder für weitere Funktion?
                    ret += S7p.DecodeByte(buffer, out unknown2);
                } while (ret < maxret);
                ret += S7p.DecodeUInt16(buffer, out blocklen);
                maxret = ret + blocklen;
            }
            return ret;
        }

        public override string ToString()
        {
            string s = "";
            int i = 1;
            s += "<VarnameList>" + Environment.NewLine;
            foreach (var name in Names)
            {
                s += "<Name index=\"" + i.ToString() + "\">" + name.ToString() + "</Name>" + Environment.NewLine;
                i++;
            }
            s += "</VarnameList>" + Environment.NewLine;
            return s;
        }
    }
}
