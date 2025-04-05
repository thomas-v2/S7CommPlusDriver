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
using System.Diagnostics;
using DataBufferList = System.Collections.Generic.LinkedList<OpenSsl.OpenSSLConnector.DataBuffer>;

namespace OpenSsl
{
    public class OpenSSLConnector
    {
        private bool m_readRequired;
        private readonly IntPtr m_pSslConnection; // SSL*
        private readonly IntPtr m_pBioIn; // BIO*
        private readonly IntPtr m_pBioOut; // BIO*

        private readonly IConnectorCallback m_DataSink;
        private int m_bytesAvailable;
        private readonly byte[] m_buffer = new byte[4096];
        private readonly DataBufferList m_pendingWriteList;
        private readonly DataBufferList m_pendingReadList;

        public interface IConnectorCallback
        {
            void WriteData(byte[] pData, int dataLength);
            void OnDataAvailable();
        }

        public class DataBuffer
        {
            public byte[] data;
            public int used;

            public DataBuffer(byte[] dat, int datLength)
            {
                data = new byte[datLength];
                Buffer.BlockCopy(dat, 0, data, 0, datLength);
                used = datLength;
            }

            public void ConsumeAndRemove(int bytesUsed)
            {
                Buffer.BlockCopy(data, bytesUsed, data, 0, bytesUsed);
                used -= bytesUsed;
            }
        };

        public OpenSSLConnector(IntPtr pContext, IConnectorCallback dataSink)
        {
            m_readRequired = false;
            m_pSslConnection = Native.SSL_new(pContext);
            m_pBioIn = Native.BIO_new(Native.BIO_s_mem());
            m_pBioOut = Native.BIO_new(Native.BIO_s_mem());

            Native.SSL_set_bio(m_pSslConnection, m_pBioIn, m_pBioOut);

            m_DataSink = dataSink;
            m_pendingWriteList = new LinkedList<DataBuffer>();
            m_pendingReadList = new LinkedList<DataBuffer>();
        }

        ~OpenSSLConnector()
        {
            Native.SSL_free(m_pSslConnection);
        }

        private int DataToWrite(byte[] pData, int dataLength)
        {
            int bytesUsed = 0;

            int result = Native.SSL_write(m_pSslConnection, pData, dataLength);

            if (result < 0)
            {
                HandleError(result);
            }
            else
            {
                bytesUsed = result;
            }

            if (Native.SSL_want(m_pSslConnection) == Native.SSL_READING)
            {
                m_readRequired = true;
            }

            return bytesUsed;
        }

        private int DataToRead(byte[] pData, int dataLength)
        {
            m_readRequired = false;

            int bytesUsed = Native.BIO_write(m_pBioIn, pData, dataLength);

            byte[] pBuffer = null;
            int bufferSize = 0;

            GetNextReadDataBuffer(ref pBuffer, ref bufferSize);

            int bytesOut;

            do
            {
                bytesOut = Native.SSL_read(m_pSslConnection, pBuffer, bufferSize);

                if (bytesOut > 0)
                {
                    OnDataToRead(pBuffer, bytesOut);
                }

                if (bytesOut < 0)
                {
                    HandleError(bytesOut);
                }
            }
            while (bytesOut > 0);

            return bytesUsed;
        }

        private void SendPendingData()
        {
            while (Native.BIO_ctrl_pending(m_pBioOut) > 0)
            {
                byte[] pBuffer = null;
                int bufferSize = 0;

                GetNextWriteDataBuffer(ref pBuffer, ref bufferSize);

                int bytesToSend = Native.BIO_read(m_pBioOut, pBuffer, bufferSize);

                if (bytesToSend > 0)
                {
                    OnDataToWrite(pBuffer, bytesToSend);
                }

                if (bytesToSend <= 0)
                {
                    if (Native.BIO_should_retry(m_pBioOut) != 0)
                    {
                        HandleError(bytesToSend);
                    }
                }
            }
        }

        public void ExpectConnect()
        {
            Native.SSL_set_connect_state(m_pSslConnection);
        }

        void HandleError(int result)
        {
            if (result <= 0)
            {
                int error = Native.SSL_get_error(m_pSslConnection, result);

                switch (error)
                {
                    case Native.SSL_ERROR_ZERO_RETURN:
                    case Native.SSL_ERROR_NONE:
                    case Native.SSL_ERROR_WANT_READ:
                        // States that can occur in a normal state
                        break;
                    default:
                        // TOOO: Handle all other errors which don't should occur.
                        // 5 with SSL_ASYNC_PAUSED has been seen...
                        Trace.WriteLine("OpenSSL HandleError: Error = " + error);
                        break;
                }
            }
        }

