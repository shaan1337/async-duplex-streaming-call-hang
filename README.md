# Reproduction steps

The following was tested on Ubuntu 18.04.

1. Install latest version of mono

```
$ mono --version
Mono JIT compiler version 6.12.0.107 (tarball Thu Dec 10 05:22:56 UTC 2020)
Copyright (C) 2002-2014 Novell, Inc, Xamarin Inc and Contributors. www.mono-project.com
        TLS:           __thread
        SIGSEGV:       altstack
        Notifications: epoll
        Architecture:  amd64
        Disabled:      none
        Misc:          softdebug 
        Interpreter:   yes
        LLVM:          yes(610)
        Suspend:       hybrid
        GC:            sgen (concurrent by default)
```

2. Install .NET 5 SDK 

```
$ dotnet --version
5.0.102
```

3. Run the application in this repository

```
$ dotnet run
[client] Request sent!
[server] Request sent!
[client] Got response!
Done!
[server] Got response!
```

The application hangs.