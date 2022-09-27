// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.ModelBinding;

using System.Collections.Generic;

public abstract class ModelValidatorProvider
{
    public abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ModelBindingExecutionContext context);
}