        protected void RunSSL()
        {
            bool dataToWrite = false;
            bool dataToRead = false;

            GetPendingOperations(ref dataToRead, ref dataToWrite);

            while ((!m_readRequired && dataToWrite) || dataToRead)
            {
                if (Native.SSL_in_init(m_pSslConnection) != 0)
                {
                    // Client waiting in connect
                }

                if (dataToRead)
                {
                    PerformRead();
                }

                if (!m_readRequired && dataToWrite)
                {
                    PerformWrite();
                }

                if (Native.BIO_ctrl_pending(m_pBioOut) != 0)
                {
                    SendPendingData();
                }

                GetPendingOperations(ref dataToRead, ref dataToWrite);
            }
        }

        public void Write(byte[] pData,int dataLen)
        {
            DataBuffer pBuffer = new DataBuffer(pData, dataLen);
            AppendBuffer(m_pendingWriteList, pBuffer);

            RunSSL();
        }

        public void ReadCompleted(byte[] pData, int dataLen)
        {
            DataBuffer pBuffer = new DataBuffer(pData, dataLen);
            AppendBuffer(m_pendingReadList, pBuffer);

            RunSSL();
        }

        private void GetPendingOperations(ref bool dataToRead, ref bool dataToWrite)
        {
            dataToRead = m_pendingReadList.Count > 0;
            dataToWrite = m_pendingWriteList.Count > 0;
        }

        private void PerformRead()
        {
            DataBuffer pBuffer = GetNextBuffer(m_pendingReadList);

            if (pBuffer != null)
            {
                int usedBytes = DataToRead(pBuffer.data, pBuffer.used);

                UseData(m_pendingReadList, pBuffer, usedBytes);
            }
        }

        private void PerformWrite()
        {
            DataBuffer pBuffer = GetNextBuffer(m_pendingWriteList);

            if (pBuffer != null)
            {
                int usedBytes = DataToWrite(pBuffer.data, pBuffer.used);

                UseData(m_pendingWriteList, pBuffer, usedBytes);
            }
        }

        private void OnDataToWrite(byte[] pData, int dataLength)
        {
            m_DataSink.WriteData(pData, dataLength);
        }

        private void OnDataToRead(byte[] pData, int dataLength)
        {
            m_bytesAvailable = dataLength;

            while (m_bytesAvailable > 0)
            {
                m_DataSink.OnDataAvailable();
                // Sink sollte anschließend Receive aufrufen um Daten abzuholen
            }
        }

        private void GetNextReadDataBuffer(ref byte[] pBuffer, ref int bufferSize)
        {
            pBuffer = m_buffer;
            bufferSize = m_buffer.Length;
        }

        private void GetNextWriteDataBuffer(ref byte[] pBuffer, ref int bufferSize)
        {
            pBuffer = m_buffer;
            bufferSize = m_buffer.Length;
        }

        public int Receive(ref byte[] pData, int dataLength)
        {
            int bytesRead = Math.Min(m_bytesAvailable, dataLength);

            Buffer.BlockCopy(m_buffer, 0, pData, 0, bytesRead);

            if (bytesRead != m_bytesAvailable)
            {
                Buffer.BlockCopy(m_buffer, bytesRead, m_buffer, 0, m_bytesAvailable - bytesRead);
            }

            m_bytesAvailable -= bytesRead;

            return bytesRead;
        }

        private void UseData(DataBufferList list, DataBuffer pBuffer, int result)
        {
            if (result == 0)
            {
                PutbackBuffer(list, pBuffer);
            }
            else if (result < pBuffer.used)
            {
                pBuffer.ConsumeAndRemove(result);
                PutbackBuffer(list, pBuffer);
            }
            else if (result == pBuffer.used)
            {
                // All data from buffer was uses, can be used for new data
            }
        }

        private void AppendBuffer(DataBufferList list, DataBuffer pBuffer)
        {
            list.AddLast(pBuffer);
        }

        DataBuffer GetNextBuffer(DataBufferList list)
        {
            DataBuffer head = null;
            if (list.Count > 0)
            {
                head = list.First.Value;
                list.RemoveFirst();
            }
            return head;
        }

        void PutbackBuffer(DataBufferList list, DataBuffer pBuffer)
        {
            list.AddFirst(pBuffer);
        }

        /// <summary>
        /// Create OMS exporter secret that is needed for the legitimation with the PLC
        /// </summary>
        /// <returns>Secret</returns>
        public byte[] getOMSExporterSecret()
        {
            byte[] secretOut = new byte[32];
            int ret = (int)Native.SSL_export_keying_material(m_pSslConnection, secretOut, (uint)secretOut.Length, "EXPERIMENTAL_OMS".ToCharArray(), 16, IntPtr.Zero, 0, 0);
            return secretOut;
        }
    }
}
