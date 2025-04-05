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
using System.Runtime.InteropServices;
using System.Text;

namespace OpenSsl
{
#if _WIN64
    using size_t = UInt64;
#else
    using size_t = UInt32;
#endif

    public class Native
    {
#if _WIN64
        const string DLLNAME = "libcrypto-3-x64.dll";
        const string SSLDLLNAME = "libssl-3-x64.dll";
#else
        const string DLLNAME = "libcrypto-3.dll";
        const string SSLDLLNAME = "libssl-3.dll";
#endif

        #region Delegates

        // typedef void (*SSL_CTX_keylog_cb_func)(const SSL *ssl, const char *line);
        // void SSL_CTX_set_keylog_callback(SSL_CTX* ctx, SSL_CTX_keylog_cb_func cb);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SSL_CTX_keylog_cb_func(IntPtr ssl, string line);

        #endregion

        #region OPENSSL

        // int OPENSSL_init_ssl(uint64_t opts, const OPENSSL_INIT_SETTINGS *settings);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int OPENSSL_init_ssl(UInt64 opts, IntPtr settings);

        #endregion

        #region SSL

        public const int SSL_NOTHING = 1;
        public const int SSL_WRITING = 2;
        public const int SSL_READING = 3;
        public const int SSL_X509_LOOKUP = 4;
        public const int SSL_ASYNC_PAUSED = 5;
        public const int SSL_ASYNC_NO_JOBS = 6;
        public const int SSL_CLIENT_HELLO_CB = 7;
        public const int SSL_RETRY_VERIFY = 8;

