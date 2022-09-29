﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;

namespace System.Web;

internal interface IHttpHandlerEndpointFactory
{
    Endpoint Create(IHttpHandler handler);
}
