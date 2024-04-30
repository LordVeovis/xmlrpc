# Kveer.XmlRPC

[![CircleCI](https://circleci.com/gh/LordVeovis/xmlrpc.svg?style=shield)](https://app.circleci.com/pipelines/github/LordVeovis)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/a6cde759dac74c278c8a77cfbb4b968a)](https://app.codacy.com/gh/LordVeovis/xmlrpc/dashboard)
[![Build Status](https://gitlab.kveer.fr/veovis/xmlrpc/badges/master/pipeline.svg)](https://gitlab.kveer.fr/veovis/xmlrpc/pipelines)
[![Code Coverage](https://gitlab.kveer.fr/veovis/xmlrpc/badges/master/coverage.svg)](https://gitlab.kveer.fr/veovis/xmlrpc/pipelines)
[![Nuget Downloads](https://img.shields.io/nuget/dt/Kveer.XmlRpc.svg)](https://www.nuget.org/packages/Kveer.XmlRPC/)

This repository contains a .net library for consuming XML-RPC web services. It is a port of the Charles Cook's library mainly for .NET Core 2.x+ but target the netstandard 2.0, so it can also be used on the full .NET Framework 4.6.1+.

## How to use

As a port, very few changes have been done but the API is still the same. The documentation on the orignal projet remains valid: [http://xml-rpc.net/][]

## Improvements

* Can/Should we use Roslyn to generate the proxy implementation of the IXmlRpcProxy interface instead of using System.Reflection?

## Legal and Licensing

This library is licensed under the [MIT license][].

[MIT license]: https://github.com/LordVeovis/xmlrpc/blob/master/LICENSE
[http://xml-rpc.net/]: https://web.archive.org/web/20210909161907/http://xml-rpc.net/