        public const int SSL_CTRL_GET_CLIENT_CERT_REQUEST = 9;
        public const int SSL_CTRL_GET_NUM_RENEGOTIATIONS = 10;
        public const int SSL_CTRL_CLEAR_NUM_RENEGOTIATIONS = 11;
        public const int SSL_CTRL_GET_TOTAL_RENEGOTIATIONS = 12;
        public const int SSL_CTRL_GET_FLAGS = 13;
        public const int SSL_CTRL_EXTRA_CHAIN_CERT = 14;
        public const int SSL_CTRL_SET_MSG_CALLBACK = 15;
        public const int SSL_CTRL_SET_MSG_CALLBACK_ARG = 16;
        public const int SSL_CTRL_SET_MTU = 17;
        public const int SSL_CTRL_SESS_NUMBER = 20;
        public const int SSL_CTRL_SESS_CONNECT = 21;
        public const int SSL_CTRL_SESS_CONNECT_GOOD = 22;
        public const int SSL_CTRL_SESS_CONNECT_RENEGOTIATE = 23;
        public const int SSL_CTRL_SESS_ACCEPT = 24;
        public const int SSL_CTRL_SESS_ACCEPT_GOOD = 25;
        public const int SSL_CTRL_SESS_ACCEPT_RENEGOTIATE = 26;
        public const int SSL_CTRL_SESS_HIT = 27;
        public const int SSL_CTRL_SESS_CB_HIT = 28;
        public const int SSL_CTRL_SESS_MISSES = 29;
        public const int SSL_CTRL_SESS_TIMEOUTS = 30;
        public const int SSL_CTRL_SESS_CACHE_FULL = 31;
        public const int SSL_CTRL_MODE = 33;
        public const int SSL_CTRL_GET_READ_AHEAD = 40;
        public const int SSL_CTRL_SET_READ_AHEAD = 41;
        public const int SSL_CTRL_SET_SESS_CACHE_SIZE = 42;
        public const int SSL_CTRL_GET_SESS_CACHE_SIZE = 43;
        public const int SSL_CTRL_SET_SESS_CACHE_MODE = 44;
        public const int SSL_CTRL_GET_SESS_CACHE_MODE = 45;
        public const int SSL_CTRL_GET_MAX_CERT_LIST = 50;
        public const int SSL_CTRL_SET_MAX_CERT_LIST = 51;
        public const int SSL_CTRL_SET_MAX_SEND_FRAGMENT = 52;
        public const int SSL_CTRL_SET_TLSEXT_SERVERNAME_CB = 53;
        public const int SSL_CTRL_SET_TLSEXT_SERVERNAME_ARG = 54;
        public const int SSL_CTRL_SET_TLSEXT_HOSTNAME = 55;
        public const int SSL_CTRL_SET_TLSEXT_DEBUG_CB = 56;
        public const int SSL_CTRL_SET_TLSEXT_DEBUG_ARG = 57;
        public const int SSL_CTRL_GET_TLSEXT_TICKET_KEYS = 58;
        public const int SSL_CTRL_SET_TLSEXT_TICKET_KEYS = 59;
        public const int SSL_CTRL_SET_TLSEXT_STATUS_REQ_CB = 63;
        public const int SSL_CTRL_SET_TLSEXT_STATUS_REQ_CB_ARG = 64;
        public const int SSL_CTRL_SET_TLSEXT_STATUS_REQ_TYPE = 65;
        public const int SSL_CTRL_GET_TLSEXT_STATUS_REQ_EXTS = 66;
        public const int SSL_CTRL_SET_TLSEXT_STATUS_REQ_EXTS = 67;
        public const int SSL_CTRL_GET_TLSEXT_STATUS_REQ_IDS = 68;
        public const int SSL_CTRL_SET_TLSEXT_STATUS_REQ_IDS = 69;
        public const int SSL_CTRL_GET_TLSEXT_STATUS_REQ_OCSP_RESP = 70;
        public const int SSL_CTRL_SET_TLSEXT_STATUS_REQ_OCSP_RESP = 71;
        public const int SSL_CTRL_SET_TLS_EXT_SRP_USERNAME_CB = 75;
        public const int SSL_CTRL_SET_SRP_VERIFY_PARAM_CB = 76;
        public const int SSL_CTRL_SET_SRP_GIVE_CLIENT_PWD_CB = 77;
        public const int SSL_CTRL_SET_SRP_ARG = 78;
        public const int SSL_CTRL_SET_TLS_EXT_SRP_USERNAME = 79;
        public const int SSL_CTRL_SET_TLS_EXT_SRP_STRENGTH = 80;
        public const int SSL_CTRL_SET_TLS_EXT_SRP_PASSWORD = 81;
        public const int DTLS_CTRL_GET_TIMEOUT = 73;
        public const int DTLS_CTRL_HANDLE_TIMEOUT = 74;
        public const int SSL_CTRL_GET_RI_SUPPORT = 76;
        public const int SSL_CTRL_CLEAR_MODE = 78;
        public const int SSL_CTRL_SET_NOT_RESUMABLE_SESS_CB = 79;
        public const int SSL_CTRL_GET_EXTRA_CHAIN_CERTS = 82;
        public const int SSL_CTRL_CLEAR_EXTRA_CHAIN_CERTS = 83;
        public const int SSL_CTRL_CHAIN = 88;
        public const int SSL_CTRL_CHAIN_CERT = 89;
        public const int SSL_CTRL_GET_GROUPS = 90;
        public const int SSL_CTRL_SET_GROUPS = 91;
        public const int SSL_CTRL_SET_GROUPS_LIST = 92;
        public const int SSL_CTRL_GET_SHARED_GROUP = 93;
        public const int SSL_CTRL_SET_SIGALGS = 97;
        public const int SSL_CTRL_SET_SIGALGS_LIST = 98;
        public const int SSL_CTRL_CERT_FLAGS = 99;
        public const int SSL_CTRL_CLEAR_CERT_FLAGS = 100;
        public const int SSL_CTRL_SET_CLIENT_SIGALGS = 101;
        public const int SSL_CTRL_SET_CLIENT_SIGALGS_LIST = 102;
        public const int SSL_CTRL_GET_CLIENT_CERT_TYPES = 103;
        public const int SSL_CTRL_SET_CLIENT_CERT_TYPES = 104;
        public const int SSL_CTRL_BUILD_CERT_CHAIN = 105;
        public const int SSL_CTRL_SET_VERIFY_CERT_STORE = 106;
        public const int SSL_CTRL_SET_CHAIN_CERT_STORE = 107;
        public const int SSL_CTRL_GET_PEER_SIGNATURE_NID = 108;
        public const int SSL_CTRL_GET_PEER_TMP_KEY = 109;
        public const int SSL_CTRL_GET_RAW_CIPHERLIST = 110;
        public const int SSL_CTRL_GET_EC_POINT_FORMATS = 111;
        public const int SSL_CTRL_GET_CHAIN_CERTS = 115;
        public const int SSL_CTRL_SELECT_CURRENT_CERT = 116;
        public const int SSL_CTRL_SET_CURRENT_CERT = 117;
        public const int SSL_CTRL_SET_DH_AUTO = 118;
        public const int DTLS_CTRL_SET_LINK_MTU = 120;
        public const int DTLS_CTRL_GET_LINK_MIN_MTU = 121;
        public const int SSL_CTRL_GET_EXTMS_SUPPORT = 122;
        public const int SSL_CTRL_SET_MIN_PROTO_VERSION = 123;
        public const int SSL_CTRL_SET_MAX_PROTO_VERSION = 124;
        public const int SSL_CTRL_SET_SPLIT_SEND_FRAGMENT = 125;
        public const int SSL_CTRL_SET_MAX_PIPELINES = 126;
        public const int SSL_CTRL_GET_TLSEXT_STATUS_REQ_TYPE = 127;
        public const int SSL_CTRL_GET_TLSEXT_STATUS_REQ_CB = 128;
        public const int SSL_CTRL_GET_TLSEXT_STATUS_REQ_CB_ARG = 129;
        public const int SSL_CTRL_GET_MIN_PROTO_VERSION = 130;
        public const int SSL_CTRL_GET_MAX_PROTO_VERSION = 131;
        public const int SSL_CTRL_GET_SIGNATURE_NID = 132;
        public const int SSL_CTRL_GET_TMP_KEY = 133;
        public const int SSL_CTRL_GET_NEGOTIATED_GROUP = 134;
        public const int SSL_CTRL_SET_RETRY_VERIFY = 136;
        public const int SSL_CTRL_GET_VERIFY_CERT_STORE = 137;
        public const int SSL_CTRL_GET_CHAIN_CERT_STORE = 138;

