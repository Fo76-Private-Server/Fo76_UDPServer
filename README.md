# Fo76_UDPServer
UDP Server which handles the DTLS communication

Currently does only the DTLS connection and sends a test packet. Adapted from WolfSSL [DTLS-PSK example](https://github.com/wolfSSL/wolfssl/tree/master/wrapper/CSharp/wolfSSL-DTLS-PSK-Server)


# Compiling the WolfSSL binaries

To compile the binaries yourself you have to clone and build the [wolfssl csharp wrapper](https://github.com/wolfSSL/wolfssl).

For the actual library these flags were used:

```
#define WOLFSSL_DTLS
#define NO_DH

#define DEBUG_WOLFSSL

#define NO_CERTS
#define NO_RSA
#define NO_ASN
#define HAVE_ENCRYPT_THEN_MAC
#define WOLFSSL_STATIC_PSK
#define WOLFSSL_AES_128
#define HAVE_AESGCM
```
