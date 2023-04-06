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

        //void OPENSSL_free(void* addr);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void OPENSSL_free(IntPtr addr);

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

        // int SSL_want_write(const SSL* ssl);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SSL_want_write(IntPtr ssl);

        // int SSL_write(SSL *ssl, const void *buf, int num);
        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SSL_write(IntPtr ssl, byte[] buf, int len);

        [DllImport(SSLDLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr TLS_client_method();

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

        // int BIO_should_retry(BIO *b);
        [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern int BIO_should_retry(IntPtr b);

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
        public static string StaticString(IntPtr ptr)
        {
            return Marshal.PtrToStringAnsi(ptr);
        }

        public static string PtrToStringAnsi(IntPtr ptr, bool hasOwnership)
        {
            var len = 0;
            for (var i = 0; i < 1024; i++, len++)
            {
                var octet = Marshal.ReadByte(ptr, i);
                if (octet == 0)
                    break;
            }

            if (len == 1024)
                return "Invalid string";

            var buf = new byte[len];
            Marshal.Copy(ptr, buf, 0, len);
            if (hasOwnership)
                Native.OPENSSL_free(ptr);

            return Encoding.ASCII.GetString(buf, 0, len);
        }

        public static IntPtr ExpectNonNull(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                throw new Exception();

            return ptr;
        }

        public static int ExpectSuccess(int ret)
        {
            if (ret <= 0)
                throw new Exception();

            return ret;
        }
        #endregion
    }
}