        // SSL/TLS related defines useful to providers
        // from: prov_ssl.h
        public const int TLS1_VERSION = 0x0301;
        public const int TLS1_1_VERSION = 0x0302;
        public const int TLS1_2_VERSION = 0x0303;
        public const int TLS1_3_VERSION = 0x0304;

        // int SSL_connect(SSL *ssl);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SSL_connect(IntPtr ssl);

        // void SSL_free(SSL *ssl);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SSL_free(IntPtr ssl);

        // int SSL_get_error(const SSL *ssl, int ret);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SSL_get_error(IntPtr ssl, int ret_code);

        // int SSL_in_init(const SSL *s);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SSL_in_init(IntPtr ssl);

        // SSL *SSL_new(SSL_CTX *ctx);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SSL_new(IntPtr ctx);

        // int SSL_read(SSL *ssl, void *buf, int num);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SSL_read(IntPtr ssl, byte[] buf, int len);

        // void SSL_set_accept_state(SSL *ssl);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SSL_set_accept_state(IntPtr ssl);

        // void SSL_set_bio(SSL *ssl, BIO *rbio, BIO *wbio);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SSL_set_bio(IntPtr ssl, IntPtr read_bio, IntPtr write_bio);

        // void SSL_set_connect_state(SSL *ssl);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SSL_set_connect_state(IntPtr ssl);

        // const char *SSL_state_string_long(const SSL *ssl);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SSL_state_string_long(IntPtr ssl);

        // int SSL_want(const SSL* ssl);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SSL_want(IntPtr ssl);

        // int SSL_write(SSL *ssl, const void *buf, int num);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SSL_write(IntPtr ssl, byte[] buf, int len);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr TLS_client_method();

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SSL_export_keying_material(IntPtr ssl, byte[] outKeyMaterial, size_t outKeyMaterialLength, char[] label, size_t labelLength, IntPtr context, size_t contextLength, int useContext);

        #endregion

