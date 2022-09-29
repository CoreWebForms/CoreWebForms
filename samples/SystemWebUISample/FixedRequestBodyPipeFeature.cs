// MIT License.

using System.IO.Pipelines;
using Microsoft.AspNetCore.Http.Features;

internal class FixedRequestBodyPipeFeature : IRequestBodyPipeFeature
{
    private readonly IHttpRequestFeature _other;
    private PipeReader? _reader;

    public FixedRequestBodyPipeFeature(IHttpRequestFeature other) => _other = other;

    public PipeReader Reader => _reader ??= PipeReader.Create(_other.Body, new StreamPipeReaderOptions(leaveOpen: true));
}
