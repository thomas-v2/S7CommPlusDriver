using S7CommPlusDriver.Legitimation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace S7CommPlusDriver {
    public partial class S7CommPlusConnection 
    {

        private byte[] omsSecret;

        /// <summary>
        /// Legitimation stage of the connect routine
        /// </summary>
        /// <param name="serverSession">Server sesstion information containing the firmware version</param>
        /// <param name="password">PLC password</param>
        /// <param name="username">PLC username (leave empty for legacy login)</param>
        /// <returns>error code (0 = ok)</returns>
        private int legitimate(ValueStruct serverSession, string password, string username = "")
        {
            // Parse device and firmware version
            string sessionVersionPAOMString = ((ValueWString)serverSession.GetStructElement((uint)Ids.LID_SessionVersionSystemPAOMString)).GetValue();
            Regex reVersions = new Regex("^.*;.*[17]\\s?([52]\\d\\d).+;[VS](\\d\\.\\d)$");
            Match m = reVersions.Match(sessionVersionPAOMString);
            if (!m.Success)
            {
                Console.WriteLine("S7CommPlusConnection - Legitimate: Could not extract firmware version!");
                return S7Consts.errCliFirmwareNotSupported;
            }
            string deviceVersion = m.Groups[1].Value;
            string firmwareVersion = m.Groups[2].Value;
            int fwVerNo = int.Parse(firmwareVersion.Split('.')[0]) * 100;
            fwVerNo += int.Parse(firmwareVersion.Split('.')[1]);

            // Check if we have to use legacy legitimation via the firmware version
            bool legacyLegitimation = false;
            if (deviceVersion.StartsWith("5"))
            {
                if (fwVerNo < 209)
                {
                    Console.WriteLine("S7CommPlusConnection - Legitimate: Firmware version is not supported!");
                    return S7Consts.errCliFirmwareNotSupported;
                }
                if (fwVerNo < 301)
                {
                    legacyLegitimation = true;
                }
            }
            else if (deviceVersion.StartsWith("2"))
            {
                if (fwVerNo < 403)
                {
                    Console.WriteLine("S7CommPlusConnection - Legitimate: Firmware version is not supported!");
                    return S7Consts.errCliFirmwareNotSupported;
                }
                if (fwVerNo < 407)
                {
                    legacyLegitimation = true;
                }
            }
            else
            {
                Console.WriteLine("S7CommPlusConnection - Legitimate: Device version is not supported!");
                return S7Consts.errCliDeviceNotSupported;
            }

            // Get current protection level
            var getVarSubstreamedReq = new GetVarSubstreamedRequest(ProtocolVersion.V2);
            getVarSubstreamedReq.InObjectId = m_SessionId;
            getVarSubstreamedReq.SessionId = m_SessionId;
            getVarSubstreamedReq.Address = Ids.EffectiveProtectionLevel;
            int res = SendS7plusFunctionObject(getVarSubstreamedReq);
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                m_client.Disconnect();
                return m_LastError;
            }

            var getVarSubstreamedRes = GetVarSubstreamedResponse.DeserializeFromPdu(m_ReceivedPDU);
            if (getVarSubstreamedRes == null)
            {
                Console.WriteLine("S7CommPlusConnection - Legitimate: GetVarSubstreamedResponse with Error!");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }

            // Check access level
            UInt32 accessLevel = (getVarSubstreamedRes.Value as ValueUDInt).GetValue();
            if (accessLevel > AccessLevel.FullAccess && password != "")
            {
                // Legitimate
                if (legacyLegitimation)
                {
                    return legitimateLegacy(password);
                }
                else
                {
                    return legitimateNew(password, username);
                }

            }
            else if (accessLevel > AccessLevel.FullAccess)
            {
                Console.WriteLine("S7CommPlusConnection - Legitimate: Warning: Access level is not fullaccess but no password set!");
            }

            return 0;
        }

        /// <summary>
        /// Legitimate using the new login method (firmware >= 3.1)
        /// </summary>
        /// <param name="password">PLC password</param>
        /// <param name="username">PLC username (leave empy for legacy login)</param>
        /// <returns>error code (0 = ok)</returns>
        private int legitimateNew(string password, string username = "")
        {
            // Get challenge
            var getVarSubstreamedReq_challange = new GetVarSubstreamedRequest(ProtocolVersion.V2);
            getVarSubstreamedReq_challange.InObjectId = m_SessionId;
            getVarSubstreamedReq_challange.SessionId = m_SessionId;
            getVarSubstreamedReq_challange.Address = Ids.ServerSessionRequest;
            int res = SendS7plusFunctionObject(getVarSubstreamedReq_challange);
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                m_client.Disconnect();
                return m_LastError;
            }

            var getVarSubstreamedRes_challenge = GetVarSubstreamedResponse.DeserializeFromPdu(m_ReceivedPDU);
            if (getVarSubstreamedRes_challenge == null)
            {
                Console.WriteLine("S7CommPlusConnection - Legitimate: getVarSubstreamedRes_challenge with Error!");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }

            byte[] challenge = (getVarSubstreamedRes_challenge.Value as ValueUSIntArray).GetValue();

            // Encrypt challengeResponse
            byte[] challengeResponse;
            if (omsSecret == null || omsSecret.Length != 32)
            {
                // Create oms exporter secret
                omsSecret = m_client.getOMSExporterSecret();
            }
            // Roll key
            byte[] key = LegitimationCrypto.sha256(omsSecret);
            omsSecret = key;

            // Use the first 16 bytes of the challenge as iv
            byte[] iv = new ArraySegment<byte>(challenge, 0, 16).ToArray();
            // Encrypt
            challengeResponse = LegitimationCrypto.EncryptAesCbc(buildLegitimationPayload(password, username), key, iv);

            // Send challengeResponse
            var setVariableReq = new SetVariableRequest(ProtocolVersion.V2);
            setVariableReq.InObjectId = m_SessionId;
            setVariableReq.SessionId = m_SessionId;
            setVariableReq.Address = Ids.Legitimate;
            setVariableReq.Value = new ValueBlob(0, challengeResponse);
            res = SendS7plusFunctionObject(setVariableReq);
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                m_client.Disconnect();
                return m_LastError;
            }

            var setVariableResponse = SetVariableResponse.DeserializeFromPdu(m_ReceivedPDU);
            if (setVariableResponse == null)
            {
                Console.WriteLine("S7CommPlusConnection - Legitimate: setVariableResponse with Error!");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }
            // Check if the legitimation attempt was successful
            Int16 errorCode = (Int16)setVariableResponse.ReturnValue;
            if (errorCode < 0)
            {
                Console.WriteLine("S7CommPlusConnection - Legitimate: access denied");
                m_client.Disconnect();
                return S7Consts.errCliAccessDenied;
            }

            return 0;
        }

        /// <summary>
        /// Builds the legitimation payload from given username and password.
        /// If username is empty the payload for a legacy login will be build.
        /// If username is not empty the payload for the new login is build.
        /// </summary>
        /// <param name="password">PLC password</param>
        /// <param name="username">PLC username (optional)</param>
        /// <returns>Build payload</returns>
        private static byte[] buildLegitimationPayload(string password, string username = "")
        {
            ValueStruct payload = new ValueStruct(Ids.LID_LegitimationPayloadStruct);
            if (username != "")
            {
                // Login with username and password = new login
                payload.AddStructElement(Ids.LID_LegitimationPayloadType, new ValueUDInt(LegitimationType.New));
                payload.AddStructElement(Ids.LID_LegitimationPayloadUsername, new ValueBlob(0, Encoding.UTF8.GetBytes(username)));
                payload.AddStructElement(Ids.LID_LegitimationPayloadPassword, new ValueBlob(0, Encoding.UTF8.GetBytes(password)));

            }
            else
            {
                // Login with only password = legacy login
                // Hash password
                byte[] hashedPw;
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    hashedPw = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                }

                payload.AddStructElement(Ids.LID_LegitimationPayloadType, new ValueUDInt(LegitimationType.Legacy));
                payload.AddStructElement(Ids.LID_LegitimationPayloadUsername, new ValueBlob(0, Encoding.UTF8.GetBytes(username)));
                payload.AddStructElement(Ids.LID_LegitimationPayloadPassword, new ValueBlob(0, hashedPw));
            }
            using (var memStr = new MemoryStream())
            {
                payload.Serialize(memStr);
                return memStr.ToArray();
            }
        }

        /// <summary>
        /// Legitimate using the old legacy login (firmware version < 3.1)
        /// </summary>
        /// <param name="password">PLC password</param>
        /// <returns>error code (0 = OK)</returns>
        private int legitimateLegacy(string password)
        {

            // Get challenge
            var getVarSubstreamedReq_challange = new GetVarSubstreamedRequest(ProtocolVersion.V2);
            getVarSubstreamedReq_challange.InObjectId = m_SessionId;
            getVarSubstreamedReq_challange.SessionId = m_SessionId;
            getVarSubstreamedReq_challange.Address = Ids.ServerSessionRequest;
            int res = SendS7plusFunctionObject(getVarSubstreamedReq_challange);
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                m_client.Disconnect();
                return m_LastError;
            }

            var getVarSubstreamedRes_challenge = GetVarSubstreamedResponse.DeserializeFromPdu(m_ReceivedPDU);
            if (getVarSubstreamedRes_challenge == null)
            {
                Console.WriteLine("S7CommPlusConnection - Legitimate: getVarSubstreamedRes_challenge with Error!");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }

            byte[] challenge = (getVarSubstreamedRes_challenge.Value as ValueUSIntArray).GetValue();

            // Calculate challengeResponse [sha1(password) xor challenge]
            byte[] challengeResponse;
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                challengeResponse = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
            if (challengeResponse.Length != challenge.Length)
            {
                Console.WriteLine("S7CommPlusConnection - Legitimate: challengeResponse.Length != challenge.Length");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }
            for (int i = 0; i < challengeResponse.Length; ++i)
            {
                challengeResponse[i] = (byte)(challengeResponse[i] ^ challenge[i]);
            }

            // Send challengeResponse
            var setVariableReq = new SetVariableRequest(ProtocolVersion.V2);
            setVariableReq.InObjectId = m_SessionId;
            setVariableReq.SessionId = m_SessionId;
            setVariableReq.Address = Ids.ServerSessionResponse;
            setVariableReq.Value = new ValueUSIntArray(challengeResponse);
            res = SendS7plusFunctionObject(setVariableReq);
            if (res != 0)
            {
                m_client.Disconnect();
                return res;
            }
            m_LastError = 0;
            WaitForNewS7plusReceived(m_ReadTimeout);
            if (m_LastError != 0)
            {
                m_client.Disconnect();
                return m_LastError;
            }

            var setVariableResponse = SetVariableResponse.DeserializeFromPdu(m_ReceivedPDU);
            if (setVariableResponse == null)
            {
                Console.WriteLine("S7CommPlusConnection - Legitimate: setVariableResponse with Error!");
                m_client.Disconnect();
                return S7Consts.errIsoInvalidPDU;
            }
            // Check if the legitimation attempt was successful
            Int16 errorCode = (Int16)setVariableResponse.ReturnValue;
            if (errorCode < 0)
            {
                Console.WriteLine("S7CommPlusConnection - Legitimate: access denied");
                m_client.Disconnect();
                return S7Consts.errCliAccessDenied;
            }

            return 0;
        }
    }
}