        #region SSL_CTX

        // long SSL_CTX_ctrl(SSL_CTX *ctx, int cmd, long larg, void *parg);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern long SSL_CTX_ctrl(IntPtr ctx, int cmd, long arg, IntPtr parg);

        // void SSL_CTX_free(SSL_CTX *ctx);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SSL_CTX_free(IntPtr ctx);

        // SSL_CTX *SSL_CTX_new(const SSL_METHOD *method);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SSL_CTX_new(IntPtr sslMethod);

        // int SSL_CTX_set_ciphersuites(SSL_CTX *ctx, const char *str);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SSL_CTX_set_ciphersuites(IntPtr ctx, string str);

        // typedef void (*SSL_CTX_keylog_cb_func)(const SSL *ssl, const char *line);
        // void SSL_CTX_set_keylog_callback(SSL_CTX* ctx, SSL_CTX_keylog_cb_func cb);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SSL_CTX_set_keylog_callback(IntPtr ctx, SSL_CTX_keylog_cb_func cb);

        #endregion

        #region BIO

        // long BIO_ctrl(BIO *bp, int cmd, long larg, void *parg);
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern long BIO_ctrl(IntPtr bp, int cmd, long larg, IntPtr parg);

        // size_t BIO_ctrl_pending(BIO *b);
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern size_t BIO_ctrl_pending(IntPtr bio);

        // void BIO_free_all(BIO *a);
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void BIO_free_all(IntPtr bio);

        // int BIO_free(BIO *a);
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int BIO_free(IntPtr bio);

        // BIO *  BIO_new(const BIO_METHOD *type);
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr BIO_new(IntPtr type);

        // int    BIO_read(BIO *b, void *buf, int len);
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int BIO_read(IntPtr b, byte[] buf, int len);

        // BIO_METHOD *   BIO_s_mem(void);
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr BIO_s_mem();

        // int BIO_test_flags(const BIO *b, int flags);
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int BIO_test_flags(IntPtr b, int flags);

        // int BIO_should_retry(BIO *b) -> define in bio.h: BIO_test_flags(a, BIO_FLAGS_SHOULD_RETRY)
        const int BIO_FLAGS_SHOULD_RETRY = 0x08;
        public static int BIO_should_retry(IntPtr b)
        {
            return Native.BIO_test_flags(b, BIO_FLAGS_SHOULD_RETRY);
        }

        // int BIO_write(BIO *b, const void *data, int dlen);
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int BIO_write(IntPtr b, byte[] data, int dlen);

        // BIO_set_mem_eof_return -> define in bio.h: BIO_ctrl(b,BIO_C_SET_BUF_MEM_EOF_RETURN,v,NULL)
        const int BIO_C_SET_BUF_MEM_EOF_RETURN = 130; // return end of input
        public static long BIO_set_mem_eof_return(IntPtr b, int v)
        {
            return Native.BIO_ctrl(b, BIO_C_SET_BUF_MEM_EOF_RETURN, v, IntPtr.Zero);
        }
        #endregion

        #region ERR
        public const int SSL_ERROR_NONE = 0;
        public const int SSL_ERROR_SSL = 1;
        public const int SSL_ERROR_WANT_READ = 2;
        public const int SSL_ERROR_WANT_WRITE = 3;
        public const int SSL_ERROR_WANT_X509_LOOKUP = 4;
        public const int SSL_ERROR_SYSCALL = 5;
        public const int SSL_ERROR_ZERO_RETURN = 6;
        public const int SSL_ERROR_WANT_CONNECT = 7;
        public const int SSL_ERROR_WANT_ACCEPT = 8;

        // void ERR_error_string_n(unsigned long e, char *buf, size_t len);
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        //public static extern void ERR_error_string_n(ulong e, IntPtr buf, size_t len);
        public static extern void ERR_error_string_n(ulong e, byte[] buf, size_t len);

        // unsigned long ERR_get_error(void);
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong ERR_get_error();

        #endregion

        #region Utilities

         public static IntPtr ExpectNonNull(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                throw new Exception();

            return ptr;
        }

        #endregion
    }
}
