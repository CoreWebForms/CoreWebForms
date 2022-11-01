// MIT License.

/**********************************************

Class hierarchy:

BaseCodeDomTreeGenerator
    BaseTemplateCodeDomTreeGenerator
        TemplateControlCodeDomTreeGenerator
            PageCodeDomTreeGenerator
            UserControlCodeDomTreeGenerator
        PageThemeCodeDomTreeGenerator
    ApplicationFileCodeDomTreeGenerator
***********************************************/

namespace System.Web.Compilation;

using System;

[Serializable]
public sealed class LinePragmaCodeInfo
{

    public LinePragmaCodeInfo()
    {
    }

    public LinePragmaCodeInfo(int startLine, int startColumn, int startGeneratedColumn, int codeLength, bool isCodeNugget)
    {
        this._startLine = startLine;
        this._startColumn = startColumn;
        this._startGeneratedColumn = startGeneratedColumn;
        this._codeLength = codeLength;
        this._isCodeNugget = isCodeNugget;
    }

    // Starting line in ASPX file
    internal int _startLine;

    public int StartLine { get { return _startLine; } }

    // Starting column in the ASPX file
    internal int _startColumn;

    public int StartColumn { get { return _startColumn; } }

    // Starting column in the generated source file (assuming no indentations are used)
    internal int _startGeneratedColumn;

    public int StartGeneratedColumn { get { return _startGeneratedColumn; } }

    // Length of the code snippet
    internal int _codeLength;

    public int CodeLength { get { return _codeLength; } }

    // Whether the script block is a nugget.
    internal bool _isCodeNugget;

    public bool IsCodeNugget { get { return _isCodeNugget; } }
}

